using ManagementSimulator.Core.Dtos.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Services.Interfaces
{
    public interface IEmployeeManagerService
    {
        Task<List<UserResponseDto>> GetManagersByEmployeeIdAsync(int employeeId);
        Task<List<UserResponseDto>> GetEmployeesByManagerIdAsync(int managerId);
        Task AddEmployeeManagerAsync(int employeeId, int managerId);
        Task DeleteEmployeeManagerAsync(int employeeId, int managerId);
        Task<EmployeeManagerResponseDto> UpdateEmployeeForManagerAsync(int employeeId, int managerId, int newEmployeeId);
        Task<EmployeeManagerResponseDto> UpdateManagerForEmployeeAsync(int employeeId, int managerId, int newManagerId);
    }
}
