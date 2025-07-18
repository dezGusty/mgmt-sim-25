using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using ManagementSimulator.Core.Dtos.Requests.Users;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Mapping;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories.Intefaces;
using ManagementSimulator.Infrastructure.Exceptions;

namespace ManagementSimulator.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJobTitleRepository _jobTitleRepository;

        public UserService(IUserRepository userRepository,IJobTitleRepository jobTitleRepository)
        {
            _userRepository = userRepository;
            _jobTitleRepository = jobTitleRepository;
        }

        public async Task<List<UserResponseDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(u => u.ToUserResponseDto()).ToList();
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetFirstOrDefaultAsync(id);
            
            if(user == null)
            {
                throw new EntryNotFoundException(nameof(User), id);
            }

            return user?.ToUserResponseDto();
        }

        public async Task<UserResponseDto> AddUserAsync(CreateUserRequestDto dto)
        {
            if(await _userRepository.GetUserByEmail(dto.Email) != null)
            {
                throw new UniqueConstraintViolationException(nameof(User), nameof(User.Email));
            }    

            if(await _jobTitleRepository.GetFirstOrDefaultAsync(dto.JobTitleId) == null)
            {
                throw new EntryNotFoundException(nameof(JobTitle), dto.JobTitleId);
            }

            var user = new User
            {
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Role = dto.Role,
                JobTitleId = dto.JobTitleId,
                PasswordHash = dto.Password // Hashing should be applied in real scenarios!
            };

            await _userRepository.AddAsync(user);
            return user.ToUserResponseDto();
        }

        public async Task<UserResponseDto?> UpdateUserAsync(int id, UpdateUserRequestDto dto)
        {
            if (await _userRepository.GetUserByEmail(dto.Email) != null)
            {
                throw new UniqueConstraintViolationException(nameof(User), nameof(User.Email));
            }

            if (await _jobTitleRepository.GetFirstOrDefaultAsync(dto.JobTitleId) == null)
            {
                throw new EntryNotFoundException(nameof(JobTitle), dto.JobTitleId);
            }

            User user = new User()
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Role = dto.Role,
                JobTitleId = dto.JobTitleId
            };
            
            if (!string.IsNullOrEmpty(dto.Password))
                user.PasswordHash = dto.Password;

            await _userRepository.UpdateAsync(user);
            return user.ToUserResponseDto();
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            if (await _userRepository.GetFirstOrDefaultAsync(id) == null)
            {
                throw new EntryNotFoundException(nameof(User), id);
            }

            return await _userRepository.DeleteAsync(id);
        }
    }
}
