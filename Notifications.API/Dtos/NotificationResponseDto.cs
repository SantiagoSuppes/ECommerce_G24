using Swashbuckle.AspNetCore.Filters;

namespace ECommerce_G24.Notifications.API.Dtos;

/// <summary>
/// DTO para respuesta de notificación.
///
/// Ejemplo:
/// {
///   "id": "11112222-3333-4444-5555-666677778888",
///   "usuarioId": "a1b2c3d4-0000-0000-0000-111122223333",
///   "mensaje": "Su orden #f1e2d3c4 fue confirmada.",
///   "tipo": "Email",
///   "estado": "Enviada",
///   "fechaEnvio": "2024-03-10T12:01:00Z"
/// }
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