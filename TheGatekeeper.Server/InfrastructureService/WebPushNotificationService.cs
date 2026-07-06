using MongoDB.Driver;
using System.Net.Http.Headers;
using System.Text.Json;
using WebPush;

namespace TheGateKeeper.Server.InfrastructureService
{
    public interface IWebPushNotificationService
    {
        Task SaveSubscriptionAsync(string userId, string endpoint, string p256dh, string auth);
        Task RemoveSubscriptionAsync(string userId);
        Task SendNotificationToAllAsync(string title, string body, string? icon = null);
        Task SendNotificationToUserAsync(string userId, string title, string body, string? icon = null);
        string GetVapidPublicKey();
    }

    public class WebPushNotificationService : IWebPushNotificationService
    {
        private readonly IMongoCollection<PushSubscriptionDaoV1> _subscriptions;
        private readonly ILogger<WebPushNotificationService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly string _vapidPublicKey;
        private readonly string _vapidPrivateKey;
        private readonly string _vapidSubject;

        public WebPushNotificationService(
            IMongoClient mongoClient,
            ILogger<WebPushNotificationService> logger,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            var database = mongoClient.GetDatabase("gateKeeper");
            _subscriptions = database.GetCollection<PushSubscriptionDaoV1>("pushSubscriptions");

            _vapidPublicKey = SecretsHelper.GetSecret(configuration, "vapid_public_key").Trim();
            _vapidPrivateKey = SecretsHelper.GetSecret(configuration, "vapid_private_key").Trim();
            _vapidSubject = SecretsHelper.GetSecret(configuration, "vapid_subject").Trim();
        }

        public string GetVapidPublicKey() => _vapidPublicKey;

        public async Task SaveSubscriptionAsync(string userId, string endpoint, string p256dh, string auth)
        {
            // One subscription per user — upsert by userId
            var filter = Builders<PushSubscriptionDaoV1>.Filter.Eq(s => s.UserId, userId);
            var replacement = new PushSubscriptionDaoV1
            {
                UserId = userId,
                Endpoint = endpoint,
                P256DH = p256dh,
                Auth = auth,
                CreatedAt = DateTime.UtcNow
            };
            var options = new ReplaceOptions { IsUpsert = true };
            await _subscriptions.ReplaceOneAsync(filter, replacement, options);
        }

        public async Task RemoveSubscriptionAsync(string userId)
        {
            var filter = Builders<PushSubscriptionDaoV1>.Filter.Eq(s => s.UserId, userId);
            await _subscriptions.DeleteOneAsync(filter);
        }

        public async Task SendNotificationToAllAsync(string title, string body, string? icon = null)
        {
            var enabledUserIds = await GetEnabledKeycloakUserIdsAsync();
            var allSubs = await _subscriptions.Find(_ => true).ToListAsync();
            var filteredSubs = enabledUserIds is not null
                ? allSubs.Where(s => enabledUserIds.Contains(s.UserId)).ToList()
                : allSubs;
            var tasks = filteredSubs.Select(sub => SendToSubscriptionAsync(sub, title, body, icon));
            await Task.WhenAll(tasks);
        }

        private async Task<HashSet<string>?> GetEnabledKeycloakUserIdsAsync()
        {
            try
            {
                var keycloakUrl = _configuration["Keycloak:auth-server-url"]?.TrimEnd('/') ?? "http://keycloak:8080";
                var realm = _configuration["Keycloak:realm"] ?? "thegatekeeper";
                var adminUser = SecretsHelper.GetSecret(_configuration, "keycloak_admin");
                var adminPassword = SecretsHelper.GetSecret(_configuration, "keycloak_admin_password");

                var client = _httpClientFactory.CreateClient();

                var tokenResponse = await client.PostAsync(
                    $"{keycloakUrl}/realms/master/protocol/openid-connect/token",
                    new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        ["grant_type"] = "password",
                        ["client_id"] = "admin-cli",
                        ["username"] = adminUser,
                        ["password"] = adminPassword,
                    }));

                if (!tokenResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Could not authenticate with Keycloak admin — sending to all subscribers.");
                    return null;
                }

                var tokenDoc = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync());
                var accessToken = tokenDoc.RootElement.GetProperty("access_token").GetString();

                var usersRequest = new HttpRequestMessage(HttpMethod.Get,
                    $"{keycloakUrl}/admin/realms/{realm}/users?max=500");
                usersRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var usersResponse = await client.SendAsync(usersRequest);
                if (!usersResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Could not fetch users from Keycloak — sending to all subscribers.");
                    return null;
                }

                var usersDoc = JsonDocument.Parse(await usersResponse.Content.ReadAsStringAsync());
                return usersDoc.RootElement.EnumerateArray()
                    .Where(u => u.TryGetProperty("enabled", out var enabled) && enabled.GetBoolean())
                    .Select(u => u.TryGetProperty("id", out var id) ? id.GetString() : null)
                    .Where(id => id is not null)
                    .ToHashSet()!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch enabled Keycloak users — sending to all subscribers.");
                return null;
            }
        }

        public async Task SendNotificationToUserAsync(string userId, string title, string body, string? icon = null)
        {
            var filter = Builders<PushSubscriptionDaoV1>.Filter.Eq(s => s.UserId, userId);
            var sub = await _subscriptions.Find(filter).FirstOrDefaultAsync();
            if (sub is null) return;
            await SendToSubscriptionAsync(sub, title, body, icon);
        }

        private async Task SendToSubscriptionAsync(PushSubscriptionDaoV1 sub, string title, string body, string? icon)
        {
            try
            {
                var webPushClient = new WebPushClient();
                webPushClient.SetVapidDetails(_vapidSubject, _vapidPublicKey, _vapidPrivateKey);

                var subscription = new PushSubscription(sub.Endpoint, sub.P256DH, sub.Auth);
                var payload = System.Text.Json.JsonSerializer.Serialize(new
                {
                    title,
                    body,
                    icon = icon ?? "/images/clown.png"
                });

                await webPushClient.SendNotificationAsync(subscription, payload);
            }
            catch (WebPushException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Gone
                                           || ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Subscription is no longer valid — attempt cleanup but don't crash if permissions are missing
                _logger.LogInformation("Push subscription for user {UserId} expired, removing.", sub.UserId);
                try { await RemoveSubscriptionAsync(sub.UserId); }
                catch (Exception cleanupEx) { _logger.LogWarning(cleanupEx, "Could not remove expired subscription for user {UserId}.", sub.UserId); }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send push notification to user {UserId}", sub.UserId);
            }
        }
    }
}
