using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagementSimulator.Core.Dtos.Requests.Users;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Mapping;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories.Intefaces;

namespace ManagementSimulator.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;

        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UserResponseDto>> GetAllUsersAsync()
        {
            var users = await _repository.GetAllAsync();
            return users.Select(u => u.ToUserResponseDto()).ToList();
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(int id)
        {
            var user = await _repository.GetFirstOrDefaultAsync(id);
            return user?.ToUserResponseDto();
        }

        public async Task<UserResponseDto> AddUserAsync(CreateUserRequestDto dto)
        {
            var user = new User
            {
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Role = dto.Role,
                JobTitleId = dto.JobTitleId,
                PasswordHash = dto.Password // Hashing should be applied in real scenarios!
            };
            await _repository.AddAsync(user);
            return user.ToUserResponseDto();
        }

        public async Task<UserResponseDto?> UpdateUserAsync(UpdateUserRequestDto dto)
        {
            var user = await _repository.GetFirstOrDefaultAsync(dto.Id);
            if (user == null)
                return null;

            user.Email = dto.Email;
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Role = dto.Role;
            user.JobTitleId = dto.JobTitleId;
            if (!string.IsNullOrEmpty(dto.Password))
                user.PasswordHash = dto.Password; // Hashing should be applied!

            user.ModifiedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(user);
            return user.ToUserResponseDto();
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}
