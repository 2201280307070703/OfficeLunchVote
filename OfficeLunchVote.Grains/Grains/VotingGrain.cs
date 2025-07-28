using OfficeLunchVote.Grains.Interfaces;
using Orleans.Runtime;

namespace Grains
{
    public class VotingGrain : Grain, IVotingGrain
    {
        private readonly IPersistentState<VoteState> _state;
        private readonly IGrainFactory _grainFactory;

        private static readonly List<string> _places = new()
        {
            "Subway", "Dominos", "SushiBar", "Local Bistro", "Vegan Food"
        };

        public VotingGrain([PersistentState("voting", "votingStorage")] IPersistentState<VoteState> state, IGrainFactory grainFactory)
        {
            _state = state;
            _grainFactory = grainFactory;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            if (_state.State.Date == default)
            {
                _state.State.Date = DateOnly.Parse(this.GetPrimaryKeyString());
            }

            if (_state.State.Votes == null)
            {
                _state.State.Votes = new Dictionary<string, int>();
            }

            foreach (var place in _places)
            {
                if (!_state.State.Votes.ContainsKey(place))
                {
                    _state.State.Votes[place] = 0;
                }
            }

            if (_state.State.Voters == null)
            {
                _state.State.Voters = new HashSet<string>();
            }

            await base.OnActivateAsync(cancellationToken);
        }

        public async Task<bool> CreateVoteAsync(string user, DateOnly date)
        {
            if(user.ToLower() == "clock")
            {
                return false;
            }

            if (_state.State?.Date != default)
            {
                return false;
            }

            var timeGrain = _grainFactory.GetGrain<ITimeGrain>(0);
            var now = await timeGrain.GetCurrentTimeAsync();

            _state.State = new VoteState
            {
                Date = date,
                Votes = new Dictionary<string, int>(),
                Voters = new HashSet<string>(),
                CreatedAt = now
            };

            foreach (var place in _places)
                _state.State.Votes[place] = 0;

            await _state.WriteStateAsync();
            return true;
        }

        public async Task<bool> VoteAsync(string user, string place)
        {
            if (user.ToLower() == "clock")
            {
                return false;
            }

            var timeGrain = _grainFactory.GetGrain<ITimeGrain>(0);
            var now = await timeGrain.GetCurrentTimeAsync();

            if (_state.State.Voters.Contains(user))
                return false;

            if (IsAfterDeadline(now))
                return false;

            if (!_places.Contains(place))
                return false;

            _state.State.Voters.Add(user);
            _state.State.Votes[place]++;

            await _state.WriteStateAsync();
            return true;
        }

        public Task<Dictionary<string, int>> GetResultsAsync()
        {
            return Task.FromResult(_state.State.Votes);
        }

        public Task<bool> HasUserVotedAsync(string user)
        {
            return Task.FromResult(_state.State.Voters.Contains(user));
        }

        public async Task<bool> IsVotingClosedAsync()
        {
            var timeGrain = _grainFactory.GetGrain<ITimeGrain>(0);
            var now = await timeGrain.GetCurrentTimeAsync();
            return IsAfterDeadline(now);
        }

        public Task<DateTime> GetClosingTimeAsync()
        {
            return Task.FromResult(_state.State.Date.ToDateTime(new TimeOnly(11, 30)));
        }

        public Task<List<string>> GetPlacesAsync()
        {
            return Task.FromResult(_places);
        }

        private bool IsAfterDeadline(DateTime now)
        {
            return now.TimeOfDay >= new TimeSpan(11, 30, 0);
        }
    }

    [GenerateSerializer]
    public class VoteState
    {
        [Id(0)] public DateOnly Date { get; set; }
        [Id(1)] public Dictionary<string, int> Votes { get; set; } = new();
        [Id(2)] public HashSet<string> Voters { get; set; } = new();
        [Id(3)] public DateTime CreatedAt { get; set; }
    }
}
