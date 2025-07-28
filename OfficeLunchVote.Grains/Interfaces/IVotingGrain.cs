namespace OfficeLunchVote.Grains.Interfaces
{
    public interface IVotingGrain: IGrainWithStringKey
    {
        Task<bool> CreateVoteAsync(string user, DateOnly date);
        Task<bool> VoteAsync(string user, string place);
        Task<Dictionary<string, int>> GetResultsAsync();
        Task<bool> HasUserVotedAsync(string user);
        Task<bool> IsVotingClosedAsync();
        Task<DateTime> GetClosingTimeAsync();
        Task<List<string>> GetPlacesAsync();
        Task<bool> IsVoteCreatedAsync();
    }
}
