using Users.API.Dtos;
using Users.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Users.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    ///<summary>
    /// Registra un nuevo usuario.
    ///</summary>
    [HttpPost("register")]
    public async Task<ActionResult<UserResponseDto>> Register([FromBody] RegisterUserRequestDto request)
    {
        var user = await _userService.RegisterAsync(request);
        return Created($"/api/users/{user.Id}", user);
    }

    ///<summary>
    /// Inicia sesión de un usuario.
    ///</summary>
    [HttpPost("login")]
    public async Task<ActionResult<UserResponseDto>> Login([FromBody] LoginUserRequestDto request)
    {
        var user = await _userService.LoginAsync(request);
        return Ok(user);
    }
}
