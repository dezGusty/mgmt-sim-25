
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Database.Repositories.Intefaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace ManagementSimulator.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogService _auditLogService;

        public AuthService(IUserRepository userRepository, IAuditLogService auditLogService)
        {
            _userRepository = userRepository;
            _auditLogService = auditLogService;
        }

        public async Task<bool> LoginAsync(HttpContext httpContext, string email, string password)
        {
            var user = await _userRepository.GetUserByEmail(email);

            if (user == null || user.MustChangePassword || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                // Log failed login attempt
                await _auditLogService.LogAuthenticationAsync(
                    "LOGIN_FAILED",
                    email,
                    user?.Id ?? 0,
                    success: false,
                    errorMessage: user == null ? "User not found" :
                                 user.MustChangePassword ? "Password change required" :
                                 "Invalid credentials",
                    httpContext: httpContext);

                return false;
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
            };

            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Role.Rolename));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await httpContext.SignInAsync("Cookies", principal);

            // Log successful login
            await _auditLogService.LogAuthenticationAsync(
                "LOGIN_SUCCESS",
                user.Email,
                user.Id,
                success: true,
                httpContext: httpContext);

            return true;
        }

        public async Task<bool> ImpersonateUserAsync(HttpContext httpContext, int targetUserId)
        {
            // Get user with roles included
            var user = await _userRepository.GetUserWithReferencesByIdAsync(targetUserId);

            // Get current admin identity before switching
            var currentUser = httpContext.User;
            var originalUserId = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var originalEmail = currentUser.FindFirst(ClaimTypes.Email)?.Value;
            var originalRoles = currentUser.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            if (user == null || user.MustChangePassword)
            {
                // Log failed impersonation attempt
                await _auditLogService.LogAuthenticationAsync(
                    "IMPERSONATION_FAILED",
                    originalEmail ?? "Unknown",
                    int.TryParse(originalUserId, out var adminId) ? adminId : 0,
                    success: false,
                    errorMessage: user == null ? "Target user not found" : "Target user must change password",
                    httpContext: httpContext);

                return false;
            }

            if (string.IsNullOrEmpty(originalUserId) || !originalRoles.Any())
            {
                // Log failed impersonation attempt - invalid admin context
                await _auditLogService.LogAuthenticationAsync(
                    "IMPERSONATION_FAILED",
                    originalEmail ?? "Unknown",
                    0,
                    success: false,
                    errorMessage: "Invalid admin context",
                    httpContext: httpContext);

                return false;
            }

            // Create claims for impersonated user - the impersonated user's identity takes precedence
            var claims = new List<Claim>
            {
                // Primary identity should be the impersonated user for token/authentication purposes
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),

                // Original admin identity preservation for fallback scenarios
                new Claim("OriginalUserId", originalUserId),
                new Claim("OriginalEmail", originalEmail ?? ""),
                new Claim("IsImpersonating", "true"),
                new Claim("ImpersonatedUserId", user.Id.ToString()),
                new Claim("HasValidImpersonationToken", "true") // Indicates impersonated user token is available
            };

            // Add impersonated user roles as primary roles
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Role.Rolename));
            }

            // Add original admin roles as separate claims for fallback access
            foreach (var originalRole in originalRoles)
            {
                claims.Add(new Claim("OriginalRole", originalRole));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await httpContext.SignInAsync("Cookies", principal);

            // Log successful impersonation
            await _auditLogService.LogAuthenticationAsync(
                "IMPERSONATION_STARTED",
                originalEmail ?? "Unknown",
                int.TryParse(originalUserId, out var origUserId) ? origUserId : 0,
                success: true,
                httpContext: httpContext,
                impersonatedUserId: user.Id,
                impersonatedUserEmail: user.Email);

            return true;
        }


        public async Task<bool> StopImpersonationAsync(HttpContext httpContext)
        {
            var currentUser = httpContext.User;

            // Check if user is currently impersonating
            var isImpersonatingClaim = currentUser.FindFirst("IsImpersonating");
            if (isImpersonatingClaim?.Value != "true")
            {
                // Log failed stop impersonation attempt
                await _auditLogService.LogAuthenticationAsync(
                    "STOP_IMPERSONATION_FAILED",
                    currentUser.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown",
                    int.TryParse(currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) ? userId : 0,
                    success: false,
                    errorMessage: "Not currently impersonating",
                    httpContext: httpContext);

                return false;
            }

            // Get context for audit logging
            var impersonatedEmail = currentUser.FindFirst(ClaimTypes.Email)?.Value;
            var impersonatedUserId = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Get original admin identity
            var originalUserId = currentUser.FindFirst("OriginalUserId")?.Value;
            var originalEmail = currentUser.FindFirst("OriginalEmail")?.Value;
            var originalRoles = currentUser.FindAll("OriginalRole").Select(r => r.Value).ToList();

            if (string.IsNullOrEmpty(originalUserId) || !originalRoles.Any())
            {
                // Log failed stop impersonation attempt
                await _auditLogService.LogAuthenticationAsync(
                    "STOP_IMPERSONATION_FAILED",
                    originalEmail ?? "Unknown",
                    int.TryParse(originalUserId, out var origId) ? origId : 0,
                    success: false,
                    errorMessage: "Invalid original admin context",
                    httpContext: httpContext);

                return false;
            }

            // Restore original admin claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, originalUserId),
                new Claim(ClaimTypes.Email, originalEmail ?? "")
            };

            foreach (var role in originalRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await httpContext.SignInAsync("Cookies", principal);

            // Log successful stop impersonation
            await _auditLogService.LogAuthenticationAsync(
                "IMPERSONATION_STOPPED",
                originalEmail ?? "Unknown",
                int.TryParse(originalUserId, out var originalUserIdInt) ? originalUserIdInt : 0,
                success: true,
                httpContext: httpContext,
                impersonatedUserId: int.TryParse(impersonatedUserId, out var impUserId) ? impUserId : (int?)null,
                impersonatedUserEmail: impersonatedEmail);

            return true;
        }

        public async Task LogoutAsync(HttpContext httpContext)
        {
            // Get user context before logging out
            var currentUser = httpContext.User;
            var userEmail = currentUser.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";
            var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isImpersonating = currentUser.FindFirst("IsImpersonating")?.Value == "true";

            // Log the logout
            await _auditLogService.LogAuthenticationAsync(
                "LOGOUT",
                userEmail,
                int.TryParse(userIdClaim, out var userId) ? userId : 0,
                success: true,
                httpContext: httpContext);

            // If impersonating, also log that context
            if (isImpersonating)
            {
                var originalEmail = currentUser.FindFirst("OriginalEmail")?.Value;
                var originalUserId = currentUser.FindFirst("OriginalUserId")?.Value;

                await _auditLogService.LogAuthenticationAsync(
                    "LOGOUT_WHILE_IMPERSONATING",
                    originalEmail ?? "Unknown",
                    int.TryParse(originalUserId, out var origId) ? origId : 0,
                    success: true,
                    httpContext: httpContext,
                    impersonatedUserId: int.TryParse(userIdClaim, out var impUserId) ? impUserId : (int?)null,
                    impersonatedUserEmail: userEmail);
            }

            await httpContext.SignOutAsync("Cookies");
        }
    }

}
