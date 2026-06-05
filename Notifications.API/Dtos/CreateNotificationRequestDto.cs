using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Filters;

namespace ECommerce_G24.Notifications.API.Dtos;

/// <summary>
/// DTO para solicitud de creación de notificación.
///
/// Ejemplo:
/// {
///   "usuarioId": "a1b2c3d4-0000-0000-0000-111122223333",
///   "mensaje": "Su orden #f1e2d3c4 fue confirmada.",
///   "tipo": "Email"
/// }
/// </summary>
public class CreateNotificationRequestDto
{
    /// Identificador del usuario receptor de la notificación.
    [Required(ErrorMessage = "El ID del usuario es requerido.")]
    [StringLength(36, MinimumLength = 36, ErrorMessage = "El ID del usuario debe ser un GUID válido.")]
    public string UsuarioId { get; set; } = string.Empty;

    /// Contenido del mensaje de la notificación.
    [Required(ErrorMessage = "El mensaje es requerido.")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "El mensaje debe tener entre 1 y 500 caracteres.")]
    public string Mensaje { get; set; } = string.Empty;

    /// Tipo de notificación (Email, SMS, Push, etc.).
    [Required(ErrorMessage = "El tipo de notificación es requerido.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "El tipo debe tener entre 1 y 50 caracteres.")]
    public string Tipo { get; set; } = string.Empty;
}