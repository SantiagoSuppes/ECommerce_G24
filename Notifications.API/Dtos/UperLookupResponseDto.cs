namespace Notifications.API.DTOs;

/// <summary>
/// Respuesta mínima recibida desde Users.API.
/// Se utiliza para validar la existencia del usuario.
/// </summary>
public class UserLookupResponseDto
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool Activo { get; set; }
}