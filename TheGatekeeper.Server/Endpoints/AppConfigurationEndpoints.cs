using Microsoft.AspNetCore.Mvc;
using TheGateKeeper.Server;
using TheGateKeeper.Server.AppControl;

namespace TheGateKeeper.Server.Endpoints
{
    public static class AppConfigurationEndpoints
    {
        public static IEndpointRouteBuilder MapAppConfigurationEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("api/AppConfiguration")
                .WithTags("AppConfiguration");

            group.MapGet("getConfiguration", async (IAppControl appControl) =>
            {
                var config = await appControl.GetConfigurationAsync();
                return Results.Ok(config);
            })
            .WithName("GetAppConfiguration")
            .Produces<AppConfigurationDtoV1>();

            group.MapGet("getGateKeeperInfo", async (IAppControl appControl) =>
            {
                var config = await appControl.GetGateKeeperInformation();
                return Results.Ok(config);
            })
            .WithName("GetGateKeeperInformation")
            .Produces<GateKeeperInformationDtoV1>();

            group.MapPut("", async ([FromBody] AppConfigurationDtoV1 config, IAppControl appControl) =>
            {
                await appControl.UpdateConfigurationAsync(config);
                return Results.Ok();
            })
            .WithName("UpdateConfiguration");

            return endpoints;
        }
    }
}