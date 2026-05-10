using Users.API.Dtos;

namespace Users.API.Services;

public interface IUserService
{
    Task<UserResponseDto> RegisterAsync(RegisterUserRequestDto request);
    Task<UserResponseDto> LoginAsync(LoginUserRequestDto request);
}
