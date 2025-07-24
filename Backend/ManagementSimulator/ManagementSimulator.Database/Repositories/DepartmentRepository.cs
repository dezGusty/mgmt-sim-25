using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories.Intefaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ManagementSimulator.Infrastructure.Exceptions;

namespace ManagementSimulator.Database.Repositories
{
    public class DepartmentRepository: BaseRepository<Department>, IDeparmentRepository
    {
        private readonly MGMTSimulatorDbContext _dbContext;
        public DepartmentRepository(MGMTSimulatorDbContext databaseContext) : base(databaseContext)
        {
            _dbContext = databaseContext;
        }

        public async Task<Department?> GetDepartmentByNameAsync(string name)
        {
            return await _dbContext.Departments.FirstOrDefaultAsync(d => d.Name == name);
        }

        public async Task<Department?> GetDepartmentByIdAsync(int id)
        {
            return await _dbContext.Departments
                .Where(d => d.DeletedAt == null)
                .FirstOrDefaultAsync(d => d.Id == id);
        }
    }
}
