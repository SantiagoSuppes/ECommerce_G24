using Users.API.Models;

namespace Users.API.Repositories;

/// <summary>
/// persistencia para usuarios.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);

    Task<User> CreateAsync(User user);

    Task<User> RegisterFailedAttemptAsync(Guid userId);

    Task ResetFailedAttemptsAsync(Guid userId);

    //Traer usuario por id para las llamadas provenientes de Notifications.API
    Task<User?> GetByIdAsync(Guid id);
}