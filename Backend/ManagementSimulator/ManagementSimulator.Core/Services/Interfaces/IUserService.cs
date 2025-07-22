using ManagementSimulator.Core.Dtos.Requests.Departments;
using ManagementSimulator.Core.Dtos.Requests.Users;
using ManagementSimulator.Core.Dtos.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<UserResponseDto>> GetAllUsersAsync();
        Task<UserResponseDto?> GetUserByIdAsync(int id);
        Task<UserResponseDto> AddUserAsync(CreateUserRequestDto request);
        Task<UserResponseDto?> UpdateUserAsync(int id, UpdateUserRequestDto request);
        Task<bool> DeleteUserAsync(int id);
        Task RestoreUserByIdAsync(int id);
    }
}
