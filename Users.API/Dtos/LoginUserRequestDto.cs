namespace Users.API.Dtos;

public class LoginUserRequestDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
