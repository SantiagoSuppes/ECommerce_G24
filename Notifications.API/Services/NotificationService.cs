using ECommerce_G24.Notifications.API.Dtos;
using ECommerce_G24.Notifications.API.Exceptions;
using ECommerce_G24.Notifications.API.Models;

namespace ECommerce_G24.Notifications.API.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly Dictionary<string, List<Notification>> _notificationsStore;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
        _notificationsStore = new Dictionary<string, List<Notification>>();
    }

    public async Task<NotificationResponseDto> SendNotificationAsync(CreateNotificationRequestDto request)
    {
        if (request == null || string.IsNullOrEmpty(request.UsuarioId) || 
            string.IsNullOrEmpty(request.Mensaje) || string.IsNullOrEmpty(request.Tipo))
        {
            _logger.LogWarning("Validacion fallida - Datos invalidos en request");
            throw new ValidationException("Los datos de la notificación son inválidos.");
        }

        if (string.IsNullOrWhiteSpace(request.UsuarioId))
        {
            _logger.LogWarning("Usuario no encontrado - UsuarioId: {UsuarioId}", request.UsuarioId);
            throw new UserNotFoundException("El usuario solicitado no fue encontrado.");
        }

        try
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid().ToString(),
                UsuarioId = request.UsuarioId,
                Mensaje = request.Mensaje,
                Tipo = request.Tipo,
                Estado = "Enviada",
                FechaEnvio = DateTime.UtcNow
            };

            if (!_notificationsStore.ContainsKey(request.UsuarioId))
            {
                _notificationsStore[request.UsuarioId] = new List<Notification>();
            }
            _notificationsStore[request.UsuarioId].Add(notification);

            _logger.LogInformation("Notificación creada: {NotificationId} para usuario {UsuarioId}", 
                notification.Id, request.UsuarioId);

            return MapToResponseDto(notification);
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (UserNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear notificación");
            throw new InternalServerException("Error interno al procesar la notificación.");
        }
    }

    public async Task<List<NotificationResponseDto>> GetNotificationsByUserIdAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("Usuario no encontrado - UsuarioId: {UsuarioId}", userId);
            throw new UserNotFoundException("El usuario solicitado no fue encontrado.");
        }

        try
        {
            if (!_notificationsStore.ContainsKey(userId) || _notificationsStore[userId].Count == 0)
            {
                _logger.LogWarning("No se encontraron notificaciones para usuario: {UserId}", userId);
                throw new NotFoundException("No se encontraron notificaciones para el usuario.");
            }

            var notifications = _notificationsStore[userId];
            _logger.LogInformation("Se obtuvieron {Count} notificaciones para usuario {UsuarioId}", 
                notifications.Count, userId);

            return notifications.Select(MapToResponseDto).ToList();
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (UserNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener notificaciones para usuario {UsuarioId}", userId);
            throw new InternalServerException("Error interno al obtener notificaciones.");
        }
    }

    private NotificationResponseDto MapToResponseDto(Notification notification)
    {
        return new NotificationResponseDto
        {
            Id = notification.Id,
            UsuarioId = notification.UsuarioId,
            Mensaje = notification.Mensaje,
            Tipo = notification.Tipo,
            Estado = notification.Estado,
            FechaEnvio = notification.FechaEnvio
        };
    }
}
