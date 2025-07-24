
using ManagementSimulator.Core.Dtos.Requests.Users;
using ManagementSimulator.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var success = await _authService.LoginAsync(HttpContext, dto.Email, dto.Password);
        if (!success)
            return Unauthorized("Incorrect email or password.");

        return Ok(new { message = "Successfully authenticated." });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync(HttpContext);
        return Ok(new { message = "Successfully logged out." });
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        if (!User.Identity!.IsAuthenticated)
            return Unauthorized(new { message = "User is not authenticated." });

        return Ok(new
        {
            UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            Email = User.FindFirst(ClaimTypes.Email)?.Value,
            Roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList()
        });
    }
}
