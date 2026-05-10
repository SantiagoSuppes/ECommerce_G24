using Microsoft.AspNetCore.Mvc;
using ECommerce_G24.source.Users.API.Dtos;
using ECommerce_G24.source.Users.API.Services;

namespace ECommerce_G24.source.Users.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserResponseDto>> Register([FromBody] RegisterUserRequestDto request)
    {
        var user = await _userService.RegisterAsync(request);
        return Created($"/api/users/{user.Id}", user);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserResponseDto>> Login([FromBody] LoginUserRequestDto request)
    {
        var user = await _userService.LoginAsync(request);
        return Ok(user);
    }
}
