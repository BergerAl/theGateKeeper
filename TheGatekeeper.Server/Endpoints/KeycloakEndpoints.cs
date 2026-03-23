using MongoDB.Driver;
using System.Net.Http.Headers;
using System.Text.Json;
using TheGateKeeper.Server;

namespace TheGateKeeper.Server.Endpoints
{
    public record KeycloakUserDto(string Username, string? FirstName, string? LastName);

    public static class KeycloakEndpoints
    {
        public static IEndpointRouteBuilder MapKeycloakEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("api/keycloak")
                .WithTags("Keycloak");

            // Authenticated: list all registered users from Keycloak
            group.MapGet("users", async (
                HttpRequest httpRequest,
                IConfiguration configuration,
                IHttpClientFactory httpClientFactory) =>
            {
                if (JwtHelper.DecodePayload(httpRequest) is null)
                    return Results.Unauthorized();

                // Get an admin token using client credentials
                // Use internal Docker network hostname for container-to-container calls
                var keycloakUrl = configuration["Keycloak:auth-server-url"]?.TrimEnd('/')
                    ?? "http://keycloak:8080";
                var realm = configuration["Keycloak:realm"] ?? "thegatekeeper";
                var adminUser = SecretsHelper.GetSecret(configuration, "keycloak_admin");
                var adminPassword = SecretsHelper.GetSecret(configuration, "keycloak_admin_password");

                var client = httpClientFactory.CreateClient();

                // Authenticate as admin
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
                    return Results.Problem("Could not authenticate with Keycloak admin.");

                var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
                var tokenDoc = JsonDocument.Parse(tokenJson);
                var accessToken = tokenDoc.RootElement.GetProperty("access_token").GetString();

                // Fetch users from the realm
                var usersRequest = new HttpRequestMessage(HttpMethod.Get,
                    $"{keycloakUrl}/admin/realms/{realm}/users?max=500");
                usersRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var usersResponse = await client.SendAsync(usersRequest);
                if (!usersResponse.IsSuccessStatusCode)
                    return Results.Problem("Could not fetch users from Keycloak.");

                var usersJson = await usersResponse.Content.ReadAsStringAsync();
                var usersDoc = JsonDocument.Parse(usersJson);

                var users = usersDoc.RootElement.EnumerateArray()
                    .Where(u => !u.TryGetProperty("enabled", out var enabled) || enabled.GetBoolean())
                    .Select(u => new KeycloakUserDto(
                        Username: u.TryGetProperty("username", out var username) ? username.GetString() ?? "" : "",
                        FirstName: u.TryGetProperty("firstName", out var fn) ? fn.GetString() : null,
                        LastName: u.TryGetProperty("lastName", out var ln) ? ln.GetString() : null
                    )).ToList();

                return Results.Ok(users);
            })
            .WithName("GetKeycloakUsers");

            // Authenticated: get vote counts for all Keycloak users
            group.MapGet("userVotes", async (
                HttpRequest httpRequest,
                IMongoClient mongoClient) =>
            {
                if (JwtHelper.DecodePayload(httpRequest) is null)
                    return Results.Unauthorized();

                var collection = mongoClient.GetDatabase("gateKeeper")
                    .GetCollection<KeycloakUserVoteDaoV1>("keycloakUserVotes");
                var votes = await collection.Find(_ => true).ToListAsync();
                var result = votes.Select(v => new { username = v.Username, votes = v.VoteCount });
                return Results.Ok(result);
            })
            .WithName("GetKeycloakUserVotes");

            // Authenticated: vote for a Keycloak user
            group.MapPost("users/{username}/vote", async (
                string username,
                HttpRequest httpRequest,
                IMongoClient mongoClient) =>
            {
                if (JwtHelper.DecodePayload(httpRequest) is null)
                    return Results.Unauthorized();

                var collection = mongoClient.GetDatabase("gateKeeper")
                    .GetCollection<KeycloakUserVoteDaoV1>("keycloakUserVotes");

                var filter = Builders<KeycloakUserVoteDaoV1>.Filter.Eq(d => d.Username, username);
                var existing = await collection.Find(filter).FirstOrDefaultAsync();

                if (existing == null)
                {
                    await collection.InsertOneAsync(new KeycloakUserVoteDaoV1
                    {
                        Username = username,
                        VoteCount = 1,
                        IsBlocked = true,
                        VoteBlockedUntil = DateTime.UtcNow.AddSeconds(0.5)
                    });
                    return Results.Ok();
                }

                if (existing.IsBlocked && existing.VoteBlockedUntil > DateTime.UtcNow)
                    return Results.BadRequest(new { message = $"Voting for {username} is currently blocked." });

                var update = Builders<KeycloakUserVoteDaoV1>.Update
                    .Inc(d => d.VoteCount, 1)
                    .Set(d => d.IsBlocked, true)
                    .Set(d => d.VoteBlockedUntil, DateTime.UtcNow.AddSeconds(0.5));
                await collection.UpdateOneAsync(filter, update);
                return Results.Ok();
            })
            .WithName("VoteForKeycloakUser");

            // Admin-only: reset all Keycloak user votes to 0
            group.MapPost("userVotes/reset", async (
                HttpRequest httpRequest,
                IMongoClient mongoClient) =>
            {
                if (!JwtHelper.HasRealmRole(httpRequest, "Admin"))
                    return Results.Forbid();

                var collection = mongoClient.GetDatabase("gateKeeper")
                    .GetCollection<KeycloakUserVoteDaoV1>("keycloakUserVotes");
                var update = Builders<KeycloakUserVoteDaoV1>.Update
                    .Set(d => d.VoteCount, 0)
                    .Set(d => d.IsBlocked, false);
                await collection.UpdateManyAsync(_ => true, update);
                return Results.Ok();
            })
            .WithName("ResetKeycloakUserVotes");

            return endpoints;
        }
    }
}
