namespace Users.API.DTOs;

/// <summary>
/// Respuesta devuelta luego de registrar un usuario.
/// No contiene PasswordHash.
/// </summary>
public class RegisterUserResponseDto
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; }

    public bool Activo { get; set; }
}