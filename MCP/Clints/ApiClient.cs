using MCP.DTOs;
using MCP.Requests;
using System.Text.Json;

namespace MCP.Clints
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;

        public ApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<UserDto>> GetUsers(CancellationToken ct)
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/v1/user", ct);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(ct);
                return JsonSerializer.Deserialize<List<UserDto>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<UserDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching users: {ex.Message}");
                throw;
            }
        }

        public async Task<UserDto> CreateUser(UserRequest user, CancellationToken ct = default)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/v1/user", user, ct);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(ct);
                return JsonSerializer.Deserialize<UserDto>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? throw new InvalidOperationException("Failed to deserialize user data.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                throw;
            }
        }
    }
}
