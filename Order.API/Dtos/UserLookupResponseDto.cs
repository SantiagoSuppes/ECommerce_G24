namespace Orders.API.DTOs;

/// <summary>
/// Datos públicos recibidos desde Users.API.
/// </summary>
public class UserLookupResponseDto
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool Activo { get; set; }
}