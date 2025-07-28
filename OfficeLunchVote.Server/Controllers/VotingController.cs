using Microsoft.AspNetCore.Mvc;
using OfficeLunchVote.Grains.Interfaces;

namespace OfficeLunchVote.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class VotingController : ControllerBase
    {
        private readonly IGrainFactory _grainFactory;

        public VotingController(IGrainFactory grainFactory)
        {
            _grainFactory = grainFactory;
        }

        private async Task<DateTime> GetCurrentServerTimeAsync()
        {
            var timeGrain = _grainFactory.GetGrain<ITimeGrain>(0);
            return await timeGrain.GetCurrentTimeAsync();
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateVote([FromQuery] string user)
        {
            if (user.ToLower() == "clock")
            {
                return BadRequest("User 'clock' cannot create vote.");
            }

            var now = await GetCurrentServerTimeAsync();
            var creationDate = DateOnly.FromDateTime(now);
            var votingGrain = _grainFactory.GetGrain<IVotingGrain>(creationDate.ToString());

            var isAlreadyCreated = await votingGrain.CreateVoteAsync(user, creationDate);
            if (isAlreadyCreated)
            {
                return BadRequest($"Vote for this date - {creationDate} already exists.");
            }

            return Ok(true);
        }

        [HttpPost("vote")]
        public async Task<IActionResult> Vote([FromQuery] string user, string place)
        {
            if (string.IsNullOrWhiteSpace(user))
                return BadRequest("User is required.");

            if (user.ToLower() == "clock")
                return BadRequest("User 'clock' cannot vote.");

            if (string.IsNullOrWhiteSpace(place))
                return BadRequest("Place is required.");

            var now = await GetCurrentServerTimeAsync();
            var votingDate = DateOnly.FromDateTime(now);
            var votingGrain = _grainFactory.GetGrain<IVotingGrain>(votingDate.ToString());

            var isVotingClosed = await votingGrain.IsVotingClosedAsync();
            if (isVotingClosed)
            {
                return BadRequest("Voting is closed.");
            }

            var isUserAlreadyVoted = await votingGrain.HasUserVotedAsync(user);
            if (isUserAlreadyVoted)
            {
                return BadRequest("You have already voted.");
            }

            var successfulVoting = await votingGrain.VoteAsync(user, place);
            if (!successfulVoting)
            {
                return BadRequest("Invalid vote.");
            }

            return Ok();
        }

        [HttpGet("results")]
        public async Task<IActionResult> Results()
        {
            var now = await GetCurrentServerTimeAsync();
            var date = DateOnly.FromDateTime(now);
            var grain = _grainFactory.GetGrain<IVotingGrain>(date.ToString());

            var closed = await grain.IsVotingClosedAsync();
            var closeTime = await grain.GetClosingTimeAsync();

            if (!closed || now > closeTime.AddHours(2))
                return BadRequest("Results not available.");

            var results = await grain.GetResultsAsync();
            return Ok(results);
        }

        [HttpGet("isVotingClosed")]
        public async Task<IActionResult> IsVotingClosed()
        {
            var now = await GetCurrentServerTimeAsync();
            var date = DateOnly.FromDateTime(now);
            var grain = _grainFactory.GetGrain<IVotingGrain>(date.ToString());

            bool isVotingClosed = await grain.IsVotingClosedAsync();
            return Ok(isVotingClosed);
        }

        [HttpGet("isUserAlreadyVoted")]
        public async Task<IActionResult> IsUserAlreadyVoted([FromQuery] string user)
        {
            if (string.IsNullOrWhiteSpace(user))
            {
                return BadRequest("User is required.");
            }

            var now = await GetCurrentServerTimeAsync();
            var date = DateOnly.FromDateTime(now);
            var grain = _grainFactory.GetGrain<IVotingGrain>(date.ToString());

            bool userAlreadyVoted = await grain.HasUserVotedAsync(user);
            return Ok(userAlreadyVoted);
        }

        [HttpGet("places")]
        public async Task<IActionResult> Places()
        {
            var now = await GetCurrentServerTimeAsync();
            var date = DateOnly.FromDateTime(now);
            var grain = _grainFactory.GetGrain<IVotingGrain>(date.ToString());

            var places = await grain.GetPlacesAsync();
            return Ok(places);
        }
    }
}
