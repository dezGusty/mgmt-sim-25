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
using ManagementSimulator.Database.Dtos.QueryParams;
using ManagementSimulator.Database.Extensions;

namespace ManagementSimulator.Database.Repositories
{
    public class DepartmentRepository: BaseRepository<Department>, IDeparmentRepository
    {
        private readonly MGMTSimulatorDbContext _dbContext;
        public DepartmentRepository(MGMTSimulatorDbContext databaseContext) : base(databaseContext)
        {
            _dbContext = databaseContext;
        }

        public async Task<Department?> GetDepartmentByNameAsync(string name, bool includeDeleted = false)
        {
            IQueryable<Department> query = _dbContext.Departments;
            if (!includeDeleted)
                query = query.Where(d => d.DeletedAt == null);
            return await query.FirstOrDefaultAsync(d => d.Name == name);
        }

        public async Task<Department?> GetDepartmentByIdAsync(int id, bool includeDeleted = false)
        {
            IQueryable<Department> query = _dbContext.Departments;
            if (!includeDeleted)
                query = query.Where(d => d.DeletedAt == null);
            return await query.FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<(List<Department> Data, int TotalCount)> GetAllDepartmentsFilteredAsync(string? name, QueryParams parameters, bool includeDeleted = false)
        {
            IQueryable<Department> query = GetRecords(includeDeletedEntities: includeDeleted);

            // Filtering
            if (!string.IsNullOrEmpty(name))
            {
                var lowerName = name.ToLower();
                query = query.Where(d => d.Name.ToLower().Contains(lowerName));
            }

            var totalCount = await query.CountAsync();

            if (parameters == null)
                return (await query.ToListAsync(), totalCount);

            // Sorting
            if (string.IsNullOrEmpty(parameters.SortBy))
                query = query.OrderBy(d => d.Id);
            else
                query = query.ApplySorting<Department>(parameters.SortBy, parameters.SortDescending ?? false);

            // Pagination
            if (parameters.Page == null || parameters.Page <= 0 || parameters.PageSize == null || parameters.PageSize <= 0)
            {
                var allData = await query.ToListAsync();
                return (allData, totalCount);
            }
            else
            {
                var pagedData = await query
                    .Skip((int)parameters.PageSize * ((int)parameters.Page - 1))
                    .Take((int)parameters.PageSize)
                    .ToListAsync();

                return (pagedData, totalCount);
            }
        }

        public async Task<List<Department>> GetAllDepartmentsAsync(List<int> ids, bool includeDeleted = false)
        {
            IQueryable<Department> query = _dbContext.Departments;
            if (!includeDeleted)
                query = query.Where(d => d.DeletedAt == null);

            return await query.Where(d => ids.Contains(d.Id)).ToListAsync();
        }
    }
}
