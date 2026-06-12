using Users.API.Models;

namespace Users.API.Repositories;

/// <summary>
/// Contrato de persistencia para usuarios.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);

    Task<User> CreateAsync(User user);

    Task<User> RegisterFailedAttemptAsync(Guid userId);

    Task ResetFailedAttemptsAsync(Guid userId);
    Task<bool> ExistsByIdAsync(Guid userId);
}