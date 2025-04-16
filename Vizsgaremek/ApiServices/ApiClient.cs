using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Vizsgaremek.ApiServices
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;

        public string? Token { get; set; }

        public ApiClient()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:3000");
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("/auth/login", new { username, password });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                Token = result?.Token;

                if (!string.IsNullOrEmpty(Token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> RegisterAsync(string username, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("/users", new { username, password });

            if (!response.IsSuccessStatusCode)
            {
                string errorJson = await response.Content.ReadAsStringAsync();

                try
                {
                    var errorObj = JsonSerializer.Deserialize<ErrorResponse>(errorJson);

                    if (errorObj?.Message?.ToLower().Contains("already exists") == true)
                    {
                        System.Windows.MessageBox.Show("Ez a felhasználónév már létezik!", "Regisztráció", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Hiba a regisztráció során!", "Regisztráció", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                }
                catch
                {
                    System.Windows.MessageBox.Show("Ismeretlen hiba történt a regisztrációnál.", "Regisztráció", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }

                return false;
            }

            return true;
        }

        public async Task<LoggedInUser?> GetLoggedInUserAsync()
        {
            if (string.IsNullOrEmpty(Token))
                return null;

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

            var response = await _httpClient.GetAsync("/auth/me");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<LoggedInUser>();
            }

            return null;
        }

        public async Task<List<LoggedInUser>> GetLeaderboardAsync()
        {
            var response = await _httpClient.GetAsync("/users");

            if (response.IsSuccessStatusCode)
            {
                var users = await response.Content.ReadFromJsonAsync<List<LoggedInUser>>();
                if (users == null)
                    return new List<LoggedInUser>();

                return users.OrderByDescending(u => u.Score).ToList();
            }

            return new List<LoggedInUser>();
        }

        public async Task<bool> AddPointsAsync(int points)
        {
            if (string.IsNullOrEmpty(Token))
                return false;

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

            var response = await _httpClient.PostAsJsonAsync("/users/add-points", new { pointsToAdd = points });

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            if (string.IsNullOrEmpty(Token))
                return false;

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

            var response = await _httpClient.DeleteAsync($"/users/{userId}");

            return response.IsSuccessStatusCode;
        }
    }

    public class LoginResponse
    {
        public string? Token { get; set; } = null;
    }

    public class LoggedInUser
    {
        public int Id { get; set; }
        public required string Username { get; set; }

        [JsonPropertyName("points")]
        public int Score { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; } = "USER";

        [JsonIgnore]
        public int Rank { get; set; }

        [JsonIgnore]
        public bool IsAdmin => Role == "ADMIN";
    }

    public class ErrorResponse
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }
    }
}