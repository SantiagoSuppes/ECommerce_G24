namespace ECommerce_G24.Notifications.API.Dtos;

/// <summary>
/// DTO para respuesta de notificación.
/// </summary>
public class NotificationResponseDto
{
    /// Identificador único de la notificación.
    public string Id { get; set; } = string.Empty;

    /// Identificador del usuario receptor.
    public string UsuarioId { get; set; } = string.Empty;

    /// Contenido del mensaje.
    public string Mensaje { get; set; } = string.Empty;

    /// Tipo de notificación.
    public string Tipo { get; set; } = string.Empty;

    /// Estado actual de la notificación (Enviada, Leída, etc.).
    public string Estado { get; set; } = string.Empty;

    /// Fecha y hora del envío en formato UTC.
    public DateTime FechaEnvio { get; set; }
}