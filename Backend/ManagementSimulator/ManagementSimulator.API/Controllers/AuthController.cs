
using ManagementSimulator.Core.Dtos.Requests.Users;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Core.Dtos.Requests.ResetPassword;
using ManagementSimulator.Core.Dtos.Requests.Impersonation;
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

        // Check if user is currently impersonating
        var isImpersonatingClaim = User.FindFirst("IsImpersonating");
        var isImpersonating = isImpersonatingClaim?.Value == "true";

        List<string> originalRoles;
        List<string> effectiveRoles;

        if (isImpersonating)
        {
            // When impersonating, current roles are the impersonated user's roles
            effectiveRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            // Original roles are the admin's roles stored in custom claims
            originalRoles = User.FindAll("OriginalRole").Select(r => r.Value).ToList();
        }
        else
        {
            // Normal case - not impersonating
            originalRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            effectiveRoles = new List<string>(originalRoles);
        }

        var isActingAsSecondManager = await _authorizationService.IsUserActingAsSecondManagerAsync(userId);
        if (isActingAsSecondManager && !effectiveRoles.Contains("Manager"))
        {
            effectiveRoles.Add("Manager");
        }

        var isTemporarilyReplaced = await _authorizationService.IsManagerTemporarilyReplacedAsync(userId);

        // Check if impersonated user token is available
        var hasValidImpersonationToken = User.FindFirst("HasValidImpersonationToken")?.Value == "true";

        return Ok(new
        {
            UserId = userIdClaim,
            Email = User.FindFirst(ClaimTypes.Email)?.Value,
            Roles = effectiveRoles,
            OriginalRoles = originalRoles,
            IsActingAsSecondManager = isActingAsSecondManager,
            IsTemporarilyReplaced = isTemporarilyReplaced,
            IsImpersonating = isImpersonating,
            ImpersonatedUserId = isImpersonating ? User.FindFirst("ImpersonatedUserId")?.Value : null,
            OriginalUserId = isImpersonating ? User.FindFirst("OriginalUserId")?.Value : null,
            HasValidImpersonationToken = hasValidImpersonationToken,
            // Indicate which authentication context should be used
            ShouldUseImpersonationToken = isImpersonating && hasValidImpersonationToken
        });
    }

    [Authorize]
    [HttpGet("me/original")]
    public async Task<IActionResult> GetOriginalMe()
    {
        if (!User.Identity!.IsAuthenticated)
            return Unauthorized(new { message = "User is not authenticated." });

        // Check if user is currently impersonating
        var isImpersonatingClaim = User.FindFirst("IsImpersonating");
        var isImpersonating = isImpersonatingClaim?.Value == "true";

        if (!isImpersonating)
        {
            // If not impersonating, just return regular user info
            return await Me();
        }

        // Get original admin user ID
        var originalUserIdClaim = User.FindFirst("OriginalUserId")?.Value;
        if (string.IsNullOrEmpty(originalUserIdClaim) || !int.TryParse(originalUserIdClaim, out var originalUserId))
            return BadRequest(new { message = "Invalid original user ID." });

        // Get original admin roles
        var originalRoles = User.FindAll("OriginalRole").Select(r => r.Value).ToList();

        var originalEmail = User.FindFirst("OriginalEmail")?.Value;

        // Check authorization services for the original admin user
        var isActingAsSecondManager = await _authorizationService.IsUserActingAsSecondManagerAsync(originalUserId);
        var isTemporarilyReplaced = await _authorizationService.IsManagerTemporarilyReplacedAsync(originalUserId);

        var effectiveRoles = new List<string>(originalRoles);
        if (isActingAsSecondManager && !effectiveRoles.Contains("Manager"))
        {
            effectiveRoles.Add("Manager");
        }

        return Ok(new
        {
            UserId = originalUserIdClaim,
            Email = originalEmail,
            Roles = effectiveRoles,
            OriginalRoles = originalRoles,
            IsActingAsSecondManager = isActingAsSecondManager,
            IsTemporarilyReplaced = isTemporarilyReplaced,
            IsImpersonating = false,
            ImpersonatedUserId = (string?)null,
            OriginalUserId = (string?)null,
            HasValidImpersonationToken = false,
            ShouldUseImpersonationToken = false
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

    [Authorize(Roles = "Admin")]
    [HttpPost("impersonate")]
    public async Task<IActionResult> Impersonate([FromBody] ImpersonateUserRequestDto request)
    {
        try
        {
            var success = await _authService.ImpersonateUserAsync(HttpContext, request.UserId);
            if (!success)
                return BadRequest(new { error = "Unable to impersonate user. User may not exist or requires password change." });

            return Ok(new { message = "Successfully impersonating user." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error impersonating user {UserId}", request.UserId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [Authorize]
    [HttpPost("stop-impersonation")]
    public async Task<IActionResult> StopImpersonation()
    {
        try
        {
            var success = await _authService.StopImpersonationAsync(HttpContext);
            if (!success)
                return BadRequest(new { error = "Not currently impersonating or unable to stop impersonation." });

            return Ok(new { message = "Successfully stopped impersonation." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping impersonation");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}

