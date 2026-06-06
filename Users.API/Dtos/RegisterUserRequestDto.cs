using System.ComponentModel.DataAnnotations;

namespace Users.API.Dtos;

public class RegisterUserRequestDto
{
    [Required(ErrorMessage = "El nombre es requerido.")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 20 caracteres")]
    public required string Nombre { get; set; }

    [Required(ErrorMessage = "El nombre es requerido.")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 20 caracteres")]
    public required string Apellido { get; set; }

    [Required(ErrorMessage = "El email es requerido.")]
    [StringLength(20, ErrorMessage = "Maximo 50 caracteres para el email.")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "La contraseña es requerida.")]
    [StringLength(20, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 20 caracteres")]
    public required string Password { get; set; }
}
