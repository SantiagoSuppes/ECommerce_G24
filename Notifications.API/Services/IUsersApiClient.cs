namespace Notifications.API.Services;

/// <summary>
/// Contrato para consultar Users.API.
/// </summary>
public interface IUsersApiClient
{
    /// <summary>
    /// Indica si el usuario existe en Users.API.
    /// </summary>
    Task<bool> UserExistsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}