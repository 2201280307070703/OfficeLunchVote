using OfficeLunchVote.Grains.Interfaces;
using Orleans.Runtime;

namespace OfficeLunchVote.Grains.Grains
{
    public class TimeGrain : Grain, ITimeGrain
    {
        private readonly IPersistentState<TimeState> _state;
        private readonly TimeProvider _timeProvider;

        public TimeGrain([PersistentState("time", "votingStorage")] IPersistentState<TimeState> state, TimeProvider timeProvider)
        {
            _state = state;
            _timeProvider = timeProvider;
        }

        public Task<DateTime> GetCurrentTimeAsync()
        {
            return Task.FromResult(_state.State.OverrideTime ?? _timeProvider.GetUtcNow().DateTime);
        }

        public Task SetCurrentTimeAsync(DateTime time)
        {
            _state.State.OverrideTime = time;
            return _state.WriteStateAsync();
        }
    }

    [GenerateSerializer]
    public class TimeState
    {
        [Id(0)] public DateTime? OverrideTime { get; set; }
    }
}