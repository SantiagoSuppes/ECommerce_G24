namespace Users.API.DTOs;

/// <summary>
/// Respuesta devuelta luego de un login correcto.
/// No contiene PasswordHash.
/// </summary>
public class LoginResponseDto
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}