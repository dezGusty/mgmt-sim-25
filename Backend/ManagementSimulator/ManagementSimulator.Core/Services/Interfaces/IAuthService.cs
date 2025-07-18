using ManagementSimulator.Core.Dtos.Requests;
using ManagementSimulator.Core.Dtos.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(UserLoginRequest request);
    }
}
