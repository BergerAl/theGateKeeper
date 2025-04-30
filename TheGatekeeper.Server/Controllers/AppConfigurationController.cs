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
            return Ok(new FrontendAppConfigurationDaoV1
            {
                DisplayedView = config.DisplayedView
            });
        }

        [HttpPut]
        public async Task<ActionResult> UpdateConfiguration([FromBody] FrontendAppConfigurationDaoV1 config)
        {
            await _appControl.UpdateConfigurationAsync(new AppConfigurationDtoV1() { DisplayedView = config.DisplayedView});
            return Ok();
        }
    }
}
