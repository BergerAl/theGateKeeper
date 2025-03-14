using Microsoft.AspNetCore.Mvc;
using TheGateKeeper.Server;
using TheGateKeeper.Server.RiotsApiService;

namespace TheGateKeeper.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TheGateKeeper(IRiotApi riotApi) : ControllerBase
    {
        private readonly IRiotApi _riotApi = riotApi;

        [HttpGet("getCurrentRanks")]
        public Task<IEnumerable<FrontEndInfo>> GetAllAvailableCycles()
        {
            return _riotApi.GetAllRanks();
        }

    }
}
