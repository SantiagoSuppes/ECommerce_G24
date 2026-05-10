namespace Users.API.Dtos;

public class UserResponseDto
{
    public required string Id { get; set; }
    public required string Nombre { get; set; }
    public required string Apellido { get; set; }
    public required string Email { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaRegistro { get; set; }
    public DateTime? FechaUltimoLogin { get; set; }
}
