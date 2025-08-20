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
                ManagerId = request.SecondaryManagerId,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };

            await _secondaryManagerRepository.AddSecondaryManagerAsync(secondaryManager);

            return new SecondaryManagerResponseDto
            {
                EmployeeId = secondaryManager.EmployeeId,
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                SecondaryManagerId = secondaryManager.ManagerId,
                SecondaryManagerName = $"{manager.FirstName} {manager.LastName}",
                StartDate = secondaryManager.StartDate,
                EndDate = secondaryManager.EndDate,
                IsActive = secondaryManager.IsActive
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

            await _secondaryManagerRepository.UpdateSecondaryManagerAsync(secondaryManager);

            return new SecondaryManagerResponseDto
            {
                EmployeeId = secondaryManager.EmployeeId,
                EmployeeName = $"{secondaryManager.Employee.FirstName} {secondaryManager.Employee.LastName}",
                SecondaryManagerId = secondaryManager.ManagerId,
                SecondaryManagerName = $"{secondaryManager.Manager.FirstName} {secondaryManager.Manager.LastName}",
                StartDate = secondaryManager.StartDate,
                EndDate = secondaryManager.EndDate,
                IsActive = secondaryManager.IsActive
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
                SecondaryManagerId = sm.ManagerId,
                SecondaryManagerName = $"{sm.Manager.FirstName} {sm.Manager.LastName}",
                StartDate = sm.StartDate,
                EndDate = sm.EndDate,
                IsActive = sm.IsActive
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
                SecondaryManagerId = sm.ManagerId,
                SecondaryManagerName = $"{sm.Manager.FirstName} {sm.Manager.LastName}",
                StartDate = sm.StartDate,
                EndDate = sm.EndDate,
                IsActive = sm.IsActive
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
                SecondaryManagerId = sm.ManagerId,
                SecondaryManagerName = $"{sm.Manager.FirstName} {sm.Manager.LastName}",
                StartDate = sm.StartDate,
                EndDate = sm.EndDate,
                IsActive = sm.IsActive
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
                SecondaryManagerId = secondaryManager.ManagerId,
                SecondaryManagerName = $"{secondaryManager.Manager.FirstName} {secondaryManager.Manager.LastName}",
                StartDate = secondaryManager.StartDate,
                EndDate = secondaryManager.EndDate,
                IsActive = secondaryManager.IsActive
            };
        }

        public async Task<List<SecondaryManagerResponseDto>> GetExpiredSecondaryManagersAsync()
        {
            var expiredManagers = await _secondaryManagerRepository.GetExpiredSecondaryManagersAsync(tracking: false);

            return expiredManagers.Select(sm => new SecondaryManagerResponseDto
            {
                EmployeeId = sm.EmployeeId,
                EmployeeName = $"{sm.Employee.FirstName} {sm.Employee.LastName}",
                SecondaryManagerId = sm.ManagerId,
                SecondaryManagerName = $"{sm.Manager.FirstName} {sm.Manager.LastName}",
                StartDate = sm.StartDate,
                EndDate = sm.EndDate,
                IsActive = sm.IsActive
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