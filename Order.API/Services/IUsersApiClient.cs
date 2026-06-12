namespace Orders.API.Services;

/// <summary>
/// Contrato para consultar Users.API.
/// </summary>
public interface IUsersApiClient
{
    /// <summary>
    /// Comprueba si el usuario existe.
    /// </summary>
    Task<bool> UserExistsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}