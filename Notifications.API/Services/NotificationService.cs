using ECommerce_G24.Notifications.API.Dtos;
using ECommerce_G24.Notifications.API.Services;
using Notifications.API.Dtos;
using Notifications.API.Exceptions;
using Notifications.API.Models;
using Notifications.API.Repositories;

namespace Notifications.API.Services;

/// <summary>
/// Servicio principal de Notifications.API.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly IUsersApiClient _usersApiClient;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository repository,
        IUsersApiClient usersApiClient,
        ILogger<NotificationService> logger)
    {
        _repository = repository;
        _usersApiClient = usersApiClient;
        _logger = logger;
    }

    public async Task<NotificationResponseDto> SendAsync(
        CreateNotificationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Guid.Empty representa un usuario inválido.
        if (request.UsuarioId == Guid.Empty)
        {
            throw new ValidationException(
                NotificationErrorCodes.InvalidNotificationData,
                "El usuario destinatario es obligatorio.");
        }

        // Evita mensajes nulos, vacíos o formados solo por espacios.
        if (string.IsNullOrWhiteSpace(request.Mensaje))
        {
            throw new ValidationException(
                NotificationErrorCodes.InvalidNotificationData,
                "El mensaje es obligatorio.");
        }

        if (request.Mensaje.Trim().Length > 500)
        {
            throw new ValidationException(
                NotificationErrorCodes.InvalidNotificationData,
                "El mensaje no puede superar los 500 caracteres.");
        }

        // Valida que el tipo sea Email, Push o SMS.
        if (!NotificationTypes.IsValid(request.Tipo))
        {
            throw new ValidationException(
                NotificationErrorCodes.InvalidNotificationData,
                "El tipo de notificación debe ser Email, Push o SMS.");
        }

        // Consulta a Users.API.
        var userExists =
            await _usersApiClient.UserExistsAsync(
                request.UsuarioId,
                cancellationToken);

        if (!userExists)
        {
            throw new NotFoundException(
                NotificationErrorCodes.UserNotFound,
                "El usuario destinatario no fue encontrado.");
        }

        // Como el endpoint registra y simula el envío,
        // la notificación se almacena directamente como Enviada.
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UsuarioId = request.UsuarioId,
            Mensaje = request.Mensaje.Trim(),
            Tipo = NotificationTypes.Normalize(request.Tipo),
            Estado = NotificationStates.Sent,
            FechaEnvio = DateTime.UtcNow
        };

        var createdNotification =
            await _repository.CreateAsync(notification);

        _logger.LogInformation(
            "Notificación enviada. NotificationId: {NotificationId}, UsuarioId: {UsuarioId}, Tipo: {Tipo}",
            createdNotification.Id,
            createdNotification.UsuarioId,
            createdNotification.Tipo);

        return MapToResponse(createdNotification);
    }

    public async Task<IReadOnlyCollection<NotificationResponseDto>>
        GetByUserIdAsync(Guid userId)
    {
        var notifications =
            await _repository.GetByUserIdAsync(userId);

        // NTF-003 cuando el usuario
        // no tiene notificaciones registradas.
        if (notifications.Count == 0)
        {
            throw new NotFoundException(
                NotificationErrorCodes.NotificationsNotFound,
                "No se encontraron notificaciones para el usuario.");
        }

        return notifications
            .Select(MapToResponse)
            .ToList();
    }

    /// <summary>
    /// Convierte la entidad de dominio a DTO público.
    /// </summary>
    private static NotificationResponseDto MapToResponse(
        Notification notification)
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