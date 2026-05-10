namespace ECommerce_G24.source.Users.API.Dtos;

public class RegisterUserRequestDto
{
    public required string Nombre { get; set; }
    public required string Apellido { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}
