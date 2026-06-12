namespace Users.API.DTOs;

/// <summary>
/// Datos públicos de un usuario.
/// Nunca contiene PasswordHash.
/// </summary>
public class UserLookupResponseDto
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool Activo { get; set; }
}