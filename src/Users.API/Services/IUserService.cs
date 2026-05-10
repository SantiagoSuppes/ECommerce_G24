using ECommerce_G24.source.Users.API.Dtos;

namespace ECommerce_G24.source.Users.API.Services;

public interface IUserService
{
    Task<UserResponseDto> RegisterAsync(RegisterUserRequestDto request);
    Task<UserResponseDto> LoginAsync(LoginUserRequestDto request);
}
