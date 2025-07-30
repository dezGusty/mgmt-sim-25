using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories;
using ManagementSimulator.Database.Repositories.Intefaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Services
{
    public class EmployeeRoleService : IEmployeeRoleService
    {
        private readonly IEmployeeRoleRepository _employeeRoleRepository;

        public EmployeeRoleService(IEmployeeRoleRepository employeeRoleRepository)
        {
            _employeeRoleRepository = employeeRoleRepository;
        }

        public async Task<List<EmployeeRoleResponseDto>> GetAllEmployeeRolesAsync()
        {
            var result = await _employeeRoleRepository.GetAllUserRolesAsync();
            return result.Select(er => new EmployeeRoleResponseDto
            {
                Id = er.Id,
                Rolename = er.Rolename
            }).ToList();
        }
    }
}
