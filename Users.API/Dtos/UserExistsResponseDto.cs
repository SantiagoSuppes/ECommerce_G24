namespace Users.API.Dtos;

/// <summary>
/// Respuesta utilizada por otros microservicios
/// para comprobar si un usuario está registrado.
/// </summary>
public class UserExistsResponseDto
{
    /// <summary>
    /// Indica si el usuario existe en Users.API.
    /// </summary>
    public bool Exists { get; set; }
}