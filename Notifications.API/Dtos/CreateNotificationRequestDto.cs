using System.ComponentModel.DataAnnotations;

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
    /// <summary>
    /// Usuario destinatario de la notificación.
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Mensaje que se desea enviar.
    /// </summary>
    [Required(ErrorMessage = "El mensaje es obligatorio.")]
    [MaxLength(
        500,
        ErrorMessage = "El mensaje no puede superar los 500 caracteres.")]
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de notificación: Email, Push o SMS.
    /// </summary>
    [Required(ErrorMessage = "El tipo es obligatorio.")]
    public string Tipo { get; set; } = string.Empty;
}