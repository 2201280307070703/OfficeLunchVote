using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace OfficeLunchVote.Client.Services
{
    public static class TimeService
    {
        public static async Task<(bool success, string? errorMessage)> SetServerTimeAsync(HttpClient http, string user, DateTime newDateTime)
        {
            var url = $"/time/dateTime?user={user}";

            var json = JsonSerializer.Serialize(newDateTime);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await http.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }
        }
    }
}
