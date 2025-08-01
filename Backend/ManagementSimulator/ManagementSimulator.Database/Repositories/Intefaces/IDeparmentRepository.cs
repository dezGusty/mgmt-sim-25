using ManagementSimulator.Database.Dtos.Department;
using ManagementSimulator.Database.Dtos.QueryParams;
using ManagementSimulator.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Database.Repositories.Intefaces
{
    public interface IDeparmentRepository : IBaseRepostory<Department>
    {
        Task<List<Department>> GetAllDepartmentsAsync(List<int> ids,bool includeDeleted = false);
        Task<Department?> GetDepartmentByIdAsync(int id, bool includeDeleted = false);
        Task<Department?> GetDepartmentByNameAsync(string name, bool includeDeleted = false);
        Task<(List<DepartmentDto> Data, int TotalCount)> GetAllDepartmentsFilteredAsync(string departmentName, QueryParams parameters, bool includeDeleted = false);
    }
}
