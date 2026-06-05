using Microsoft.AspNetCore.Mvc;
using ECommerce_G24.Notifications.API.Dtos;
using ECommerce_G24.Notifications.API.Services;

namespace ECommerce_G24.Notifications.API.Controllers;

///<summary>
/// Controlador para gestionar notificaciones de usuarios.
/// Permite crear, enviar y recuperar notificaciones del sistema.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Notifications")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(INotificationService notificationService, ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Crea y envía una nueva notificación a un usuario.
    /// </summary>
    /// <param name="request">Datos de la notificación a crear.</param>
    /// <returns>Notificación creada con su ID único.</returns>
    /// <response code="201">Notificación creada exitosamente.</response>
    /// <response code="400">Datos de entrada inválidos. Código de error: NTF-002.</response>
    /// <response code="404">Usuario no encontrado. Código de error: NTF-001.</response>
    /// <response code="500">Error interno en el servidor. Código de error: NTF-004.</response>
    [HttpPost("send")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<NotificationResponseDto>> SendNotification([FromBody] CreateNotificationRequestDto request)
    {
        _logger.LogInformation("Solicitud de envío de notificación para usuario: {UsuarioId}", request.UsuarioId);
        
        var notification = await _notificationService.SendNotificationAsync(request);
        
        return Created($"/api/notifications/{notification.Id}", notification);
    }

    /// <summary>
    /// Obtiene todas las notificaciones de un usuario específico.
    /// </summary>
    /// <param name="userId">ID del usuario cuyas notificaciones se desean obtener.</param>
    /// <returns>Lista de notificaciones del usuario.</returns>
    /// <response code="200">Notificaciones obtenidas exitosamente.</response>
    /// <response code="404">Usuario no encontrado. Código de error: NTF-001.</response>
    /// <response code="500">Error interno en el servidor. Código de error: NTF-004.</response>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(List<NotificationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<NotificationResponseDto>>> GetNotificationsByUserId(string userId)
    {
        _logger.LogInformation("Solicitud de obtención de notificaciones para usuario: {UserId}", userId);
        
        var notifications = await _notificationService.GetNotificationsByUserIdAsync(userId);
        
        return Ok(notifications);
    }
}
