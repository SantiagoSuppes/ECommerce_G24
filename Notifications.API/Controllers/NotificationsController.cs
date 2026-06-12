using ECommerce_G24.Notifications.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using Notifications.API.DTOs;
using Notifications.API.Services;

namespace Notifications.API.Controllers;

/// <summary>
/// Endpoints para registrar y consultar notificaciones.
/// </summary>
[ApiController]
[Route("api/notifications")]
[Produces("application/json")]
[Tags("Notifications")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(
        INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Registra y simula el envío de una notificación.
    /// </summary>
   
    [HttpPost("send")]
    [ProducesResponseType(
        typeof(NotificationResponseDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ErrorResponseDto),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ErrorResponseDto),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ErrorResponseDto),
        StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<NotificationResponseDto>> Send(
        CreateNotificationRequestDto request,
        CancellationToken cancellationToken)
    {
        var notification =
            await _notificationService.SendAsync(
                request,
                cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            notification);
    }

    /// <summary>
    /// Lista todas las notificaciones de un usuario.
    /// </summary>
    [HttpGet("{userId:guid}")]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<NotificationResponseDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ErrorResponseDto),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ErrorResponseDto),
        StatusCodes.Status500InternalServerError)]
    public async Task<
        ActionResult<IReadOnlyCollection<NotificationResponseDto>>>
        GetByUserId(Guid userId)
    {
        var notifications =
            await _notificationService.GetByUserIdAsync(
                userId);

        return Ok(notifications);
    }
}