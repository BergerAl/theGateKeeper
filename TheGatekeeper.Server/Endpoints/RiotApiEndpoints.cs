using Microsoft.AspNetCore.Mvc;
using TheGateKeeper.Server.RiotsApiService;
using TheGateKeeper.Server.VotingService;

namespace TheGateKeeper.Server.Endpoints
{
    public static class RiotApiEndpoints
    {
        public static IEndpointRouteBuilder MapRiotApiEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("api/TheGateKeeper")
                .WithTags("TheGateKeeper");

            group.MapGet("getCurrentRanks", async (IRiotApi riotApi) =>
            {
                var ranks = await riotApi.GetAllRanks();
                return Results.Ok(ranks);
            })
            .WithName("GetCurrentRanks")
            .Produces<IEnumerable<FrontEndInfo>>();

            group.MapGet("getCurrentVoteStandings", async (IVotingService votingService) =>
            {
                var standings = await votingService.GetVoteStandings();
                return Results.Ok(standings);
            })
            .WithName("GetCurrentVoteStandings")
            .Produces<IEnumerable<VoteStandingsDtoV1>>();

            group.MapGet("getHistory", async ([FromQuery] string userName, IRiotApi riotApi) =>
            {
                var history = await riotApi.GetHistory(userName);
                return Results.Ok(history);
            })
            .WithName("GetHistory")
            .Produces<RankTimeLineEntryDaoV1>();

            group.MapPost("voteForUser", async ([FromBody] string userName, IVotingService votingService) =>
            {
                if (userName is null)
                {
                    return Results.BadRequest(new { message = "No userName provided" });
                }
                try
                {
                    var result = await votingService.VoteForUser(userName);
                    if (result.Success)
                    {
                        return Results.Ok();
                    }
                    return Results.BadRequest(new { message = result.ErrorMessage });
                }
                catch (Exception e)
                {
                    return Results.BadRequest(new { message = $"Couldn't vote for user because of exception: {e}" });
                }
            })
            .WithName("VoteForUser");

            group.MapPost("apiKey", ([FromBody] string apiKey, IRiotApi riotApi) =>
            {
                try
                {
                    if (riotApi.SetNewApiKey(apiKey))
                    {
                        return Results.Ok($"New api key set");
                    }
                    return Results.BadRequest(new { message = "New api key not set" });
                }
                catch (Exception e)
                {
                    return Results.BadRequest(new { message = $"Couldn't set new api, because of exception: {e}" });
                }
            })
            .WithName("AddNewApiKey");

            return endpoints;
        }
    }
}