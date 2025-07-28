using Orleans;

namespace OfficeLunchVote.Grains.Interfaces
{
    public interface ITimeGrain: IGrainWithIntegerKey
    {
        Task<DateTime> GetCurrentTimeAsync();
        Task SetCurrentTimeAsync(DateTime time);
    }
}
