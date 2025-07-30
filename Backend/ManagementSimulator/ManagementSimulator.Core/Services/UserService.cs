using ManagementSimulator.Core.Dtos.Requests.Users;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Dtos.Responses.PagedResponse;
using ManagementSimulator.Core.Dtos.Responses.User;
using ManagementSimulator.Core.Mapping;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Core.Utils;
using ManagementSimulator.Database.Dtos.QueryParams;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories.Intefaces;
using ManagementSimulator.Infrastructure.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using System.Data;

namespace ManagementSimulator.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJobTitleRepository _jobTitleRepository;
        private readonly IEmployeeRoleRepository _employeeRoleRepository;
        private readonly IDeparmentRepository _deparmentRepository;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;

        public UserService(
            IUserRepository userRepository,
            IJobTitleRepository jobTitleRepository,
            IEmployeeRoleRepository employeeRoleRepository,
            IDeparmentRepository deparmentRepository,
            IEmailService emailService,
            IMemoryCache cache)
        {
            _userRepository = userRepository;
            _employeeRoleRepository = employeeRoleRepository;
            _jobTitleRepository = jobTitleRepository;
            _deparmentRepository = deparmentRepository;
            _emailService = emailService;
            _cache = cache;
        }

        public async Task<List<UserResponseDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersWithReferencesAsync(includeDeleted: true);
            return users.Select(u => new UserResponseDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName ?? string.Empty,
                LastName = u.LastName ?? string.Empty,
                Roles = u.Roles?.Select(r => r.Role.Rolename).ToList() ?? new List<string>(),
                JobTitleId = u.JobTitleId,
                JobTitleName = u.Title?.Name ?? string.Empty,
                DepartmentId = u.Title?.DepartmentId ?? 0,
                DepartmentName = u.Title?.Department?.Name ?? string.Empty,
                IsActive = u.DeletedAt == null,
            }).ToList();
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

            string temporaryPassword = PasswordGenerator.GenerateSimpleCode();

            double yearsOfEmployment = (DateTime.Now - dto.DateOfEmployment).TotalDays / 365.25;

            var user = new User
            {
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                JobTitleId = dto.JobTitleId,
                Title = jt,
                AnnuallyLeaveDays = 21 + (int)(yearsOfEmployment) / 10,
                LeaveDaysLeftCurrentYear = dto.LeaveDaysLeftCurrentYear,
                DateOfEmployment = dto.DateOfEmployment,
                MustChangePassword = true,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(temporaryPassword)
            };

            await _userRepository.AddAsync(user);

            // User shall always be at least employee
            EmployeeRoleUser eru = new EmployeeRoleUser
            {
                UsersId = user.Id,
                RolesId = await _employeeRoleRepository.GetEmployeeRoleUserByNameAsync("Employee")
            };
            await _employeeRoleRepository.AddEmployeeRoleUserAsync(eru);

            foreach (var roleId in dto.EmployeeRolesId.Distinct())
            {
                var role = await _employeeRoleRepository.GetFirstOrDefaultAsync(roleId);
                if (role == null)
                {
                    throw new EntryNotFoundException(nameof(EmployeeRole), roleId);
                }

                var existingRelation = await _employeeRoleRepository
                    .GetEmployeeRoleUserAsync(user.Id, roleId, includeDeleted: true);

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
                else
                {
                    existingRelation.DeletedAt = null;
                }
            }

            try
            {
                await _emailService.SendWelcomeEmailWithPasswordAsync(
                    user.Email,
                    user.FirstName,
                    temporaryPassword
                );
            }
            catch (Exception ex)
            {
                throw new MailNotSentException(user.Email);        
            }

            return user.ToUserResponseDto();
        }

        public async Task<bool> SendPasswordResetCodeAsync(string email)
        {
            var user = await _userRepository.GetUserByEmail(email);
            if (user == null)
            {
                return false;
            }

            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var resetCode = new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            var cacheKey = $"reset_code_{resetCode}";
            _cache.Set(cacheKey, email, TimeSpan.FromMinutes(15));

            try
            {
                await _emailService.SendPasswordResetCodeAsync(email, user.FirstName, resetCode);
                return true;
            }
            catch (Exception)
            {
                _cache.Remove(cacheKey);
                throw;
            }
        }

        public async Task<bool> ResetPasswordWithCodeAsync(string verificationCode, string newPassword)
        {
            var cacheKey = $"reset_code_{verificationCode}";

            if (_cache.TryGetValue(cacheKey, out string email))
            {
                var user = await _userRepository.GetUserByEmail(email);
                if (user != null)
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                    user.MustChangePassword = false; 
                    user.ModifiedAt = DateTime.UtcNow;

                    await _userRepository.SaveChangesAsync();

                    _cache.Remove(cacheKey);

                    return true;
                }
            }

            return false;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetUserByEmail(email);
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

                    var employeeRoleUser = await _employeeRoleRepository.GetEmployeeRoleUserAsync(existing.Id, roleId, includeDeleted: true);
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
            var userToRestore = await _userRepository.GetUserByIdAsync(id, includeDeleted: true);
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

                    SubordinatesIds = subordinates.SelectMany(u => u.Subordinates.Select(s => s.EmployeeId)).ToList(),
                    SubordinatesNames = subordinates.SelectMany(u => u.Subordinates.Select(s => $"{s.Employee.FirstName} {s.Employee.LastName}")).ToList(),
                    SubordinatesEmails = subordinates.SelectMany(u => u.Subordinates.Select(s => s.Employee.Email ?? string.Empty)).ToList(),
                    SubordinatesJobTitles = subordinates.SelectMany(u => u.Subordinates.Select(s => s.Employee.Title?.Name ?? string.Empty)).ToList(),
                    SubordinatesJobTitleIds = subordinates.SelectMany(u => u.Subordinates.Select(s => s.Employee.JobTitleId)).ToList(),
                    ManagersIds = managers.SelectMany(u => u.Managers.Select(m => m.ManagerId)).ToList(),
                };
            }).ToList();
        }

        public async Task<PagedResponseDto<UserResponseDto>> GetAllUsersIncludeRelationshipsFilteredAsync(QueriedUserRequestDto payload)
        {
            var (users, totalCount) = await _userRepository.GetAllManagersFilteredAsync(payload.LastName, payload.Email, payload.PagedQueryParams.ToQueryParams());

            if (users == null || !users.Any())
                return new PagedResponseDto<UserResponseDto>
                {
                    Data = new List<UserResponseDto>(),
                    Page = payload.PagedQueryParams.Page ?? 1,
                    PageSize = payload.PagedQueryParams.PageSize ?? 1,
                    TotalPages = 0
                };

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

            var mappedUsers = users.Select(u =>
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
                    SubordinatesIds = subordinates.SelectMany(u => u.Subordinates.Select(s => s.EmployeeId)).ToList(),
                    SubordinatesNames = subordinates.SelectMany(u => u.Subordinates.Select(s => $"{s.Employee.FirstName} {s.Employee.LastName}")).ToList(),
                    SubordinatesEmails = subordinates.SelectMany(u => u.Subordinates.Select(s => s.Employee.Email ?? string.Empty)).ToList(),
                    SubordinatesJobTitles = subordinates.SelectMany(u => u.Subordinates.Select(s => s.Employee.Title?.Name ?? string.Empty)).ToList(),
                    SubordinatesJobTitleIds = subordinates.SelectMany(u => u.Subordinates.Select(s => s.Employee.JobTitleId)).ToList(),
                    ManagersIds = managers.SelectMany(u => u.Managers.Select(m => m.ManagerId)).ToList(),
                };
            }).ToList();

            return new PagedResponseDto<UserResponseDto>
            {
                Data = mappedUsers,
                Page = payload.PagedQueryParams.Page ?? 1,
                PageSize = payload.PagedQueryParams.PageSize ?? 1,
                TotalPages = payload.PagedQueryParams.PageSize != null ?
                    (int)Math.Ceiling((double)totalCount / (int)payload.PagedQueryParams.PageSize) : 1 
            };
        }

        public async Task<PagedResponseDto<UserResponseDto>> GetAllUsersFilteredAsync(QueriedUserRequestDto payload)
        {
            var (users, totalCount) = await _userRepository.GetAllUsersWithReferencesFilteredAsync(payload.LastName, payload.Email, payload.PagedQueryParams.ToQueryParams());

            if (users == null || !users.Any())
                return new PagedResponseDto<UserResponseDto>
                {
                    Data = new List<UserResponseDto>(),
                    Page = payload.PagedQueryParams.Page ?? 1,
                    PageSize = payload.PagedQueryParams.PageSize ?? 1,
                    TotalPages = 0
                };

            return new PagedResponseDto<UserResponseDto>
            {
                Data = users.Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName ?? string.Empty,
                    LastName = u.LastName ?? string.Empty,
                    Roles = u.Roles?.Select(r => r.Role.Rolename).ToList() ?? new List<string>(),
                    JobTitleId = u.JobTitleId,
                    JobTitleName = u.Title?.Name ?? string.Empty,
                    DepartmentId = u.Title?.DepartmentId ?? 0,
                    DepartmentName = u.Title?.Department?.Name ?? string.Empty,
                    IsActive = u.DeletedAt == null,
                }),
                Page = payload.PagedQueryParams.Page ?? 1,
                PageSize = payload.PagedQueryParams.PageSize ?? 1,
                TotalPages = payload.PagedQueryParams.PageSize != null ?
                    (int)Math.Ceiling((double)totalCount / (int)payload.PagedQueryParams.PageSize) : 1
            };
        }

        public async Task<List<User>> GetAllAdminsAsync(string? lastName, string? email)
        {
            return await _userRepository.GetAllAdminsAsync(lastName, email);
        }

        public async Task<PagedResponseDto<UserResponseDto>> GetAllUnassignedUsersFilteredAsync(int page, int pageSize)
        {
            var (unassignedUsers, totalCount) = await _userRepository.GetAllUnassignedUsersFilteredAsync(new QueryParams { Page = page, PageSize = pageSize });

            if (unassignedUsers == null || !unassignedUsers.Any())
                return new PagedResponseDto<UserResponseDto>
                {
                    Data = new List<UserResponseDto>(),
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = 0
                };

            return new PagedResponseDto<UserResponseDto>
            {
                Data = unassignedUsers.Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Roles = u.Roles.Select(r => r.Role.Rolename).ToList(),
                    JobTitleId = u.JobTitleId,
                    JobTitleName = u.Title?.Name ?? string.Empty,
                    DepartmentId = u.Title?.DepartmentId ?? 0,
                    DepartmentName = u.Title?.Department?.Name ?? string.Empty,
                }),
                Page = page,
                PageSize = pageSize,
                TotalPages = pageSize > 0 ?
                    (int)Math.Ceiling((double)totalCount / pageSize) : 1 
            };
        }

        public async Task<PagedResponseDto<UserResponseDto>> GetAllManagersFilteredAsync(QueriedUserRequestDto payload)
        {
            (List<User>? managers, int totalCount) = await _userRepository.GetAllManagersFilteredAsync(payload.LastName, payload.Email, payload.PagedQueryParams.ToQueryParams(), includeDeleted: false);

            if (managers == null || !managers.Any())
                return new PagedResponseDto<UserResponseDto>
                {
                    Data = new List<UserResponseDto>(),
                    Page = payload.PagedQueryParams.Page ?? 1,
                    PageSize = payload.PagedQueryParams.PageSize ?? 1,
                    TotalPages = 0
                };

            return new PagedResponseDto<UserResponseDto>
            {
                Data = managers.Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName ?? string.Empty,
                    LastName = u.LastName ?? string.Empty,
                    Roles = u.Roles?.Select(r => r.Role.Rolename).ToList() ?? new List<string>(),
                    JobTitleId = u.JobTitleId,
                    JobTitleName = u.Title?.Name ?? string.Empty,
                    DepartmentId = u.Title?.DepartmentId ?? 0,
                    DepartmentName = u.Title?.Department?.Name ?? string.Empty,
                    SubordinatesIds = u.Subordinates.Select(u => u.EmployeeId).ToList(),
                    IsActive = u.DeletedAt == null,
                }),
                Page = payload.PagedQueryParams.Page ?? 1,
                PageSize = payload.PagedQueryParams.PageSize ?? 1,
                TotalPages = payload.PagedQueryParams.PageSize != null ?
                    (int)Math.Ceiling((double)totalCount / (int)payload.PagedQueryParams.PageSize) : 1
            };
        }
    }
}
