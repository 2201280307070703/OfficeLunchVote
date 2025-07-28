using System.Net.Http.Json;

namespace OfficeLunchVote.Client.services
{
    public static class VotingService
    {
        public static async Task<List<string>> GetPlacesAsync(HttpClient http)
        {
            var response = await http.GetFromJsonAsync<List<string>>("/voting/places");
            return response;
        }

        public static async Task<bool> IsVotingClosedAsync(HttpClient http)
        {
            var response = await http.GetFromJsonAsync<bool>("/voting/isVotingClosed");
            return response;
        }

        public static async Task<(bool alreadyVoted, string? errorMessage)> IsUserAlreadyVotedAsync(HttpClient http, string user)
        {
            if (string.IsNullOrWhiteSpace(user))
            {
                return (false, "User is empty.");
            }

            var response = await http.GetAsync($"/voting/isUserAlreadyVoted?user={Uri.EscapeDataString(user)}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<bool>();
                return (result, null);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }
        }

        public static async Task<(bool success, string? errorMessage)> SubmitVoteAsync(HttpClient http, string user, string place)
        {
            var url = $"/voting/vote?user={user}&place={place}";
            var response = await http.PostAsync(url, null);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }

        public static async Task<(Dictionary<string, int>? results, string? errorMessage)> GetResultsAsync(HttpClient http)
        {
            var response = await http.GetAsync("/voting/results");

            if (response.IsSuccessStatusCode)
            {
                var results = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
                return (results, null);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return (null, error);
            }
        }

        public static async Task<(bool success, string? errorMessage)> CreateVoteAsync(HttpClient http, string user)
        {
            var response = await http.PostAsync($"/voting/create?user={user}", null);

            if (response.IsSuccessStatusCode)
            {
                var isCreated = await response.Content.ReadFromJsonAsync<bool>();
                return (isCreated, null);
            }

            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }
    }
}
