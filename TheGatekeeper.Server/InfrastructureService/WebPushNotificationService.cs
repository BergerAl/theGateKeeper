using MongoDB.Driver;
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
        private readonly string _vapidPublicKey;
        private readonly string _vapidPrivateKey;
        private readonly string _vapidSubject;

        public WebPushNotificationService(
            IMongoClient mongoClient,
            ILogger<WebPushNotificationService> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            var database = mongoClient.GetDatabase("gateKeeper");
            _subscriptions = database.GetCollection<PushSubscriptionDaoV1>("pushSubscriptions");

            _vapidPublicKey = SecretsHelper.GetSecret(configuration, "vapid_public_key");
            _vapidPrivateKey = SecretsHelper.GetSecret(configuration, "vapid_private_key");
            _vapidSubject = SecretsHelper.GetSecret(configuration, "vapid_subject");
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
            var allSubs = await _subscriptions.Find(_ => true).ToListAsync();
            var tasks = allSubs.Select(sub => SendToSubscriptionAsync(sub, title, body, icon));
            await Task.WhenAll(tasks);
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
                // Subscription is no longer valid — remove it
                _logger.LogInformation("Push subscription for user {UserId} expired, removing.", sub.UserId);
                await RemoveSubscriptionAsync(sub.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send push notification to user {UserId}", sub.UserId);
            }
        }
    }
}
