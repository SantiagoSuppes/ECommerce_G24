using Users.API.Dtos;
using Users.API.DTOs;

namespace Users.API.Services;

/// <summary>
/// Contrato de lógica de negocio de Users.API.
/// </summary>
public interface IUserService
{
    Task<RegisterUserResponseDto> RegisterAsync(
        RegisterUserRequestDto request);

    Task<LoginResponseDto> LoginAsync(
        LoginRequestDto request);
    Task<UserExistsResponseDto> ExistsAsync(Guid userId);
}