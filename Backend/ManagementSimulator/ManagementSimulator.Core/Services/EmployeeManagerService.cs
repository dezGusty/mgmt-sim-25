using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories.Intefaces;
using ManagementSimulator.Infrastructure.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Services
{
    public class EmployeeManagerService : IEmployeeManagerService
    {
        private readonly IEmployeeManagerRepository _employeeManagerRepository;
        private readonly IUserRepository _userRepository;

        public EmployeeManagerService(IEmployeeManagerRepository employeeManagerRepository, IUserRepository userRepository)
        {
            _employeeManagerRepository = employeeManagerRepository;
            _userRepository = userRepository;
        }

        public async Task AddEmployeeManagerAsync(int employeeId, int managerId)
        {
            if(await _userRepository.GetFirstOrDefaultAsync(employeeId) == null)
            {
                throw new EntryNotFoundException(nameof(Database.Entities.User), employeeId);
            }

            if (await _userRepository.GetFirstOrDefaultAsync(managerId) == null)
            {
                throw new EntryNotFoundException(nameof(Database.Entities.User), managerId);
            }

            await _employeeManagerRepository.AddEmployeeManagersAsync(employeeId, managerId);
        }

        public async Task DeleteEmployeeManagerAsync(int employeeId, int managerId)
        {
            if (await _userRepository.GetFirstOrDefaultAsync(employeeId) == null)
            {
                throw new EntryNotFoundException(nameof(Database.Entities.User), employeeId);
            }

            if (await _userRepository.GetFirstOrDefaultAsync(managerId) == null)
            {
                throw new EntryNotFoundException(nameof(Database.Entities.User), managerId);
            }

            await _employeeManagerRepository.DeleteEmployeeManagerAsync(employeeId, managerId);
        }

        public async Task<List<UserResponseDto>> GetEmployeesByManagerIdAsync(int managerId)
        {
            var employees = await _employeeManagerRepository.GetEmployeesForManagerByIdAsync(managerId);

            if (employees == null || !employees.Any())
            {
                throw new EntryNotFoundException(nameof(Database.Entities.User), managerId);
            }

            return employees.Select(e => new UserResponseDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                Role = string.Join(" ", e.Roles.Select(r => r.Rolename)),
                JobTitleName = e.Title.Name ?? string.Empty,
            }).ToList();
        }

        public async Task<List<UserResponseDto>> GetManagersByEmployeeIdAsync(int employeeId)
        {
            var managers = await _employeeManagerRepository.GetManagersForEmployeesByIdAsync(employeeId);

            if (managers == null || !managers.Any())
            {
                throw new EntryNotFoundException(nameof(Database.Entities.User), employeeId);
            }

            return managers.Select(e => new UserResponseDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                Role = string.Join(" ", e.Roles.Select(r => r.Rolename)),
                JobTitleName = e.Title.Name ?? string.Empty,
            }).ToList();
        }

        public async Task<EmployeeManagerResponseDto> UpdateEmployeeForManagerAsync(int employeeId, int managerId, int newEmployeeId)
        {
            var userManager = await _employeeManagerRepository.GetEmployeeManagersByIdAsync(employeeId, managerId);

            if(userManager == null)
            {
                throw new EntryNotFoundException(nameof(Database.Entities.EmployeeManager), $"{employeeId}-{managerId}");
            }

            userManager.EmployeeId = newEmployeeId;
            userManager.ModifiedAt = DateTime.UtcNow;

            await _employeeManagerRepository.SaveChangesAsync();
            return new EmployeeManagerResponseDto
            {
                EmployeeId = userManager.EmployeeId,
                ManagerId = userManager.ManagerId,
            };
        }

        public async Task<EmployeeManagerResponseDto> UpdateManagerForEmployeeAsync(int employeeId, int managerId, int newManagerId)
        {
            var userManager = await _employeeManagerRepository.GetEmployeeManagersByIdAsync(employeeId, managerId);

            if (userManager == null)
            {
                throw new EntryNotFoundException(nameof(Database.Entities.EmployeeManager), $"{employeeId}-{managerId}");
            }

            userManager.ManagerId = newManagerId;
            userManager.ModifiedAt = DateTime.UtcNow;
            await _employeeManagerRepository.SaveChangesAsync();

            return new EmployeeManagerResponseDto
            {
                EmployeeId = userManager.EmployeeId,
                ManagerId = userManager.ManagerId,
            };
        }
    }
}
