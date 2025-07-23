using ManagementSimulator.Core.Dtos.Requests.Users;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Dtos.Responses.User;
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
        private readonly IEmployeeRoleRepository _employeeRoleRepository;
        private readonly IDeparmentRepository _deparmentRepository;

        public UserService(
            IUserRepository userRepository,
            IJobTitleRepository jobTitleRepository,
            IEmployeeRoleRepository employeeRoleRepository,
            IDeparmentRepository deparmentRepository)
        {
            _userRepository = userRepository;
            _employeeRoleRepository = employeeRoleRepository;
            _jobTitleRepository = jobTitleRepository;
            _deparmentRepository = deparmentRepository;
        }

        public async Task<List<UserResponseDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersWithReferencesAsync();
            return users.Select(u => u.ToUserResponseDto()).ToList();
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetUserWithReferencesByIdAsync(id);
            
            if(user == null)
            {
                throw new EntryNotFoundException(nameof(User), id);
            }

            return user?.ToUserResponseDto();
        }

        public async Task<UserResponseDto> AddUserAsync(CreateUserRequestDto dto)
        {
            if (await _userRepository.GetUserByEmail(dto.Email) != null)
            {
                throw new UniqueConstraintViolationException(nameof(User), nameof(User.Email));
            }

            JobTitle? jt = await _jobTitleRepository.GetFirstOrDefaultAsync(dto.JobTitleId);
            if (jt == null)
            {
                throw new EntryNotFoundException(nameof(JobTitle), dto.JobTitleId);
            }

            var user = new User
            {
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                JobTitleId = dto.JobTitleId,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Title = jt,
            };

            await _userRepository.AddAsync(user);

            foreach (var roleId in dto.EmployeeRolesId.Distinct())
            {
                var role = await _employeeRoleRepository.GetFirstOrDefaultAsync(roleId);
                if (role == null)
                {
                    throw new EntryNotFoundException(nameof(EmployeeRole), roleId);
                }

                var existingRelation = await _employeeRoleRepository
                    .GetEmployeeRoleUserAsync(user.Id, roleId);

                if (existingRelation == null)
                {
                    var employeeRoleUser = new EmployeeRoleUser
                    {
                        RolesId = roleId,
                        UsersId = user.Id,
                        Role = role,
                        User = user,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _employeeRoleRepository.AddEmployeeRoleUserAsync(employeeRoleUser);
                }
            }

            return user.ToUserResponseDto();
        }

        public async Task<UserResponseDto?> UpdateUserAsync(int id, UpdateUserRequestDto dto)
        {
            User? existing = await _userRepository.GetUserWithReferencesByIdAsync(id);
            if (existing == null)
            {
                throw new EntryNotFoundException(nameof(User), id);
            }

            if (dto.Email != null && dto.Email != string.Empty && dto.Email != existing.Email)
            {
                var userWithEmail = await _userRepository.GetUserByEmail(dto.Email);
                if (userWithEmail != null)
                {
                    throw new UniqueConstraintViolationException(nameof(User), nameof(User.Email));
                }
            }

            JobTitle? jobTitle = null;
            if (dto.JobTitleId != null)
            {
                jobTitle = await _jobTitleRepository.GetFirstOrDefaultAsync((int)dto.JobTitleId);
                if (jobTitle != null)
                {
                    existing.Title = jobTitle;
                }
                else
                {
                    throw new EntryNotFoundException(nameof(JobTitle), dto.JobTitleId);
                }
            }

            if (dto.EmployeeRolesId != null && dto.EmployeeRolesId.Any())
            {
                var existingRelations = await _employeeRoleRepository
                    .GetEmployeeRoleUsersByUserIdAsync(existing.Id);

                foreach (var relation in existingRelations)
                {
                    await _employeeRoleRepository.DeleteEmployeeUserRoleAsync(relation);
                }

                foreach (var roleId in dto.EmployeeRolesId.Distinct())
                {
                    var role = await _employeeRoleRepository.GetFirstOrDefaultAsync(roleId);
                    if (role == null)
                    {
                        throw new EntryNotFoundException(nameof(EmployeeRole), roleId);
                    }

                    var employeeRoleUser = await _employeeRoleRepository.GetEmployeeRoleUserIncludeDeletedAsync(existing.Id, roleId);
                    if(employeeRoleUser != null)
                    {
                        if (employeeRoleUser.DeletedAt != null)
                        {
                            employeeRoleUser.DeletedAt = null;
                            await _employeeRoleRepository.SaveChangesAsync();
                            continue;
                        }
                    }
                    else
                    {
                        var employeeRoleUsertToBeAdded = new EmployeeRoleUser
                        {
                            RolesId = roleId,
                            UsersId = existing.Id,
                            CreatedAt = DateTime.UtcNow,
                            ModifiedAt = DateTime.UtcNow,
                            DeletedAt = null
                        };
                        await _employeeRoleRepository.AddEmployeeRoleUserAsync(employeeRoleUsertToBeAdded);
                    }
                }
            }

            PatchHelper.PatchRequestToEntity.PatchFrom<UpdateUserRequestDto, User>(existing, dto);

            existing.ModifiedAt = DateTime.UtcNow;

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

        public async Task RestoreUserByIdAsync(int id)
        {
            var userToRestore = await _userRepository.GetUserByIdIncludeDeletedAsync(id);
            if (userToRestore == null)
            {
                throw new EntryNotFoundException(nameof(User), id);
            }

            userToRestore.DeletedAt = null;
            await _userRepository.SaveChangesAsync();
        }

        public async Task<List<UserResponseDto>> GetAllUsersIncludeRelationshipsAsync()
        {
            var users = await _userRepository.GetAllAsync();
            var userIds = users.Select(u => u.Id).ToList();
            List<int> jobTitleIds = users.Select(u => u.JobTitleId).Distinct().ToList();

            var jobTitlesTask = await _jobTitleRepository.GetJobTitlesWithDepartmentsAsync(jobTitleIds);
            var rolesTask = await _employeeRoleRepository.GetEmployeeRoleUsersByUserIdsAsync(userIds);
            var subordinatesTask = await _userRepository.GetSubordinatesByUserIdsAsync(userIds);
            var managersTask = await _userRepository.GetManagersByUserIdsAsync(userIds);

            var jobTitlesDict = jobTitlesTask.ToDictionary(jt => jt.Id);

            var userRolesDict = rolesTask
                .GroupBy(r => r.UsersId)  
                .ToDictionary(g => g.Key, g => g.ToList());

            var subordinatesDict = subordinatesTask
                .GroupBy(s => s.Id)
                .ToDictionary(g => g.Key, g => g.ToList());

            var managersDict = managersTask
                .GroupBy(m => m.Id)  
                .ToDictionary(g => g.Key, g => g.ToList());

            return users.Select(u =>
            {
                var jobTitle = jobTitlesDict.GetValueOrDefault(u.JobTitleId);

                var roles = userRolesDict.GetValueOrDefault(u.Id, new List<EmployeeRoleUser>());
                var subordinates = subordinatesDict.GetValueOrDefault(u.Id, new List<User>());
                var managers = managersDict.GetValueOrDefault(u.Id, new List<User>());

                return new UserResponseDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,

                    Roles = roles.Select(r => r.Role.Rolename).ToList(),

                    JobTitleId = u.JobTitleId,
                    JobTitleName = jobTitle?.Name ?? string.Empty,
                    DepartmentId = jobTitle?.DepartmentId ?? 0,
                    DepartmentName = jobTitle?.Department?.Name ?? string.Empty,

                    SubordinatesId = subordinates.SelectMany(u => u.Subordinates.Select(s => s.EmployeeId)).ToList(),
                    SubordinatesNames = subordinates.SelectMany(u => u.Subordinates.Select(s => $"{s.Employee.FirstName} {s.Employee.LastName}")).ToList(),
                    SubordinatesJobTitles = subordinates.SelectMany(u => u.Subordinates.Select(s => s.Employee.Title?.Name ?? string.Empty)).ToList(),
                    SubordinatesJobTitleIds = subordinates.SelectMany(u => u.Subordinates.Select(s => s.Employee.JobTitleId)).ToList()
                };
            }).ToList();
        }
    }
}
