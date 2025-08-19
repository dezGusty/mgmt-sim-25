using ManagementSimulator.Core.Dtos.Requests.SecondaryManagers;
using ManagementSimulator.Core.Dtos.Responses.SecondaryManager;
using ManagementSimulator.Core.Dtos.Responses.User;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories.Intefaces;
using ManagementSimulator.Infrastructure.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Services
{
    public class SecondaryManagerService : ISecondaryManagerService
    {
        private readonly ISecondaryManagerRepository _secondaryManagerRepository;
        private readonly IUserRepository _userRepository;

        public SecondaryManagerService(ISecondaryManagerRepository secondaryManagerRepository, IUserRepository userRepository)
        {
            _secondaryManagerRepository = secondaryManagerRepository;
            _userRepository = userRepository;
        }

        public async Task<SecondaryManagerResponseDto> AssignSecondaryManagerAsync(CreateSecondaryManagerRequest request, int assignedByAdminId)
        {
            // Validate users exist
            var employee = await _userRepository.GetFirstOrDefaultAsync(request.EmployeeId);
            if (employee == null)
            {
                throw new EntryNotFoundException(nameof(User), request.EmployeeId);
            }

            var manager = await _userRepository.GetFirstOrDefaultAsync(request.SecondaryManagerId);
            if (manager == null)
            {
                throw new EntryNotFoundException(nameof(User), request.SecondaryManagerId);
            }

            var admin = await _userRepository.GetFirstOrDefaultAsync(assignedByAdminId);
            if (admin == null)
            {
                throw new EntryNotFoundException(nameof(User), assignedByAdminId);
            }

            // Validate date range
            if (request.StartDate >= request.EndDate)
            {
                throw new InvalidDateRangeException("Start date must be before end date");
            }

            if (request.StartDate < DateTime.UtcNow.Date)
            {
                throw new InvalidDateRangeException("Start date cannot be in the past");
            }

            // Check for overlapping assignments
            var hasOverlapping = await _secondaryManagerRepository.HasOverlappingSecondaryManagerAsync(
                request.EmployeeId, request.SecondaryManagerId, request.StartDate, request.EndDate);

            if (hasOverlapping)
            {
                throw new UniqueConstraintViolationException(nameof(SecondaryManager), 
                    "There is already an overlapping secondary manager assignment for this period");
            }

            var secondaryManager = new SecondaryManager
            {
                EmployeeId = request.EmployeeId,
                SecondaryManagerId = request.SecondaryManagerId,
                AssignedByAdminId = assignedByAdminId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Reason = request.Reason,
                CreatedAt = DateTime.UtcNow
            };

            await _secondaryManagerRepository.AddSecondaryManagerAsync(secondaryManager);

            return new SecondaryManagerResponseDto
            {
                EmployeeId = secondaryManager.EmployeeId,
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                SecondaryManagerId = secondaryManager.SecondaryManagerId,
                SecondaryManagerName = $"{manager.FirstName} {manager.LastName}",
                AssignedByAdminId = secondaryManager.AssignedByAdminId,
                AssignedByAdminName = $"{admin.FirstName} {admin.LastName}",
                StartDate = secondaryManager.StartDate,
                EndDate = secondaryManager.EndDate,
                Reason = secondaryManager.Reason,
                IsActive = secondaryManager.IsActive,
                CreatedAt = secondaryManager.CreatedAt,
                ModifiedAt = secondaryManager.ModifiedAt
            };
        }

        public async Task<SecondaryManagerResponseDto> UpdateSecondaryManagerAsync(int employeeId, int secondaryManagerId, DateTime startDate, UpdateSecondaryManagerRequest request)
        {
            var secondaryManager = await _secondaryManagerRepository.GetSecondaryManagerByIdAsync(
                employeeId, secondaryManagerId, startDate, includeDeleted: false, tracking: true);

            if (secondaryManager == null)
            {
                throw new EntryNotFoundException(nameof(SecondaryManager), $"{employeeId}-{secondaryManagerId}-{startDate}");
            }

            // Validate new end date
            if (request.NewEndDate <= secondaryManager.StartDate)
            {
                throw new InvalidDateRangeException("End date must be after start date");
            }

            if(request.NewEndDate < DateTime.UtcNow.Date)
            {
                throw new InvalidDateRangeException("End date cannot be in the past");
            }

            secondaryManager.EndDate = request.NewEndDate;
            secondaryManager.Reason = request.Reason ?? secondaryManager.Reason;
            secondaryManager.ModifiedAt = DateTime.UtcNow;

            await _secondaryManagerRepository.UpdateSecondaryManagerAsync(secondaryManager);

            return new SecondaryManagerResponseDto
            {
                EmployeeId = secondaryManager.EmployeeId,
                EmployeeName = $"{secondaryManager.Employee.FirstName} {secondaryManager.Employee.LastName}",
                SecondaryManagerId = secondaryManager.SecondaryManagerId,
                SecondaryManagerName = $"{secondaryManager.Manager.FirstName} {secondaryManager.Manager.LastName}",
                AssignedByAdminId = secondaryManager.AssignedByAdminId,
                AssignedByAdminName = $"{secondaryManager.AssignedByAdmin.FirstName} {secondaryManager.AssignedByAdmin.LastName}",
                StartDate = secondaryManager.StartDate,
                EndDate = secondaryManager.EndDate,
                Reason = secondaryManager.Reason,
                IsActive = secondaryManager.IsActive,
                CreatedAt = secondaryManager.CreatedAt,
                ModifiedAt = secondaryManager.ModifiedAt
            };
        }

        public async Task RemoveSecondaryManagerAsync(int employeeId, int secondaryManagerId, DateTime startDate)
        {
            var secondaryManager = await _secondaryManagerRepository.GetSecondaryManagerByIdAsync(
                employeeId, secondaryManagerId, startDate, includeDeleted: false, tracking: false);

            if (secondaryManager == null)
            {
                throw new EntryNotFoundException(nameof(SecondaryManager), $"{employeeId}-{secondaryManagerId}-{startDate}");
            }

            await _secondaryManagerRepository.DeleteSecondaryManagerAsync(employeeId, secondaryManagerId, startDate);
        }

        public async Task<List<SecondaryManagerResponseDto>> GetAllSecondaryManagersAsync()
        {
            var secondaryManagers = await _secondaryManagerRepository.GetAllSecondaryManagersAsync(includeDeleted: false, tracking: false);

            return secondaryManagers.Select(sm => new SecondaryManagerResponseDto
            {
                EmployeeId = sm.EmployeeId,
                EmployeeName = $"{sm.Employee.FirstName} {sm.Employee.LastName}",
                SecondaryManagerId = sm.SecondaryManagerId,
                SecondaryManagerName = $"{sm.Manager.FirstName} {sm.Manager.LastName}",
                AssignedByAdminId = sm.AssignedByAdminId,
                AssignedByAdminName = $"{sm.AssignedByAdmin.FirstName} {sm.AssignedByAdmin.LastName}",
                StartDate = sm.StartDate,
                EndDate = sm.EndDate,
                Reason = sm.Reason,
                IsActive = sm.IsActive,
                CreatedAt = sm.CreatedAt,
                ModifiedAt = sm.ModifiedAt
            }).ToList();
        }

        public async Task<List<SecondaryManagerResponseDto>> GetSecondaryManagersForEmployeeAsync(int employeeId)
        {
            var secondaryManagers = await _secondaryManagerRepository.GetSecondaryManagersForEmployeeAsync(
                employeeId, includeDeleted: false, tracking: false);

            if (!secondaryManagers.Any())
            {
                throw new EntryNotFoundException(nameof(SecondaryManager), employeeId);
            }

            return secondaryManagers.Select(sm => new SecondaryManagerResponseDto
            {
                EmployeeId = sm.EmployeeId,
                EmployeeName = $"{sm.Employee?.FirstName} {sm.Employee?.LastName}",
                SecondaryManagerId = sm.SecondaryManagerId,
                SecondaryManagerName = $"{sm.Manager.FirstName} {sm.Manager.LastName}",
                AssignedByAdminId = sm.AssignedByAdminId,
                AssignedByAdminName = $"{sm.AssignedByAdmin.FirstName} {sm.AssignedByAdmin.LastName}",
                StartDate = sm.StartDate,
                EndDate = sm.EndDate,
                Reason = sm.Reason,
                IsActive = sm.IsActive,
                CreatedAt = sm.CreatedAt,
                ModifiedAt = sm.ModifiedAt
            }).ToList();
        }

        public async Task<List<SecondaryManagerResponseDto>> GetActiveSecondaryManagersForEmployeeAsync(int employeeId)
        {
            var secondaryManagers = await _secondaryManagerRepository.GetActiveSecondaryManagersForEmployeeAsync(
                employeeId, tracking: false);

            return secondaryManagers.Select(sm => new SecondaryManagerResponseDto
            {
                EmployeeId = sm.EmployeeId,
                EmployeeName = $"{sm.Employee?.FirstName} {sm.Employee?.LastName}",
                SecondaryManagerId = sm.SecondaryManagerId,
                SecondaryManagerName = $"{sm.Manager.FirstName} {sm.Manager.LastName}",
                AssignedByAdminId = sm.AssignedByAdminId,
                AssignedByAdminName = $"{sm.AssignedByAdmin.FirstName} {sm.AssignedByAdmin.LastName}",
                StartDate = sm.StartDate,
                EndDate = sm.EndDate,
                Reason = sm.Reason,
                IsActive = sm.IsActive,
                CreatedAt = sm.CreatedAt,
                ModifiedAt = sm.ModifiedAt
            }).ToList();
        }

        public async Task<List<UserResponseDto>> GetEmployeesWithActiveSecondaryManagerAsync(int secondaryManagerId)
        {
            var employees = await _secondaryManagerRepository.GetEmployeesWithActiveSecondaryManagerAsync(
                secondaryManagerId, tracking: false);

            if (!employees.Any())
            {
                throw new EntryNotFoundException(nameof(User), secondaryManagerId);
            }

            return employees.Select(e => new UserResponseDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                Roles = e.Roles
                    .Where(eru => eru.DeletedAt == null && eru.Role != null)
                    .Select(ru => ru.Role.Rolename)
                    .ToList(),
                JobTitleName = e.Title?.Name ?? string.Empty,
                JobTitleId = e.JobTitleId
            }).ToList();
        }

        public async Task<List<SecondaryManagerResponseDto>> GetSecondaryManagersAssignedByAdminAsync(int adminId)
        {
            var secondaryManagers = await _secondaryManagerRepository.GetSecondaryManagersAssignedByAdminAsync(
                adminId, includeDeleted: false, tracking: false);

            return secondaryManagers.Select(sm => new SecondaryManagerResponseDto
            {
                EmployeeId = sm.EmployeeId,
                EmployeeName = $"{sm.Employee.FirstName} {sm.Employee.LastName}",
                SecondaryManagerId = sm.SecondaryManagerId,
                SecondaryManagerName = $"{sm.Manager.FirstName} {sm.Manager.LastName}",
                AssignedByAdminId = sm.AssignedByAdminId,
                AssignedByAdminName = $"{sm.AssignedByAdmin?.FirstName} {sm.AssignedByAdmin?.LastName}",
                StartDate = sm.StartDate,
                EndDate = sm.EndDate,
                Reason = sm.Reason,
                IsActive = sm.IsActive,
                CreatedAt = sm.CreatedAt,
                ModifiedAt = sm.ModifiedAt
            }).ToList();
        }

        public async Task<SecondaryManagerResponseDto?> GetSecondaryManagerByIdAsync(int employeeId, int secondaryManagerId, DateTime startDate)
        {
            var secondaryManager = await _secondaryManagerRepository.GetSecondaryManagerByIdAsync(
                employeeId, secondaryManagerId, startDate, includeDeleted: false, tracking: false);

            if (secondaryManager == null)
            {
                return null;
            }

            return new SecondaryManagerResponseDto
            {
                EmployeeId = secondaryManager.EmployeeId,
                EmployeeName = $"{secondaryManager.Employee.FirstName} {secondaryManager.Employee.LastName}",
                SecondaryManagerId = secondaryManager.SecondaryManagerId,
                SecondaryManagerName = $"{secondaryManager.Manager.FirstName} {secondaryManager.Manager.LastName}",
                AssignedByAdminId = secondaryManager.AssignedByAdminId,
                AssignedByAdminName = $"{secondaryManager.AssignedByAdmin.FirstName} {secondaryManager.AssignedByAdmin.LastName}",
                StartDate = secondaryManager.StartDate,
                EndDate = secondaryManager.EndDate,
                Reason = secondaryManager.Reason,
                IsActive = secondaryManager.IsActive,
                CreatedAt = secondaryManager.CreatedAt,
                ModifiedAt = secondaryManager.ModifiedAt
            };
        }

        public async Task<List<SecondaryManagerResponseDto>> GetExpiredSecondaryManagersAsync()
        {
            var expiredManagers = await _secondaryManagerRepository.GetExpiredSecondaryManagersAsync(tracking: false);

            return expiredManagers.Select(sm => new SecondaryManagerResponseDto
            {
                EmployeeId = sm.EmployeeId,
                EmployeeName = $"{sm.Employee.FirstName} {sm.Employee.LastName}",
                SecondaryManagerId = sm.SecondaryManagerId,
                SecondaryManagerName = $"{sm.Manager.FirstName} {sm.Manager.LastName}",
                AssignedByAdminId = sm.AssignedByAdminId,
                AssignedByAdminName = $"{sm.AssignedByAdmin?.FirstName} {sm.AssignedByAdmin?.LastName}",
                StartDate = sm.StartDate,
                EndDate = sm.EndDate,
                Reason = sm.Reason,
                IsActive = sm.IsActive,
                CreatedAt = sm.CreatedAt,
                ModifiedAt = sm.ModifiedAt
            }).ToList();
        }

        public async Task<bool> HasActiveSecondaryManagerAsync(int employeeId, int secondaryManagerId)
        {
            return await _secondaryManagerRepository.HasActiveSecondaryManagerAsync(employeeId, secondaryManagerId);
        }

        public async Task<bool> ValidateSecondaryManagerAssignmentAsync(int employeeId, int secondaryManagerId, DateTime startDate, DateTime endDate)
        {
            // Check if users exist
            var employee = await _userRepository.GetFirstOrDefaultAsync(employeeId);
            var manager = await _userRepository.GetFirstOrDefaultAsync(secondaryManagerId);

            if (employee == null || manager == null)
            {
                return false;
            }

            // Check date range validity
            if (startDate >= endDate || startDate < DateTime.UtcNow.Date)
            {
                return false;
            }

            // Check for overlapping assignments
            var hasOverlapping = await _secondaryManagerRepository.HasOverlappingSecondaryManagerAsync(
                employeeId, secondaryManagerId, startDate, endDate);

            return !hasOverlapping;
        }
    }
}