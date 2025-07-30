using ManagementSimulator.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Database.Repositories.Intefaces
{
    public interface IEmployeeRoleRepository : IBaseRepostory<Entities.EmployeeRole>
    {
        Task AddEmployeeRoleUserAsync(EmployeeRoleUser employeeRoleUser); 
        Task<EmployeeRoleUser?> GetEmployeeRoleUserAsync(int userId, int roleId, bool includeDeleted = false);
        Task DeleteEmployeeUserRoleAsync(EmployeeRoleUser employeeRoleUser);
        Task<List<EmployeeRoleUser>> GetEmployeeRoleUsersByUserIdAsync(int userId, bool includeDeleted = false);
        Task<List<EmployeeRoleUser>?> GetEmployeeRoleUsersByUserIdsAsync(List<int> userIds, bool includeDeleted = false);
        Task<int> GetEmployeeRoleUserByNameAsync(string name, bool includeDeleted = false);
    }
}
