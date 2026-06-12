using System.Net;
using System.Net.Http.Json;
using Notifications.API.DTOs;

namespace Notifications.API.Services;

/// <summary>
/// Cliente HTTP utilizado para validar usuarios
/// contra Users.API.
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
        var response =
            await _httpClient.GetAsync(
                $"/api/users/{userId}",
                cancellationToken);

        // Si Users.API devuelve 404, el usuario no existe.
        if (response.StatusCode == HttpStatusCode.NotFound)
            return false;

        // Cualquier otro error se considera un fallo inesperado.
        response.EnsureSuccessStatusCode();

        var user =
            await response.Content
                .ReadFromJsonAsync<UserLookupResponseDto>(
                    cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Usuario validado en Users.API. UsuarioId: {UsuarioId}",
            userId);

        return user is not null &&
               user.Id == userId;
    }
}