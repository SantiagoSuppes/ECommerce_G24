using System.Net;

namespace Orders.API.Clients;

public class UsersApiClient : IUsersApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsersApiClient> _logger;

    public UsersApiClient(HttpClient httpClient, ILogger<UsersApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"/api/users/{userId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return false;

        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Usuario verificado en Users.API. UserId: {UserId}", userId);
        return true;
    }
}