using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheGateKeeper.Server;
using TheGateKeeper.Server.AppControl;

namespace TheGateKeeper.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppConfiguration(IAppControl appControl) : ControllerBase
    {
        private readonly IAppControl _appControl = appControl;


        [HttpGet("getConfiguration")]
        public async Task<ActionResult> GetAppConfiguration()
        {
            var config = await _appControl.GetConfigurationAsync();
            return Ok(config);
        }

        [HttpGet("getGateKeeperInfo")]
        public async Task<ActionResult> GetGateKeeperInformation()
        {
            var config = await _appControl.GetGateKeeperInformation();
            return Ok(config);
        }

        [Authorize]
        [HttpPut]
        public async Task<ActionResult> UpdateConfiguration([FromBody] AppConfigurationDtoV1 config)
        {
            await _appControl.UpdateConfigurationAsync(config);
            return Ok();
        }
    }
}
