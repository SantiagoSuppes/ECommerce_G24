using ECommerce_G24.Notifications.API.Dtos;

namespace ECommerce_G24.Notifications.API.Services;

public interface INotificationService
{
    Task<NotificationResponseDto> SendNotificationAsync(CreateNotificationRequestDto request);
    Task<List<NotificationResponseDto>> GetNotificationsByUserIdAsync(string userId);
}
