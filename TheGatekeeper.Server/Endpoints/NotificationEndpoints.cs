using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TheGateKeeper.Server.InfrastructureService;

namespace TheGateKeeper.Server.Endpoints
{
    public record SubscribeRequest(string Endpoint, string P256DH, string Auth);
    public record BroadcastRequest(string Title, string Body);

    internal static class JwtHelper
    {
        /// <summary>
        /// Manually decodes the JWT payload from the Authorization header.
        /// Used because Keycloak audience validation may prevent middleware from populating ClaimsPrincipal.
        /// </summary>
        internal static JsonElement? DecodePayload(HttpRequest request)
        {
            var authHeader = request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
                return null;

            var token = authHeader["Bearer ".Length..];
            var parts = token.Split('.');
            if (parts.Length != 3) return null;

            try
            {
                var padding = (4 - parts[1].Length % 4) % 4;
                var base64 = parts[1].Replace('-', '+').Replace('_', '/') + new string('=', padding);
                var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64));
                return JsonDocument.Parse(json).RootElement.Clone();
            }
            catch
            {
                return null;
            }
        }

        internal static string? GetSubject(HttpRequest request)
        {
            var payload = DecodePayload(request);
            if (payload is null) return null;
            return payload.Value.TryGetProperty("sub", out var sub) ? sub.GetString() : null;
        }

        internal static bool HasRealmRole(HttpRequest request, string role)
        {
            var payload = DecodePayload(request);
            if (payload is null) return false;
            try
            {
                // Check realm_access.roles for a custom role (e.g. "Admin")
                if (payload.Value.TryGetProperty("realm_access", out var realmAccess)
                    && realmAccess.TryGetProperty("roles", out var realmRoles))
                {
                    foreach (var r in realmRoles.EnumerateArray())
                        if (r.GetString() == role) return true;
                }

                // Check resource_access['realm-management'].roles for built-in "realm-admin"
                if (payload.Value.TryGetProperty("resource_access", out var resourceAccess)
                    && resourceAccess.TryGetProperty("realm-management", out var realmMgmt)
                    && realmMgmt.TryGetProperty("roles", out var mgmtRoles))
                {
                    foreach (var r in mgmtRoles.EnumerateArray())
                        if (r.GetString() == "realm-admin") return true;
                }
            }
            catch { /* malformed */ }
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
            .WithName("GetVapidPublicKey");

            // Authenticated: save push subscription for the calling user
            group.MapPost("subscribe", async (
                [FromBody] SubscribeRequest request,
                HttpRequest httpRequest,
                IWebPushNotificationService pushService) =>
            {
                var userId = JwtHelper.GetSubject(httpRequest);
                if (string.IsNullOrWhiteSpace(userId))
                    return Results.Unauthorized();

                if (string.IsNullOrWhiteSpace(request.Endpoint)
                    || string.IsNullOrWhiteSpace(request.P256DH)
                    || string.IsNullOrWhiteSpace(request.Auth))
                    return Results.BadRequest("Invalid subscription data.");

                await pushService.SaveSubscriptionAsync(userId, request.Endpoint, request.P256DH, request.Auth);
                return Results.Created("/api/notifications/subscribe", null);
            })
            .WithName("Subscribe");

            // Authenticated: remove push subscription for the calling user
            group.MapDelete("unsubscribe", async (
                HttpRequest httpRequest,
                IWebPushNotificationService pushService) =>
            {
                var userId = JwtHelper.GetSubject(httpRequest);
                if (string.IsNullOrWhiteSpace(userId))
                    return Results.Unauthorized();

                await pushService.RemoveSubscriptionAsync(userId);
                return Results.Ok();
            })
            .WithName("Unsubscribe");

            // Admin: broadcast a custom push notification to all subscribers
            group.MapPost("broadcast", async (
                [FromBody] BroadcastRequest request,
                HttpRequest httpRequest,
                IWebPushNotificationService pushService) =>
            {
                if (!JwtHelper.HasRealmRole(httpRequest, "Admin"))
                    return Results.Forbid();

                if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Body))
                    return Results.BadRequest("Title and Body are required.");

                await pushService.SendNotificationToAllAsync(request.Title, request.Body);
                return Results.Ok();
            })
            .WithName("BroadcastNotification");

            return endpoints;
        }
    }
}
