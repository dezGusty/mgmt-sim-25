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
        Task<EmployeeRoleUser?> GetEmployeeRoleUserAsync(int userId, int roleId, bool includeDeleted = false, bool tracking = false);
        Task DeleteEmployeeUserRoleAsync(EmployeeRoleUser employeeRoleUser);
        Task<List<EmployeeRoleUser>> GetEmployeeRoleUsersByUserIdAsync(int userId, bool includeDeleted = false, bool tracking = false);
        Task<List<EmployeeRoleUser>> GetEmployeeRoleUsersByUserIdsAsync(List<int> userIds, bool includeDeleted = false, bool tracking = false);
        Task<int> GetEmployeeRoleUserByNameAsync(string name, bool includeDeleted = false, bool tracking = false);
        Task<List<EmployeeRole>> GetAllUserRolesAsync(bool includeDeleted = false, bool tracking = false);
    }
}
