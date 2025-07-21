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
                JobTitleId = dto.JobTitleId,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            // creezi relațiile din tabela intermediară
            foreach (var managerId in dto.ManagerIds)
            {
                var userManager = new EmployeeManager
                {
                    EmployeeId = user.Id,
                    ManagerId = managerId
                };
                //_context.UserManagers.Add(userManager);
            }

            await _userRepository.AddAsync(user);
            return user.ToUserResponseDto();
        }

        public async Task<UserResponseDto?> UpdateUserAsync(int id, UpdateUserRequestDto dto)
        {
            User? existing = await _userRepository.GetFirstOrDefaultAsync(id);

            if(existing == null)
            {
                throw new EntryNotFoundException(nameof(User), id);
            }

            if (dto.Email != null && dto.Email != string.Empty && await _userRepository.GetUserByEmail(dto.Email) != null)
            {
                throw new UniqueConstraintViolationException(nameof(User), nameof(User.Email));
            }

            if (dto.JobTitleId != null && await _jobTitleRepository.GetFirstOrDefaultAsync((int)dto.JobTitleId) == null)
            {
                throw new EntryNotFoundException(nameof(JobTitle), dto.JobTitleId);
            }

            PatchHelper.PatchRequestToEntity.PatchFrom<UpdateUserRequestDto, User>(existing, dto);

            await _userRepository.SaveChangesAsync();
            return existing.ToUserResponseDto();
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
