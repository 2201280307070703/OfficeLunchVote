using Microsoft.AspNetCore.Mvc;
using OfficeLunchVote.Grains.Interfaces;

namespace OfficeLunchVote.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TimeController : ControllerBase
    {
        private readonly IGrainFactory _grainFactory;

        public TimeController(IGrainFactory grainFactory)
        {
            _grainFactory = grainFactory;
        }

        [HttpGet("dateTime")]
        public async Task<IActionResult> GetCurrentTime()
        {
            var timeGrain = _grainFactory.GetGrain<ITimeGrain>(0);
            var currentTime = await timeGrain.GetCurrentTimeAsync();
            return Ok(currentTime);
        }

        [HttpPost("dateTime")]
        public async Task<IActionResult> SetServerTime([FromQuery] string user, [FromBody] DateTime newDate)
        {
            if (user != "clock")
            {
                return BadRequest("Only 'clock' user can set the date.");
            }

            var timeGrain = _grainFactory.GetGrain<ITimeGrain>(0);
            await timeGrain.SetCurrentTimeAsync(newDate);

            return Ok(newDate);
        }
    }
}
