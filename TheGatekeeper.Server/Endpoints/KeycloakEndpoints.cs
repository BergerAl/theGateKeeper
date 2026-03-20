using System.Net.Http.Headers;
using System.Text.Json;

namespace TheGateKeeper.Server.Endpoints
{
    public record KeycloakUserDto(string Username, string? FirstName, string? LastName);

    public static class KeycloakEndpoints
    {
        public static IEndpointRouteBuilder MapKeycloakEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("api/keycloak")
                .WithTags("Keycloak");

            // Admin-only: list all registered users from Keycloak
            group.MapGet("users", async (
                HttpRequest httpRequest,
                IConfiguration configuration,
                IHttpClientFactory httpClientFactory) =>
            {
                if (!JwtHelper.HasRealmRole(httpRequest, "Admin"))
                    return Results.Forbid();

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

                var users = usersDoc.RootElement.EnumerateArray().Select(u => new KeycloakUserDto(
                    Username: u.TryGetProperty("username", out var username) ? username.GetString() ?? "" : "",
                    FirstName: u.TryGetProperty("firstName", out var fn) ? fn.GetString() : null,
                    LastName: u.TryGetProperty("lastName", out var ln) ? ln.GetString() : null
                )).ToList();

                return Results.Ok(users);
            })
            .WithName("GetKeycloakUsers");

            return endpoints;
        }
    }
}
