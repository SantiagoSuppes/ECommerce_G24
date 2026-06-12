using Microsoft.AspNetCore.Mvc;
using Users.API.Dtos;
using Users.API.DTOs;
using Users.API.Services;

namespace Users.API.Controllers;

/// <summary>
/// Endpoints relacionados con registro y autenticación de usuarios.
/// </summary>
[ApiController]
[Route("api/users")]
[Produces("application/json")]
[Consumes("application/json")]
[Tags("Users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(
        IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Registra un nuevo usuario.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(
        typeof(RegisterUserResponseDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ErrorResponseDto),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ErrorResponseDto),
        StatusCodes.Status409Conflict)]
    [ProducesResponseType(
        typeof(ErrorResponseDto),
        StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RegisterUserResponseDto>> Register(
        RegisterUserRequestDto request)
    {
        var response =
            await _userService.RegisterAsync(request);

        // El TP exige 201 Created.
        return StatusCode(
            StatusCodes.Status201Created,
            response);
    }

    /// <summary>
    /// Autentica un usuario mediante email y contraseña.
    /// </summary>

    [HttpPost("login")]
    [ProducesResponseType(
        typeof(LoginResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ErrorResponseDto),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ErrorResponseDto),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ErrorResponseDto),
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        typeof(ErrorResponseDto),
        StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LoginResponseDto>> Login(
        LoginRequestDto request)
    {
        var response =
            await _userService.LoginAsync(request);

        return Ok(response);
    }
    /// <summary>
    /// Comprueba si un usuario está registrado.
    /// Este endpoint es utilizado por otros microservicios.
    /// </summary>
    /// <param name="id">
    /// Identificador del usuario.
    /// </param>
    /// <returns>
    /// Devuelve true si el usuario existe.
    /// </returns>
    [HttpGet("{id:guid}/exists")]
    [ProducesResponseType(
        typeof(UserExistsResponseDto),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<UserExistsResponseDto>>
        Exists(Guid id)
    {
        var response =
            await _userService.ExistsAsync(id);

        return Ok(response);
    }
}