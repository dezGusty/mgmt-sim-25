﻿using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Dtos.Responses.User;
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

            if (await _employeeManagerRepository.GetEmployeeManagersByIdAsync(employeeId, managerId) != null)
            {
                throw new UniqueConstraintViolationException(nameof(EmployeeManager), "Entry already exists");
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
                Roles = e.Roles
                    .Where(eru => eru.DeletedAt == null && eru.Role != null)
                    .Select(ru => ru.Role.Rolename)
                    .ToList(),

                JobTitleName = e.Title.Name ?? string.Empty,
                JobTitleId = e.JobTitleId
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
                Roles = e.Roles
                    .Where(eru => eru.DeletedAt == null && eru.Role != null)
                    .Select(ru => ru.Role.Rolename)
                    .ToList(),

                JobTitleName = e.Title.Name ?? string.Empty,
                JobTitleId = e.JobTitleId
            }).ToList();
        }

        public async Task<EmployeeManagerResponseDto> UpdateManagerForEmployeeAsync(int employeeId, int managerId, int newManagerId)
        {
            var userManager = await _employeeManagerRepository.GetEmployeeManagersByIdIncludeDeletedAsync(employeeId, managerId);
            if (userManager == null)
            {
                throw new EntryNotFoundException(nameof(Database.Entities.EmployeeManager), $"{employeeId}-{managerId}");
            }
            var newEmployeeManager = await _employeeManagerRepository.GetEmployeeManagersByIdIncludeDeletedAsync(employeeId, newManagerId);
            if (newEmployeeManager != null)
            {
                if (newEmployeeManager.DeletedAt == null)
                    throw new UniqueConstraintViolationException(nameof(Database.Entities.EmployeeManager), "Entry already exists");
                else
                {
                    newEmployeeManager.DeletedAt = null;
                    newEmployeeManager.ModifiedAt = DateTime.UtcNow;
                    userManager.DeletedAt = DateTime.UtcNow;
                }
            }
            else
            {
                userManager.ManagerId = newManagerId;
                userManager.ModifiedAt = DateTime.UtcNow;
            }
            await _employeeManagerRepository.SaveChangesAsync();
            return new EmployeeManagerResponseDto
            {
                EmployeeId = employeeId,
                ManagerId = newEmployeeManager?.ManagerId ?? newManagerId,
            };
        }

        public async Task<EmployeeManagerResponseDto> UpdateEmployeeForManagerAsync(int employeeId, int managerId, int newEmployeeId)
        {
            var userManager = await _employeeManagerRepository.GetEmployeeManagersByIdIncludeDeletedAsync(employeeId, managerId);
            if (userManager == null)
            {
                throw new EntryNotFoundException(nameof(Database.Entities.EmployeeManager), $"{employeeId}-{managerId}");
            }
            var newEmployeeManager = await _employeeManagerRepository.GetEmployeeManagersByIdIncludeDeletedAsync(newEmployeeId, managerId);
            if (newEmployeeManager != null)
            {
                if (newEmployeeManager.DeletedAt == null)
                    throw new UniqueConstraintViolationException(nameof(Database.Entities.EmployeeManager), "Entry already exists");
                else
                {
                    newEmployeeManager.DeletedAt = null;
                    newEmployeeManager.ModifiedAt = DateTime.UtcNow;
                    userManager.DeletedAt = DateTime.UtcNow;
                }
            }
            else
            {
                userManager.EmployeeId = newEmployeeId;
                userManager.ModifiedAt = DateTime.UtcNow;
            }
            await _employeeManagerRepository.SaveChangesAsync();
            return new EmployeeManagerResponseDto
            {
                EmployeeId = newEmployeeManager?.EmployeeId ?? newEmployeeId,
                ManagerId = managerId,
            };
        }

        public async Task<List<EmployeeManagerResponseDto>> GetAllEmployeeManagersAsync()
        {
            var employeeManagers = await _employeeManagerRepository.GetAllEmployeeManagersAsync();
            return employeeManagers.Select(employeeManagers => new EmployeeManagerResponseDto
            {
                EmployeeId = employeeManagers.EmployeeId,
                ManagerId = employeeManagers.ManagerId,
            }).ToList();
        }
    }
}
