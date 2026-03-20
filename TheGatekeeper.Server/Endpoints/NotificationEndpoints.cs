using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using TheGateKeeper.Server.InfrastructureService;

namespace TheGateKeeper.Server.Endpoints
{
    public record SubscribeRequest(string Endpoint, string P256DH, string Auth);
    public record BroadcastRequest(string Title, string Body);

    internal static class ClaimsPrincipalExtensions
    {
        /// <summary>Checks Keycloak realm_access.roles for a given role name.</summary>
        internal static bool HasKeycloakRealmRole(this ClaimsPrincipal user, string role)
        {
            var realmAccess = user.FindFirstValue("realm_access");
            if (string.IsNullOrWhiteSpace(realmAccess)) return false;
            try
            {
                var doc = JsonDocument.Parse(realmAccess);
                if (doc.RootElement.TryGetProperty("roles", out var roles))
                    foreach (var r in roles.EnumerateArray())
                        if (r.GetString() == role) return true;
            }
            catch { /* malformed claim — deny */ }
            return false;
        }
    }

    public static class NotificationEndpoints
    {
        public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("api/notifications")
                .WithTags("Notifications");

            // Public: frontend needs the key before the user logs in to subscribe
            group.MapGet("vapid-public-key", (IWebPushNotificationService pushService) =>
            {
                return Results.Ok(new { publicKey = pushService.GetVapidPublicKey() });
            })
            .WithName("GetVapidPublicKey")
            .AllowAnonymous();

            // Authenticated: save push subscription for the calling user
            group.MapPost("subscribe", async (
                [FromBody] SubscribeRequest request,
                ClaimsPrincipal user,
                IWebPushNotificationService pushService) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? user.FindFirstValue("sub");
                if (string.IsNullOrWhiteSpace(userId))
                    return Results.Unauthorized();

                if (string.IsNullOrWhiteSpace(request.Endpoint)
                    || string.IsNullOrWhiteSpace(request.P256DH)
                    || string.IsNullOrWhiteSpace(request.Auth))
                    return Results.BadRequest("Invalid subscription data.");

                await pushService.SaveSubscriptionAsync(userId, request.Endpoint, request.P256DH, request.Auth);
                return Results.Created("/api/notifications/subscribe", null);
            })
            .WithName("Subscribe")
            .RequireAuthorization();

            // Authenticated: remove push subscription for the calling user
            group.MapDelete("unsubscribe", async (
                ClaimsPrincipal user,
                IWebPushNotificationService pushService) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? user.FindFirstValue("sub");
                if (string.IsNullOrWhiteSpace(userId))
                    return Results.Unauthorized();

                await pushService.RemoveSubscriptionAsync(userId);
                return Results.Ok();
            })
            .WithName("Unsubscribe")
            .RequireAuthorization();

            // Admin: send a test notification to the calling user
            group.MapPost("test", async (
                ClaimsPrincipal user,
                IWebPushNotificationService pushService) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? user.FindFirstValue("sub");
                if (string.IsNullOrWhiteSpace(userId))
                    return Results.Unauthorized();

                await pushService.SendNotificationToUserAsync(
                    userId,
                    "The GateKeeper",
                    "\uD83D\uDD14 Push notifications are working!");
                return Results.Ok();
            })
            .WithName("TestNotification")
            .RequireAuthorization();

            // Admin: broadcast a custom push notification to all subscribers
            group.MapPost("broadcast", async (
                [FromBody] BroadcastRequest request,
                ClaimsPrincipal user,
                IWebPushNotificationService pushService) =>
            {
                if (!user.HasKeycloakRealmRole("Admin"))
                    return Results.Forbid();

                if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Body))
                    return Results.BadRequest("Title and Body are required.");

                await pushService.SendNotificationToAllAsync(request.Title, request.Body);
                return Results.Ok();
            })
            .WithName("BroadcastNotification")
            .RequireAuthorization();

            return endpoints;
        }
    }
}
