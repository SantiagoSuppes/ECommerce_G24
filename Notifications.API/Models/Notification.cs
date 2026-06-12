using System.ComponentModel.DataAnnotations;

namespace Notifications.API.Models;

/// <summary>
/// Representa una notificación registrada para un usuario.
/// </summary>
public class Notification
{
    /// <summary>
    /// Identificador único de la notificación.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador del usuario destinatario.
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Contenido de la notificación.
    /// </summary>
    [Required(ErrorMessage = "El mensaje es obligatorio.")]
    [MaxLength(
        500,
        ErrorMessage = "El mensaje no puede superar los 500 caracteres.")]
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>
    /// Medio utilizado para la notificación:
    /// Email, Push o SMS.
    /// </summary>
    [Required(ErrorMessage = "El tipo es obligatorio.")]
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// Estado de la notificación:
    /// Pendiente, Enviada o Fallida.
    /// </summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// Fecha asignada automáticamente al registrar el envío.
    /// </summary>
    public DateTime FechaEnvio { get; set; }
}