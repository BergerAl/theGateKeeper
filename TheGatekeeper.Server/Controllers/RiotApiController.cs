using Microsoft.AspNetCore.Mvc;
using TheGateKeeper.Server;
using TheGateKeeper.Server.RiotsApiService;
using TheGateKeeper.Server.VotingService;

namespace TheGateKeeper.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TheGateKeeper(IRiotApi riotApi, IVotingService votingService) : ControllerBase
    {
        private readonly IRiotApi _riotApi = riotApi;
        private readonly IVotingService _voteService = votingService;

        [HttpGet("getCurrentRanks")]
        public Task<IEnumerable<FrontEndInfo>> GetAllRanks()
        {
            return _riotApi.GetAllRanks();
        }

        [HttpGet("getCurrentVoteStandings")]
        public async Task<IEnumerable<VoteStandingsDtoV1>> GetCurrentVoteStandings()
        {
            return await _voteService.GetVoteStandings();
        }

        [HttpPost("voteForUser")]
        public async Task<IActionResult> VoteForUser([FromBody] string userName)
        {
            // Validate the input
            if (userName is null)
            {
                return BadRequest(new { message = "No userName provided" });
            }
            try
            {
                var result = await _voteService.VoteForUser(userName);
                if (result.Success) {
                    return Ok();
                }
                return BadRequest(new { message = result.ErrorMessage });
            }
            catch (Exception e)
            {
                return BadRequest(new { message = $"Couldn't vote for user because of exception: {e}" });
            }

        }

        [HttpPost("apiKey")]
        public IActionResult AddNewApiKey([FromBody] string apiKey)
        {
            try
            {
                if (_riotApi.SetNewApiKey(apiKey))
                {
                    return Ok($"New api key set");
                }
                return BadRequest(new { message = "New api key not set" });
            }
            catch (Exception e)
            {

                return BadRequest(new { message = $"Couldn't set new api, because of exception: {e}" });
            }          
        }
    }
}
