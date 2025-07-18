using ManagementSimulator.Core.Dtos.Requests;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Mapping;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Database.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly MGMTSimulatorDbContext _context;
        private readonly IJwtService _jwtService;

        public AuthService(MGMTSimulatorDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<LoginResponse?> LoginAsync(UserLoginRequest request)
        {
            var user = await _context.Users
                .Include(u => u.Title)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                return null;

            if (!user.IsPasswordValid(request.Password))
                return null;

            var token = _jwtService.GenerateToken(user);
            var expiresAt = DateTime.UtcNow.AddHours(24);

            return new LoginResponse
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = user.ToUserResponseDto()
            };
        }
    }
}
