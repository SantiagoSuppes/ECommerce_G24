using ECommerce_G24.Notifications.API.Dtos;
using Notifications.API.DTOs;

namespace Notifications.API.Services;

/// <summary>
/// Contrato de lógica de negocio
/// para Notifications.API.
/// </summary>
public interface INotificationService
{
    Task<NotificationResponseDto> SendAsync(
        CreateNotificationRequestDto request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<NotificationResponseDto>> GetByUserIdAsync(
        Guid userId);
}