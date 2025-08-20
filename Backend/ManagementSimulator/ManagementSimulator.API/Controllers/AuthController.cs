
using ManagementSimulator.Core.Dtos.Requests.Users;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Core.Dtos.Requests.ResetPassword;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly IResourceAuthorizationService _authorizationService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, IUserService userService, IResourceAuthorizationService authorizationService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _userService = userService;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var success = await _authService.LoginAsync(HttpContext, dto.Email, dto.Password);
        if (!success)
            return Unauthorized("Incorrect email or password.");

        return Ok(new { message = "Successfully authenticated." });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync(HttpContext);
        return Ok(new { message = "Successfully logged out." });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        if (!User.Identity!.IsAuthenticated)
            return Unauthorized(new { message = "User is not authenticated." });

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Invalid user ID." });

        var originalRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
        var effectiveRoles = new List<string>(originalRoles);

        var isActingAsSecondManager = await _authorizationService.IsUserActingAsSecondManagerAsync(userId);
        if (isActingAsSecondManager && !effectiveRoles.Contains("Manager"))
        {
            effectiveRoles.Add("Manager");
        }

        var isTemporarilyReplaced = await _authorizationService.IsManagerTemporarilyReplacedAsync(userId);

        return Ok(new
        {
            UserId = userIdClaim,
            Email = User.FindFirst(ClaimTypes.Email)?.Value,
            Roles = effectiveRoles,
            OriginalRoles = originalRoles,
            IsActingAsSecondManager = isActingAsSecondManager,
            IsTemporarilyReplaced = isTemporarilyReplaced
        });
    }

    [HttpPost("send-reset-code")]
    [AllowAnonymous]
    public async Task<IActionResult> SendResetCode([FromBody] SendResetCodeRequestDto request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { error = "Email is required" }); // JSON format
            }

            var success = await _userService.SendPasswordResetCodeAsync(request.Email);

            if (!success)
            {
                return BadRequest(new { error = "Email not found" }); // JSON format
            }

            return Ok(new { message = "Reset code sent successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending reset code to {Email}", request.Email);
            return StatusCode(500, new { error = "Internal server error" }); // JSON format
        }
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.VerificationCode) ||
                string.IsNullOrEmpty(request.NewPassword) ||
                string.IsNullOrEmpty(request.ConfirmPassword))
            {
                return BadRequest(new { error = "All fields are required" });
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest(new { error = "Passwords do not match" });
            }

            var success = await _userService.ResetPasswordWithCodeAsync(request.VerificationCode, request.NewPassword);

            if (!success)
            {
                return BadRequest(new { error = "Invalid or expired verification code" });
            }

            return Ok(new { message = "Password reset successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}

