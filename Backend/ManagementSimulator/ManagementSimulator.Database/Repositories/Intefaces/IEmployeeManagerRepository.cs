using ManagementSimulator.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Database.Repositories.Intefaces
{
    public interface IEmployeeManagerRepository
    {
        Task<List<EmployeeManager>> GetAllEmployeeManagersAsync(bool includeDeleted = false);
        Task<List<User>> GetManagersForEmployeesByIdAsync(int subordinateId);
        Task<List<User>> GetEmployeesForManagerByIdAsync(int managerId);
        Task<List<EmployeeManager>> GetEMRelationshipForEmployeesByIdAsync(int subordinateId, bool includeDeleted = false);
        Task AddEmployeeManagersAsync(int subOrdinateId,int managersId);
        Task DeleteEmployeeManagerAsync(int employeeId, int managerId);
        Task<EmployeeManager?> GetEmployeeManagersByIdAsync(int employeeId, int managerId, bool includeDeleted = false);
        Task SaveChangesAsync();
    }
}
