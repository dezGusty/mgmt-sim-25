
using ManagementSimulator.Core.Dtos.Requests.Users;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Core.Dtos.Requests.ResetPassword;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService; 
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, IUserService userService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _userService = userService;
        _logger = logger;
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

    [HttpPost("send-reset-code")]
    public async Task<IActionResult> SendResetCode([FromBody] SendResetCodeRequestDto request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("Email is required");
            }

            var success = await _userService.SendPasswordResetCodeAsync(request.Email);

            if (!success)
            {
                return BadRequest("Email not found");
            }

            return Ok(new { message = "Reset code sent successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending reset code to {Email}", request.Email);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.VerificationCode) ||
                string.IsNullOrEmpty(request.NewPassword) ||
                string.IsNullOrEmpty(request.ConfirmPassword))
            {
                return BadRequest("All fields are required");
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest("Passwords do not match");
            }

            var success = await _userService.ResetPasswordWithCodeAsync(request.VerificationCode, request.NewPassword);

            if (!success)
            {
                return BadRequest("Invalid or expired verification code");
            }

            return Ok(new { message = "Password reset successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return StatusCode(500, "Internal server error");
        }
    }
}

