using Users.API.Dtos;
using Users.API.DTOs;

namespace Users.API.Services;

public interface IUserService
{
    Task<RegisterUserResponseDto> RegisterAsync(RegisterUserRequestDto request);
    Task<LoginResponseDto> LoginAsync(LoginUserRequestDto request);

    Task<UserLookupResponseDto?> GetByIdAsync(Guid id);
}
