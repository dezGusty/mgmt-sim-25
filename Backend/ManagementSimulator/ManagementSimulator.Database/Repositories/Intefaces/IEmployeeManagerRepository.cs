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
        public Task<List<EmployeeManager>> GetAllEmployeeManagersAsync();
        public Task<List<User>> GetManagersForEmployeesByIdAsync(int subordinateId);
        public Task<List<User>> GetEmployeesForManagerByIdAsync(int managerId);
        public Task AddEmployeeManagersAsync(int subOrdinateId,int managersId);
        public Task DeleteEmployeeManagerAsync(int employeeId, int managerId);
        public Task<EmployeeManager?> GetEmployeeManagersByIdAsync(int employeeId, int managerId);
        public Task<EmployeeManager?> GetEmployeeManagersByIdIncludeDeletedAsync(int employeeId, int managerId);
        public Task SaveChangesAsync(); 
    }
}
