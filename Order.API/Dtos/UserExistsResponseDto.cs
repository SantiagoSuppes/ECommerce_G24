namespace Orders.API.Dtos;

/// <summary>
/// Respuesta recibida desde Users.API.
/// </summary>
public class UserExistsResponseDto
{
    /// <summary>
    /// Indica si el usuario está registrado.
    /// </summary>
    public bool Exists { get; set; }
}