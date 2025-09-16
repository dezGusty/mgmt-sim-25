
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

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> LoginAsync(HttpContext httpContext, string email, string password)
        {
            var user = await _userRepository.GetUserByEmail(email);
            if (user == null || user.MustChangePassword || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return false;

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
            return true;
        }

        public async Task<bool> ImpersonateUserAsync(HttpContext httpContext, int targetUserId)
        {
            // Get user with roles included
            var user = await _userRepository.GetUserWithReferencesByIdAsync(targetUserId);
            if (user == null || user.MustChangePassword)
                return false;

            // Get current admin identity before switching
            var currentUser = httpContext.User;
            var originalUserId = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var originalEmail = currentUser.FindFirst(ClaimTypes.Email)?.Value;
            var originalRoles = currentUser.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            if (string.IsNullOrEmpty(originalUserId) || !originalRoles.Any())
                return false;

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
            return true;
        }


        public async Task<bool> StopImpersonationAsync(HttpContext httpContext)
        {
            var currentUser = httpContext.User;
            
            // Check if user is currently impersonating
            var isImpersonatingClaim = currentUser.FindFirst("IsImpersonating");
            if (isImpersonatingClaim?.Value != "true")
                return false;

            // Get original admin identity
            var originalUserId = currentUser.FindFirst("OriginalUserId")?.Value;
            var originalEmail = currentUser.FindFirst("OriginalEmail")?.Value;
            var originalRoles = currentUser.FindAll("OriginalRole").Select(r => r.Value).ToList();

            if (string.IsNullOrEmpty(originalUserId) || !originalRoles.Any())
                return false;

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
            return true;
        }

        public async Task LogoutAsync(HttpContext httpContext)
        {
            await httpContext.SignOutAsync("Cookies");
        }
    }

}
