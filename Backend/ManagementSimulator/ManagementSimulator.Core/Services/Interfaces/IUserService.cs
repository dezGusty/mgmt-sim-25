using ManagementSimulator.Core.Dtos.Requests.Departments;
using ManagementSimulator.Core.Dtos.Requests.Users;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Dtos.Responses.PagedResponse;
using ManagementSimulator.Core.Dtos.Responses.User;
using ManagementSimulator.Database.Dtos.QueryParams;
using ManagementSimulator.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Services.Interfaces
{
    public interface IUserService
    {
        Task<PagedResponseDto<UserResponseDto>> GetAllUnassignedUsersFilteredAsync(QueriedUserRequestDto payload);
        Task<List<User>> GetAllAdminsAsync(string? lastName, string? email);
        Task<PagedResponseDto<UserResponseDto>> GetAllUsersIncludeRelationshipsFilteredAsync(QueriedUserRequestDto payload);
        Task<PagedResponseDto<UserResponseDto>> GetAllUsersFilteredAsync(QueriedUserRequestDto payload);
        Task<List<UserResponseDto>> GetAllUsersIncludeRelationshipsAsync();
        Task<List<UserResponseDto>> GetAllUsersAsync();
        Task<PagedResponseDto<UserResponseDto>> GetAllManagersFilteredAsync(QueriedUserRequestDto payload);
        Task<UserResponseDto?> GetUserByIdAsync(int id);
        Task<UserResponseDto> AddUserAsync(CreateUserRequestDto request);
        Task<UserResponseDto?> UpdateUserAsync(int id, UpdateUserRequestDto request);
        Task<bool> DeleteUserAsync(int id);
        Task RestoreUserByIdAsync(int id);
        Task<bool> SendPasswordResetCodeAsync(string email);
        Task<bool> ResetPasswordWithCodeAsync(string verificationCode, string newPassword);
        Task<User?> GetUserByEmailAsync(string email);
    }
}
