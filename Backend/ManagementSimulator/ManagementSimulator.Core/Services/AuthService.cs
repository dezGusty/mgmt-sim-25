
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
        private readonly ISecondaryManagerRepository _secondaryManagerRepository;

        public AuthService(IUserRepository userRepository, ISecondaryManagerRepository secondaryManagerRepository)
        {
            _userRepository = userRepository;
            _secondaryManagerRepository = secondaryManagerRepository;
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

            // Add permanent roles
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Role.Rolename));
            }

            // Check if user is currently an active secondary manager
            var isActiveSecondaryManager = await _secondaryManagerRepository
                .HasActiveSecondaryManagerAssignmentAsync(user.Id);
            
            if (isActiveSecondaryManager)
            {
                // Add temporary Manager role if not already present
                if (!claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Manager"))
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Manager"));
                }
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
