using Orders.API.Dtos;
using System.Net.Http.Json;

namespace Orders.API.Services;

/// <summary>
/// Cliente HTTP utilizado para consultar Users.API.
/// </summary>
public class UsersApiClient : IUsersApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsersApiClient> _logger;

    public UsersApiClient(
        HttpClient httpClient,
        ILogger<UsersApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> UserExistsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var requestUrl =
            $"/api/users/{userId}/exists";

        _logger.LogInformation(
            "Consultando usuario en {BaseAddress}{RequestUrl}",
            _httpClient.BaseAddress,
            requestUrl);

        var response =
            await _httpClient.GetAsync(
                requestUrl,
                cancellationToken);

        /*
         * El endpoint /exists siempre debería responder 200.
         *
         * Si devuelve 404, 500 o cualquier otro error,
         * significa que hay un problema de integración.
         */
        response.EnsureSuccessStatusCode();

        var result =
            await response.Content
                .ReadFromJsonAsync<UserExistsResponseDto>(
                    cancellationToken:
                        cancellationToken);

        return result?.Exists ?? false;
    }
}