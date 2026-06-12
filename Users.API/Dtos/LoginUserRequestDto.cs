namespace ECommerce_G24.source.Users.API.Dtos;

public class LoginUserRequestDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
