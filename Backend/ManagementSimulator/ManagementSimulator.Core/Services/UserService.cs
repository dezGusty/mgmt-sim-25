using ManagementSimulator.Core.Dtos.Requests.Users;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Dtos.Responses.PagedResponse;
using ManagementSimulator.Core.Dtos.Responses.User;
using ManagementSimulator.Core.Dtos.Responses.Users;
using ManagementSimulator.Core.Mapping;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Core.Utils;
using ManagementSimulator.Database.Dtos.QueryParams;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories.Intefaces;
using ManagementSimulator.Infrastructure.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using System.Data;
using ManagementSimulator.Core.Dtos.Responses.LeaveRequest;
using ManagementSimulator.Database.Enums;
using ManagementSimulator.Database.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace ManagementSimulator.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJobTitleRepository _jobTitleRepository;
        private readonly IEmployeeRoleRepository _employeeRoleRepository;
        private readonly IDeparmentRepository _deparmentRepository;
        private readonly IEmailService _emailService;
        private readonly IEmployeeManagerService _employeeManagerService;
        private readonly IMemoryCache _cache;
        private readonly ILeaveRequestTypeRepository _leaveRequestTypeRepository;
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly MGMTSimulatorDbContext _dbContext;
    private readonly IAvailabilityService _availabilityService;
    private readonly IAuditLogService _auditLogService;
    private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWeekendService _weekendService;

        public UserService(
            IUserRepository userRepository,
            IJobTitleRepository jobTitleRepository,
            IEmployeeRoleRepository employeeRoleRepository,
            IDeparmentRepository deparmentRepository,
            IEmailService emailService,
            IEmployeeManagerService employeeManagerService,
            IMemoryCache cache,
            ILeaveRequestTypeRepository leaveRequestTypeRepository,
            ILeaveRequestRepository leaveRequestRepository,
            MGMTSimulatorDbContext dbContext,
            IAvailabilityService availabilityService,
            IAuditLogService auditLogService,
            IHttpContextAccessor httpContextAccessor,
            IWeekendService weekendService)
        {
            _userRepository = userRepository;
            _employeeRoleRepository = employeeRoleRepository;
            _jobTitleRepository = jobTitleRepository;
            _deparmentRepository = deparmentRepository;
            _emailService = emailService;
            _employeeManagerService = employeeManagerService;
            _cache = cache;
            _leaveRequestTypeRepository = leaveRequestTypeRepository;
            _leaveRequestRepository = leaveRequestRepository;
            _dbContext = dbContext;
            _availabilityService = availabilityService;
            _auditLogService = auditLogService;
            _httpContextAccessor = httpContextAccessor;
            _weekendService = weekendService;
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
                DepartmentId = u.DepartmentId,
                DepartmentName = u.Department?.Name ?? string.Empty,
                IsActive = u.DeletedAt == null,
                Vacation = u.Vacation,
            }).ToList();
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetUserWithReferencesByIdAsync(id);

            if (user == null)
            {
                throw new EntryNotFoundException(nameof(User), id);
            }

            return user?.ToUserResponseDto();
        }

        public async Task<UserResponseDto> AddUserAsync(CreateUserRequestDto dto)
        {
            if (await _userRepository.GetUserByEmail(dto.Email, includeDeleted: true) != null)
            {
                throw new UniqueConstraintViolationException(nameof(User), nameof(User.Email));
            }

            JobTitle? jt = await _jobTitleRepository.GetFirstOrDefaultAsync(dto.JobTitleId);
            if (jt == null)
            {
                throw new EntryNotFoundException(nameof(JobTitle), dto.JobTitleId);
            }

            string temporaryPassword = PasswordGenerator.GenerateSimpleCode();

            var user = new User
            {
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                JobTitleId = dto.JobTitleId,
                DepartmentId = dto.DepartmentId,
                Title = jt,
                DateOfEmployment = dto.DateOfEmployment,
                MustChangePassword = true,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(temporaryPassword),
                Vacation = dto.Vacation ?? 21,
                EmploymentType = dto.EmploymentType
            };

            await _userRepository.AddAsync(user);

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
                    .GetEmployeeRoleUserAsync(user.Id, roleId, includeDeleted: true, tracking: true);

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
            catch (Exception)
            {
                throw new MailNotSentException(user.Email);
            }

            // Update availability based on employment type
            await _availabilityService.UpdateUserAvailabilityAsync(user.Id);

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

            if (_cache.TryGetValue(cacheKey, out string? email) && !string.IsNullOrEmpty(email))
            {
                var user = await _userRepository.GetUserByEmail(email, tracking: true);
                if (user != null)
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                    user.MustChangePassword = false;

                    await _userRepository.UpdateAsync(user);

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

        public async Task<PagedResponseDto<HrUserResponseDto>> GetAllUsersForHrAsync(HrUsersRequestDto request)
        {
            var year = request.Year ?? DateTime.Now.Year;

            var users = await _userRepository.GetAllUsersWithReferencesAsync(includeDeleted: false);

            var filteredUsers = users.Where(u => u.DeletedAt == null).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Department))
            {
                var departmentLower = request.Department.ToLower();
                filteredUsers = filteredUsers.Where(u =>
                    u.Department != null &&
                    u.Department.Name.ToLower().Contains(departmentLower));
            }

            var totalCount = filteredUsers.Count();

            var pageSize = request.PageSize ?? 10;
            var page = request.Page ?? 1;

            var pagedUsers = filteredUsers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var leaveRequestTypes = await _leaveRequestTypeRepository.GetAllAsync();

            var hrUserDtos = new List<HrUserResponseDto>();

            foreach (var user in pagedUsers)
            {
                var hrUserDto = await MapToHrUserResponseDto(user, year, leaveRequestTypes);
                hrUserDtos.Add(hrUserDto);
            }

            return new PagedResponseDto<HrUserResponseDto>
            {
                Data = hrUserDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        private async Task<HrUserResponseDto> MapToHrUserResponseDto(User user, int year, List<LeaveRequestType> leaveRequestTypes)
        {
            var hrUserDto = new HrUserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = user.Roles?.Select(r => r.Role.Rolename).ToList() ?? new List<string>(),
                JobTitleId = user.JobTitleId,
                JobTitleName = user.Title?.Name ?? string.Empty,
                DepartmentId = user.DepartmentId,
                DepartmentName = user.Department?.Name ?? string.Empty,
                IsActive = user.DeletedAt == null,
                DateOfEmployment = user.DateOfEmployment,
                Vacation = user.Vacation
            };

            var startOfYear = new DateTime(year, 1, 1);
            var endOfYear = new DateTime(year, 12, 31);

            var userLeaveRequests = await _leaveRequestRepository.GetAllWithRelationshipsAsync();
            var yearLeaveRequests = userLeaveRequests
                .Where(lr => lr.UserId == user.Id &&
                           lr.StartDate <= endOfYear &&
                           lr.EndDate >= startOfYear)
                .ToList();

            hrUserDto.TotalLeaveDays = user.Vacation;
            var approvedRequests = yearLeaveRequests.Where(lr => lr.RequestStatus == RequestStatus.Approved);
            var pendingRequests = yearLeaveRequests.Where(lr => lr.RequestStatus == RequestStatus.Pending);

            hrUserDto.UsedLeaveDays = CalculateTotalDays(pendingRequests);
            hrUserDto.RemainingLeaveDays = hrUserDto.TotalLeaveDays - hrUserDto.UsedLeaveDays;


            
            hrUserDto.LeaveTypeStatistics = new List<LeaveTypeStatDto>();

            foreach (var leaveType in leaveRequestTypes)
            {
                var typeRequests = yearLeaveRequests.Where(lr => lr.LeaveRequestTypeId == leaveType.Id);
                var pendingTypeRequests = typeRequests.Where(lr => lr.RequestStatus == RequestStatus.Pending);

                var usedDaysForType = CalculateTotalDays(pendingTypeRequests);
                var maxAllowedDays = leaveType.Title == "Vacation"
                    ? user.Vacation
                    : (leaveType.MaxDays ?? hrUserDto.TotalLeaveDays);

                var leaveTypeStat = new LeaveTypeStatDto
                {
                    LeaveTypeId = leaveType.Id,
                    LeaveTypeName = leaveType.Title,
                    UsedDays = usedDaysForType,
                    RemainingDays = maxAllowedDays - usedDaysForType,
                    MaxAllowedDays = maxAllowedDays
                };

                hrUserDto.LeaveTypeStatistics.Add(leaveTypeStat);
            }

            return hrUserDto;
        }

        private int CalculateTotalDays(IEnumerable<LeaveRequest> leaveRequests)
        {
            int totalDays = 0;

            foreach (var request in leaveRequests)
            {
                var workingDays = _weekendService.CountWorkingDays(request.StartDate.Date, request.EndDate.Date, new HashSet<DateTime>());
                totalDays += workingDays;
            }

            return totalDays;
        }

        public async Task<UserResponseDto?> UpdateUserAsync(int id, UpdateUserRequestDto dto)
        {
            User? existing = await _userRepository.GetUserWithReferencesByIdAsync(id, tracking: true);
            if (existing == null)
            {
                throw new EntryNotFoundException(nameof(User), id);
            }

            // Snapshot of the existing user state (old values) for audit logging
            var oldSnapshot = new
            {
                Id = existing.Id,
                Email = existing.Email,
                FirstName = existing.FirstName,
                LastName = existing.LastName,
                Roles = existing.Roles?.Select(r => r.Role?.Rolename).ToList() ?? new List<string?>(),
                JobTitleId = existing.JobTitleId,
                DepartmentId = existing.DepartmentId,
                Vacation = existing.Vacation,
                EmploymentType = existing.EmploymentType
            };

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

            if (dto.EmployeeRolesId != null)
            {
                var existingRelations = await _employeeRoleRepository
                    .GetEmployeeRoleUsersByUserIdAsync(existing.Id, tracking: false);

                int employeeRole = await _employeeRoleRepository.GetEmployeeRoleUserByNameAsync("Employee");

                var roleIdsToKeep = new HashSet<int>();
                roleIdsToKeep.Add(employeeRole); // Always keep Employee role

                if (dto.EmployeeRolesId.Any())
                {
                    var validRoleIds = dto.EmployeeRolesId.Where(id => id > 0).Distinct();
                    foreach (var roleId in validRoleIds)
                    {
                        var role = await _employeeRoleRepository.GetFirstOrDefaultAsync(roleId);
                        if (role == null)
                        {
                            throw new EntryNotFoundException(nameof(EmployeeRole), roleId);
                        }
                        roleIdsToKeep.Add(roleId);
                    }
                }

                foreach (var relation in existingRelations)
                {
                    if (!roleIdsToKeep.Contains(relation.RolesId))
                    {
                        var trackedRelation = await _employeeRoleRepository.GetEmployeeRoleUserAsync(existing.Id, relation.RolesId, includeDeleted: false, tracking: true);
                        if (trackedRelation != null)
                        {
                            await _employeeRoleRepository.DeleteEmployeeUserRoleAsync(trackedRelation);
                        }
                    }
                }

                if (dto.EmployeeRolesId.Any())
                {
                    var validRoleIds = dto.EmployeeRolesId.Where(id => id > 0).Distinct();
                    foreach (var roleId in validRoleIds)
                    {
                        if (roleId == employeeRole) continue;

                        var existingRelation = existingRelations.FirstOrDefault(r => r.RolesId == roleId && r.DeletedAt == null);
                        if (existingRelation == null)
                        {
                            var deletedRelation = await _employeeRoleRepository.GetEmployeeRoleUserAsync(existing.Id, roleId, includeDeleted: true, tracking: true);
                            if (deletedRelation != null && deletedRelation.DeletedAt != null)
                            {
                                deletedRelation.DeletedAt = null;
                                deletedRelation.ModifiedAt = DateTime.UtcNow;
                                await _employeeRoleRepository.SaveChangesAsync();
                            }
                            else if (deletedRelation == null)
                            {
                                var employeeRoleUserToBeAdded = new EmployeeRoleUser
                                {
                                    RolesId = roleId,
                                    UsersId = existing.Id,
                                    CreatedAt = DateTime.UtcNow,
                                    ModifiedAt = DateTime.UtcNow,
                                    DeletedAt = null
                                };
                                await _employeeRoleRepository.AddEmployeeRoleUserAsync(employeeRoleUserToBeAdded);
                            }
                        }
                    }
                }
            }

            PatchHelper.PatchRequestToEntity.PatchFrom<UpdateUserRequestDto, User>(existing, dto);

            if (dto.Vacation.HasValue)
            {
                existing.Vacation = dto.Vacation.Value;
            }

            await _userRepository.UpdateAsync(existing);

            // Update availability if employment type changed or other relevant fields
            if (dto.EmploymentType.HasValue)
            {
                await _availabilityService.UpdateUserAvailabilityAsync(existing.Id);
            }

            // Capture new state and log update to audit log service so OldValues/NewValues are stored
            try
            {
                var newSnapshot = new
                {
                    Id = existing.Id,
                    Email = existing.Email,
                    FirstName = existing.FirstName,
                    LastName = existing.LastName,
                    Roles = existing.Roles?.Select(r => r.Role?.Rolename).ToList() ?? new List<string?>(),
                    JobTitleId = existing.JobTitleId,
                    DepartmentId = existing.DepartmentId,
                    Vacation = existing.Vacation,
                    EmploymentType = existing.EmploymentType
                };

                await _auditLogService.LogUpdateAsync(oldSnapshot, newSnapshot, _httpContextAccessor?.HttpContext?.User, _httpContextAccessor?.HttpContext);
            }
            catch
            {
                // Swallow audit logging exceptions to avoid impacting main flow
            }

            return existing.ToUserResponseDto();
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            if (await _userRepository.GetFirstOrDefaultAsync(id) == null)
            {
                throw new EntryNotFoundException(nameof(User), id);
            }

            try
            {
                await _employeeManagerService.SetSubordinatesToUnassignedAsync(id);
            }
            catch (EntryNotFoundException)
            {
            }

            return await _userRepository.DeleteAsync(id);
        }

        public async Task RestoreUserByIdAsync(int id)
        {
            var userToRestore = await _userRepository.GetUserByIdAsync(id, includeDeleted: true, tracking: true);
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
            List<int> departmentsIds = users.Select(u => u.DepartmentId).Distinct().ToList();

            var jobTitles = await _jobTitleRepository.GetJobTitlesAsync(jobTitleIds);
            var departments = await _deparmentRepository.GetAllDepartmentsAsync(departmentsIds);
            var roles = await _employeeRoleRepository.GetEmployeeRoleUsersByUserIdsAsync(userIds);
            var subordinates = await _userRepository.GetSubordinatesByUserIdsAsync(userIds);
            var managers = await _userRepository.GetManagersByUserIdsAsync(userIds);

            var jobTitlesDict = jobTitles.ToDictionary(jt => jt.Id);
            var departmentsDict = departments.ToDictionary(d => d.Id);

            var userRolesDict = roles
                .GroupBy(r => r.UsersId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var subordinatesDict = subordinates
                .GroupBy(s => s.Id)
                .ToDictionary(g => g.Key, g => g.ToList());

            var managersDict = managers
                .GroupBy(m => m.Id)
                .ToDictionary(g => g.Key, g => g.ToList());


            return users.Select(u =>
            {
                var jobTitle = jobTitlesDict.GetValueOrDefault(u.JobTitleId);
                var department = departmentsDict.GetValueOrDefault(u.DepartmentId);

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
                    DepartmentId = department?.Id ?? 0,
                    DepartmentName = department?.Name ?? string.Empty,

                    SubordinatesIds = subordinates.SelectMany(u => u.Subordinates.Select(s => s.EmployeeId)).ToList(),
                    SubordinatesNames = subordinates.SelectMany(u => u.Subordinates.Select(s => $"{s.Employee.FirstName} {s.Employee.LastName}")).ToList(),
                    SubordinatesEmails = subordinates.SelectMany(u => u.Subordinates.Select(s => s.Employee.Email ?? string.Empty)).ToList(),
                    SubordinatesJobTitles = subordinates.SelectMany(u => u.Subordinates.Select(s => s.Employee.Title?.Name ?? string.Empty)).ToList(),
                    SubordinatesJobTitleIds = subordinates.SelectMany(u => u.Subordinates.Select(s => s.Employee.JobTitleId)).ToList(),
                    ManagersIds = managers.SelectMany(u => u.Managers.Select(m => m.ManagerId)).ToList(),
                    Vacation = u.Vacation,
                };
            }).ToList();
        }

        public async Task<PagedResponseDto<UserResponseDto>> GetAllUsersIncludeRelationshipsFilteredAsync(QueriedUserRequestDto payload)
        {
            var (users, totalCount) = await _userRepository.GetAllManagersWithTeamsFilteredAsync(
                payload.GlobalSearch,
                payload.ManagerName,
                payload.EmployeeName,
                payload.ManagerEmail,
                payload.EmployeeEmail,
                payload.JobTitle,
                payload.Department,
                payload.PagedQueryParams.ToQueryParams());

            if (users == null || !users.Any())
                return new PagedResponseDto<UserResponseDto>
                {
                    Data = new List<UserResponseDto>(),
                    Page = payload.PagedQueryParams.Page ?? 1,
                    PageSize = payload.PagedQueryParams.PageSize ?? 1,
                    TotalCount = totalCount,
                    TotalPages = 0
                };

            var userIds = users.Select(u => u.Id).ToList();
            List<int> jobTitleIds = users.Select(u => u.JobTitleId).Distinct().ToList();
            List<int> departmentIds = users.Select(u => u.DepartmentId).Distinct().ToList();

            var jobTitles = await _jobTitleRepository.GetJobTitlesAsync(jobTitleIds);
            var departments = await _deparmentRepository.GetAllDepartmentsAsync(departmentIds);
            var roles = await _employeeRoleRepository.GetEmployeeRoleUsersByUserIdsAsync(userIds);
            var subordinates = await _userRepository.GetSubordinatesByUserIdsAsync(userIds);
            var managers = await _userRepository.GetManagersByUserIdsAsync(userIds);

            var jobTitlesDict = jobTitles.ToDictionary(jt => jt.Id);
            var departmentsDict = departments.ToDictionary(d => d.Id);

            var userRolesDict = roles
                .GroupBy(r => r.UsersId)
                .ToDictionary(g => g.Key, g => g.ToList());
            var subordinatesDict = subordinates
                .GroupBy(s => s.Id)
                .ToDictionary(g => g.Key, g => g.ToList());
            var managersDict = managers
                .GroupBy(m => m.Id)
                .ToDictionary(g => g.Key, g => g.ToList());

            var mappedUsers = users.Select(u =>
            {
                var jobTitle = jobTitlesDict.GetValueOrDefault(u.JobTitleId);
                var department = departmentsDict.GetValueOrDefault(u.DepartmentId);

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
                    DepartmentId = u.DepartmentId,
                    DepartmentName = department?.Name ?? string.Empty,
                    SubordinatesIds = subordinates.SelectMany(u => u.Subordinates.Where(s => s.DeletedAt == null).Select(s => s.EmployeeId)).ToList(),
                    SubordinatesNames = subordinates.SelectMany(u => u.Subordinates.Where(s => s.DeletedAt == null).Select(s => $"{s.Employee.FirstName} {s.Employee.LastName}")).ToList(),
                    SubordinatesEmails = subordinates.SelectMany(u => u.Subordinates.Where(s => s.DeletedAt == null).Select(s => s.Employee.Email ?? string.Empty)).ToList(),
                    SubordinatesJobTitles = subordinates.SelectMany(u => u.Subordinates.Where(s => s.DeletedAt == null).Select(s => s.Employee.Title?.Name ?? string.Empty)).ToList(),
                    SubordinatesJobTitleIds = subordinates.SelectMany(u => u.Subordinates.Where(s => s.DeletedAt == null).Select(s => s.Employee.JobTitleId)).ToList(),
                    ManagersIds = managers.SelectMany(u => u.Managers.Where(s => s.DeletedAt == null).Select(m => m.ManagerId)).ToList(),
                };
            }).ToList();

            return new PagedResponseDto<UserResponseDto>
            {
                Data = mappedUsers,
                Page = payload.PagedQueryParams.Page ?? 1,
                PageSize = payload.PagedQueryParams.PageSize ?? 1,
                TotalCount = totalCount,
                TotalPages = payload.PagedQueryParams.PageSize != null ?
                    (int)Math.Ceiling((double)totalCount / (int)payload.PagedQueryParams.PageSize) : 1
            };
        }

        public async Task<PagedResponseDto<UserResponseDto>> GetAllUsersFilteredAsync(QueriedUserRequestDto payload)
        {
            if (payload.ActivityStatus != null && payload.ActivityStatus == Enums.UserActivityStatus.INACTIVE)
            {
                var (deletedUsers, deletedTotalCount) = await _userRepository.GetAllInactiveUsersWithReferencesFilteredAsync(payload.Name, payload.Email, payload.Department, payload.JobTitle, payload.GlobalSearch, payload.PagedQueryParams.ToQueryParams());

                if (deletedUsers == null || !deletedUsers.Any())
                    return new PagedResponseDto<UserResponseDto>
                    {
                        Data = new List<UserResponseDto>(),
                        Page = payload.PagedQueryParams.Page ?? 1,
                        PageSize = payload.PagedQueryParams.PageSize ?? 1,
                        TotalPages = 0
                    };

                return new PagedResponseDto<UserResponseDto>
                {
                    Data = deletedUsers.Select(u => new UserResponseDto
                    {
                        Id = u.Id,
                        Email = u.Email,
                        FirstName = u.FirstName ?? string.Empty,
                        LastName = u.LastName ?? string.Empty,
                        Roles = u.Roles?.Select(r => r.Role.Rolename).ToList() ?? new List<string>(),
                        JobTitleId = u.JobTitleId,
                        JobTitleName = u.Title?.Name ?? string.Empty,
                        DepartmentId = u.DepartmentId,
                        DepartmentName = u.Department?.Name ?? string.Empty,
                        IsActive = u.DeletedAt == null,
                        Vacation = u.Vacation,
                    }),
                    Page = payload.PagedQueryParams.Page ?? 1,
                    PageSize = payload.PagedQueryParams.PageSize ?? 1,
                    TotalPages = payload.PagedQueryParams.PageSize != null ?
                        (int)Math.Ceiling((double)deletedTotalCount / (int)payload.PagedQueryParams.PageSize) : 1
                };
            }

            bool includeDeleted = payload.ActivityStatus == null || payload.ActivityStatus == Enums.UserActivityStatus.ALL;

            var (users, totalCount) = await _userRepository.GetAllUsersWithReferencesFilteredAsync(payload.Name, payload.Email, payload.Department, payload.JobTitle, payload.GlobalSearch, payload.PagedQueryParams.ToQueryParams(), includeDeleted: includeDeleted);

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
                    DepartmentId = u.DepartmentId,
                    DepartmentName = u.Department?.Name ?? string.Empty,
                    IsActive = u.DeletedAt == null,
                    Vacation = u.Vacation,
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

        public async Task<PagedResponseDto<UserResponseDto>> GetAllAdminsFilteredAsync(QueriedUserRequestDto payload)
        {
            (List<User>? admins, int totalCount) = await _userRepository.GetAllAdminsFilteredAsync(
                payload.GlobalSearch,
                payload.Name,
                payload.Email,
                payload.JobTitle,
                payload.Department,
                payload.PagedQueryParams.ToQueryParams(),
                includeDeleted: false);

            if (admins == null || !admins.Any())
                return new PagedResponseDto<UserResponseDto>
                {
                    Data = new List<UserResponseDto>(),
                    Page = payload.PagedQueryParams.Page ?? 1,
                    PageSize = payload.PagedQueryParams.PageSize ?? 1,
                    TotalCount = totalCount,
                    TotalPages = 0
                };

            return new PagedResponseDto<UserResponseDto>
            {
                Data = admins.Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName ?? string.Empty,
                    LastName = u.LastName ?? string.Empty,
                    Roles = u.Roles?.Select(r => r.Role.Rolename).ToList() ?? new List<string>(),
                    JobTitleId = u.JobTitleId,
                    JobTitleName = u.Title?.Name ?? string.Empty,
                    DepartmentId = u.DepartmentId,
                    DepartmentName = u.Department?.Name ?? string.Empty,
                    IsActive = u.DeletedAt == null,
                    Vacation = u.Vacation,
                }),
                Page = payload.PagedQueryParams.Page ?? 1,
                PageSize = payload.PagedQueryParams.PageSize ?? 1,
                TotalCount = totalCount,
                TotalPages = payload.PagedQueryParams.PageSize != null ?
                    (int)Math.Ceiling((double)totalCount / (int)payload.PagedQueryParams.PageSize) : 1
            };
        }

        public async Task<PagedResponseDto<UserResponseDto>> GetAllUnassignedUsersFilteredAsync(QueriedUserRequestDto payload)
        {
            var queryParams = new QueryParams
            {
                Page = payload.PagedQueryParams.Page ?? 1,
                PageSize = payload.PagedQueryParams.PageSize ?? 10,
                SortBy = payload.PagedQueryParams.SortBy
            };

            var (unassignedUsers, totalCount) = await _userRepository.GetAllUnassignedUsersFilteredAsync(
                queryParams,
                payload.GlobalSearch,
                payload.UnassignedName,
                payload.JobTitle);

            if (unassignedUsers == null || !unassignedUsers.Any())
                return new PagedResponseDto<UserResponseDto>
                {
                    Data = new List<UserResponseDto>(),
                    Page = payload.PagedQueryParams.Page ?? 1,
                    PageSize = payload.PagedQueryParams.PageSize ?? 10,
                    TotalCount = totalCount,
                    TotalPages = 0
                };

            var pageSize = payload.PagedQueryParams.PageSize ?? 10;
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
                    DepartmentId = u.DepartmentId,
                    DepartmentName = u.Department?.Name ?? string.Empty,
                    Vacation = u.Vacation,
                }),
                Page = payload.PagedQueryParams.Page ?? 1,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = pageSize > 0 ?
                    (int)Math.Ceiling((double)totalCount / pageSize) : 1
            };
        }

        public async Task<PagedResponseDto<UserResponseDto>> GetAllManagersFilteredAsync(QueriedUserRequestDto payload)
        {
            (List<User>? managers, int totalCount) = await _userRepository.GetAllManagersFilteredAsync(
                payload.GlobalSearch,
                payload.ManagerName,
                payload.EmployeeName,
                payload.ManagerEmail,
                payload.EmployeeEmail,
                payload.JobTitle,
                payload.Department,
                payload.PagedQueryParams.ToQueryParams(),
                includeDeleted: false);

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
                    DepartmentId = u.DepartmentId,
                    DepartmentName = u.Department?.Name ?? string.Empty,
                    SubordinatesIds = u.Subordinates.Select(u => u.EmployeeId).ToList(),
                    IsActive = u.DeletedAt == null,
                    Vacation = u.Vacation,
                }),
                Page = payload.PagedQueryParams.Page ?? 1,
                PageSize = payload.PagedQueryParams.PageSize ?? 1,
                TotalPages = payload.PagedQueryParams.PageSize != null ?
                    (int)Math.Ceiling((double)totalCount / (int)payload.PagedQueryParams.PageSize) : 1
            };
        }

        public async Task<int> AdjustUserVacationAsync(int userId, int daysDelta)
        {
            var user = await _userRepository.GetUserByIdAsync(userId, tracking: true);
            if (user == null)
            {
                throw new EntryNotFoundException(nameof(User), userId);
            }

            checked
            {
                user.Vacation = user.Vacation + daysDelta;
            }

            if (user.Vacation < 0)
            {
                user.Vacation = 0;
            }

            await _userRepository.UpdateAsync(user);
            return user.Vacation;
        }

        public async Task<int> GetTotalAdminsCountAsync()
        {
            return await _userRepository.GetTotalAdminsCountAsync(includeDeleted: false);
        }

        public async Task<int> GetTotalManagersCountAsync()
        {
            return await _userRepository.GetTotalManagersCountAsync(includeDeleted: false);
        }

        public async Task<int> GetTotalUnassignedUsersCountAsync()
        {
            return await _userRepository.GetTotalUnassignedUsersCountAsync(includeDeleted: false);
        }

        public async Task<GlobalSearchResponseDto> GlobalSearchAsync(GlobalSearchRequestDto request)
        {
            var response = new GlobalSearchResponseDto();

            var shouldFetchManagers = string.IsNullOrEmpty(request.SearchCategory) ||
                                     request.SearchCategory == "Global" ||
                                     request.SearchCategory == "Managers";

            var shouldFetchAdmins = string.IsNullOrEmpty(request.SearchCategory) ||
                                   request.SearchCategory == "Global" ||
                                   request.SearchCategory == "Admins";

            var shouldFetchUnassigned = string.IsNullOrEmpty(request.SearchCategory) ||
                                       request.SearchCategory == "Global" ||
                                       request.SearchCategory == "Unassigned";

            if (shouldFetchManagers)
            {
                var managersRequest = new QueriedUserRequestDto
                {
                    GlobalSearch = request.GlobalSearch,
                    PagedQueryParams = request.ManagersPagedParams
                };

                response.Managers = await GetAllUsersIncludeRelationshipsFilteredAsync(managersRequest);
            }

            if (shouldFetchAdmins)
            {
                var adminsRequest = new QueriedUserRequestDto
                {
                    GlobalSearch = request.GlobalSearch,
                    PagedQueryParams = request.AdminsPagedParams
                };

                response.Admins = await GetAllAdminsFilteredAsync(adminsRequest);
            }

            if (shouldFetchUnassigned)
            {
                var unassignedRequest = new QueriedUserRequestDto
                {
                    GlobalSearch = request.GlobalSearch,
                    PagedQueryParams = request.UnassignedUsersPagedParams
                };

                response.UnassignedUsers = await GetAllUnassignedUsersFilteredAsync(unassignedRequest);
            }

            if (request.IncludeTotalCounts)
            {
                // Get system-wide totals (not affected by search)
                var systemTotalAdmins = await GetTotalAdminsCountAsync();
                var systemTotalManagers = await GetTotalManagersCountAsync();
                var systemTotalUnassigned = await GetTotalUnassignedUsersCountAsync();

                var totalCounts = new GlobalSearchCountsDto
                {
                    TotalAdmins = systemTotalAdmins,
                    TotalManagers = systemTotalManagers,
                    TotalUnassignedUsers = systemTotalUnassigned
                };

                response.TotalCounts = totalCounts;
            }

            return response;
        }
    }
}
