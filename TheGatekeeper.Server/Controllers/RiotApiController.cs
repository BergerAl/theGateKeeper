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

        //[HttpGet("allAvailableCycles")]
        //public Task<IEnumerable<string>> GetAllAvailableCycles()
        //{
        //    return _workpieceProviderService.GetAllAvailableCycles();
        //}

        //[HttpGet("allAvailableWorkpieces")]
        //public Task<IEnumerable<WorkpieceListStructure>> GetAllAvailableWorkpieces()
        //{
        //    return _workpieceProviderService.GetAllAvailableWorkpieces();
        //}

        //[HttpGet("allAvailableDressers")]
        //public Task<IEnumerable<Dresser>> GetAllAvailableDressers()
        //{
        //    return _workpieceProviderService.GetAllDressers();
        //}

        //[HttpGet("workpiece/{workpieceDirectory}/{workpieceName}")]
        //public Task<Workpiece> GetWorkpiece(string workpieceDirectory, string workpieceName)
        //{
        //    return _workpieceProviderService.GetWorkpieceByName($"{workpieceDirectory}/{workpieceName}");
        //}

        //[HttpPost("saveWorkpiece")]
        //public async Task<IActionResult> SaveWorkpiece([FromQuery] string workpiecePath, [FromBody] Workpiece workpiece)
        //{
        //    // Validate the input
        //    if (workpiece is null)
        //    {
        //        return BadRequest(new { message = "No workpiece provided" });
        //    }
        //    try
        //    {
        //        var result = await _workpieceProviderService.SaveWorkpiece(workpiecePath, workpiece).ConfigureAwait(false);
        //        if (result.Success)
        //        {
        //            return Ok(new { message = $"Workpiece saved successfully", success = true });
        //        }
        //        else
        //        {
        //            return Ok(new { message = $"Saving workpiece failed with error: {result.ErrorMessage}", success = false });
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        return BadRequest(new { message = $"Couldn't save the workpiece because of exception: {e}" });
        //    }

        //}

        //[HttpPost("addWorkpiece")]
        //public async Task<IActionResult> AddWorkpiece([FromQuery] string workpieceDirectory, [FromBody] string workpieceName)
        //{
        //    // Validate the input
        //    if (workpieceName is null)
        //    {
        //        return BadRequest(new { message = "No workpiece name provided" });
        //    }
        //    try
        //    {
        //        var result = await _workpieceProviderService.AddWorkpiece(workpieceDirectory, workpieceName).ConfigureAwait(false);
        //        if (result.Success)
        //        {
        //            return Ok(new { message = $"Workpiece saved successfully", success = true });
        //        }
        //        else
        //        {
        //            return Ok(new { message = $"Saving workpiece failed with error: {result.ErrorMessage}", success = false });
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        return BadRequest(new { message = $"Couldn't save the workpiece because of exception: {e}" });
        //    }

        //}
    }
}
