using Notifications.API.Models;

namespace Notifications.API.Repositories;

/// <summary>
/// Contrato de persistencia para las notificaciones.
/// </summary>
public interface INotificationRepository
{
    Task<Notification> CreateAsync(
        Notification notification);

    Task<IReadOnlyCollection<Notification>> GetByUserIdAsync(
        Guid userId);
}