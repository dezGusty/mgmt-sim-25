using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Services.Interfaces
{
    public interface IEmployeeRoleService
    {
        Task<List<EmployeeRoleResponseDto>> GetAllEmployeeRolesAsync();
    }
}
