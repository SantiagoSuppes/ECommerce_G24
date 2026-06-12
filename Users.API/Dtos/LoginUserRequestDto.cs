using System.ComponentModel.DataAnnotations;

namespace Users.API.Dtos;
public class LoginUserRequestDto
{
    /// <summary>
    /// Email registrado.
    /// </summary>
    [Required(ErrorMessage = "El email es requerido.")]
    [StringLength(50, ErrorMessage = "Maximo 50 caracteres para el email.")]
    public required string Email { get; set; }
    /// <summary>
    /// Contraseña del usuario.
    /// </summary>
    [Required(ErrorMessage = "La contraseña es requerida.")]
    [StringLength(20, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 20 caracteres")]
    public required string Password { get; set; }
}
