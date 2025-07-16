using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories.Intefaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ManagementSimulator.Database.Repositories
{
    public class DepartmentRepository: BaseRepository<Department>, IDeparmentRepository
    {
        private readonly MGMTSimulatorDbContext _databaseContext;
        public DepartmentRepository(MGMTSimulatorDbContext databaseContext) :base(databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public Task<List<Department>> GetAllDepartmentsAsync()
        {
            return GetAllAsync(includeDeletedEntities: false);
        }

        public Task<Department?> GetDepartmentByIdAsync(int id)
        {
            return _databaseContext.Departments
                .FirstOrDefaultAsync(department => department.Id == id);
        }

        public async Task<Department?> AddDepartmentAsync(Department department)
        {
            Insert(department);
            await SaveChangesAsync();
            return department;
        }

        public async Task<Department?> UpdateDepartmentAsync(Department department)
        {
            Update(department);
            await SaveChangesAsync();
            return department;
        }

        public async Task<bool> DeleteDepartmentAsync(int id)
        {
            Department? department = await GetDepartmentByIdAsync(id);

            if (department is null)
            {
                return false;
            }
            SoftDelete(department);
            return true;
        }

    }
}
