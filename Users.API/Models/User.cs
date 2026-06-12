using System.ComponentModel.DataAnnotations;

namespace Users.API.Models;

/// <summary>
/// Representa un usuario registrado en el sistema.
/// </summary>
public class User
{
    /// <summary>
    /// Identificador único del usuario.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre del usuario.
    /// </summary>
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Apellido del usuario.
    /// </summary>
    [Required(ErrorMessage = "El apellido es obligatorio.")]
    public string Apellido { get; set; } = string.Empty;

    /// <summary>
    /// Email único del usuario.
    /// </summary>
    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hash de la contraseña.
    /// </summary>
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Fecha asignada automáticamente al registrar al usuario.
    /// </summary>
    public DateTime FechaRegistro { get; set; }

    /// <summary>
    /// Indica si el usuario puede iniciar sesión.
    /// </summary>
    public bool Activo { get; set; }

    /// <summary>
    /// Cantidad de intentos fallidos consecutivos.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int IntentosFallidos { get; set; }
}