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
        Task<Department?> GetDepartmentByIdAsync(int id);
        Task<Department?> GetDepartmentByNameAsync(string name);
        Task<List<Department>> GetAllDepartmentsFilteredAsync(string departmentName, QueryParams parameters);
    }
}
