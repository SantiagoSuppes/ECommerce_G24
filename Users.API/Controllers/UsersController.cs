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

        
        return StatusCode(
            StatusCodes.Status201Created,
            response);
    }

    
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
        LoginUserRequestDto request)

    {
        var response =
            await _userService.LoginAsync(request);

        return Ok(response);
    }

    /// <summary>
    /// Obtiene los datos públicos de un usuario por ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(UserLookupResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserLookupResponseDto>> GetById(
        Guid id)
    {
        var user =
            await _userService.GetByIdAsync(id);

        if (user is null)
            return NotFound();

        return Ok(user);
    }
}