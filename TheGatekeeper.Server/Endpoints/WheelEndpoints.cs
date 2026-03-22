using Microsoft.AspNetCore.SignalR;
using TheGateKeeper.Server.WheelService;

namespace TheGateKeeper.Server.Endpoints
{
    public record WheelSpinRequest(string SelectedUser, string Result);
    public record WheelOptionsUpdateRequest(List<string> Options);
    public record WheelOpenRequest(string SelectedUser);

    public static class WheelEndpoints
    {
        public static IEndpointRouteBuilder MapWheelEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("api/wheel")
                .WithTags("Wheel");

            // Public: get the current wheel options
            group.MapGet("options", async (IWheelService wheelService) =>
            {
                var options = await wheelService.GetOptionsAsync();
                return Results.Ok(new { options });
            })
            .WithName("GetWheelOptions");

            // Admin-only: update wheel options
            group.MapPut("options", async (
                HttpRequest httpRequest,
                WheelOptionsUpdateRequest body,
                IWheelService wheelService) =>
            {
                if (!JwtHelper.HasRealmRole(httpRequest, "Admin"))
                    return Results.Forbid();

                await wheelService.SaveOptionsAsync(body.Options);
                return Results.NoContent();
            })
            .WithName("UpdateWheelOptions");

            // Authenticated: record a spin result and notify all subscribers
            group.MapPost("spin", async (
                HttpRequest httpRequest,
                WheelSpinRequest body,
                IWheelService wheelService) =>
            {
                if (JwtHelper.DecodePayload(httpRequest) is null)
                    return Results.Unauthorized();

                if (string.IsNullOrWhiteSpace(body.SelectedUser) || string.IsNullOrWhiteSpace(body.Result))
                    return Results.BadRequest(new { message = "SelectedUser and Result are required." });

                await wheelService.NotifySpinResultAsync(body.SelectedUser, body.Result);
                return Results.Ok();
            })
            .WithName("SpinWheel");

            // Admin-only: signal a specific user to open the spin wheel on their screen
            group.MapPost("open", async (
                HttpRequest httpRequest,
                WheelOpenRequest body,
                IHubContext<EventHub> hub) =>
            {
                if (!JwtHelper.HasRealmRole(httpRequest, "Admin"))
                    return Results.Forbid();

                if (string.IsNullOrWhiteSpace(body.SelectedUser))
                    return Results.BadRequest(new { message = "SelectedUser is required." });

                await hub.Clients.All.SendAsync("OpenWheelFor", body.SelectedUser);
                return Results.Ok();
            })
            .WithName("OpenWheelFor");

            return endpoints;
        }
    }
}
