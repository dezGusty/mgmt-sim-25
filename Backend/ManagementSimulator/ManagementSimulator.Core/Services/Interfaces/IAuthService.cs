
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Services.Interfaces
{
    public interface IAuthService
    {

        Task<bool> LoginAsync(HttpContext httpContext, string email, string password);
        Task<bool> ImpersonateUserAsync(HttpContext httpContext, int targetUserId);
        Task LogoutAsync(HttpContext httpContext);
    }

}
