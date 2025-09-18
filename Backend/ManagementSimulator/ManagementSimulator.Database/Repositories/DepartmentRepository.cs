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
using ManagementSimulator.Database.Dtos.Department;
using ManagementSimulator.Database.Enums;

namespace ManagementSimulator.Database.Repositories
{
    public class DepartmentRepository : BaseRepository<Department>, IDeparmentRepository
    {
        private readonly MGMTSimulatorDbContext _dbContext;
        public DepartmentRepository(MGMTSimulatorDbContext databaseContext, IAuditService auditService) : base(databaseContext, auditService)
        {
            _dbContext = databaseContext;
        }

        public async Task<Department?> GetDepartmentByNameAsync(string name, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<Department> query = _dbContext.Departments;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(d => d.DeletedAt == null);

            return await query.FirstOrDefaultAsync(d => d.Name == name);
        }

        public async Task<Department?> GetDepartmentByIdAsync(int id, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<Department> query = _dbContext.Departments;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(d => d.DeletedAt == null);

            return await query.FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<(List<DepartmentDto> Data, int TotalCount)> GetAllInactiveDepartmentsFilteredAsync(string? name, QueryParams parameters, bool tracking = false)
        {
            IQueryable<Department> query = _dbContext.Departments;

            if (!tracking)
                query = query.AsNoTracking();

            query = query.Where(d => d.DeletedAt != null).Include(d => d.Users);

            // Filtering
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(d => d.Name.Contains(name));
            }

            var totalCount = await query.CountAsync();

            if (parameters == null)
                return (await query.Select(d => new DepartmentDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    EmployeeCount = d.Users.Count,
                    DeletedAt = d.DeletedAt
                }).ToListAsync(), totalCount);

            // Sorting
            if (string.IsNullOrEmpty(parameters.SortBy))
                query = query.OrderBy(d => d.Id);
            else
                query = query.ApplySorting<Department>(parameters.SortBy, parameters.SortDescending ?? false);

            // Pagination
            if (parameters.Page == null || parameters.Page <= 0 || parameters.PageSize == null || parameters.PageSize <= 0)
            {
                return (await query.Select(d => new DepartmentDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    EmployeeCount = d.Users.Count,
                    DeletedAt = d.DeletedAt
                }).ToListAsync(), totalCount);
            }
            else
            {
                var pagedData = await query
                    .Skip((int)parameters.PageSize * ((int)parameters.Page - 1))
                    .Take((int)parameters.PageSize)
                    .Select(d => new DepartmentDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Description = d.Description,
                        EmployeeCount = d.Users.Count,
                        DeletedAt = d.DeletedAt
                    }).ToListAsync();

                return (pagedData, totalCount);
            }
        }

        public async Task<(List<DepartmentDto> Data, int TotalCount)> GetAllDepartmentsFilteredAsync(string? name, QueryParams parameters, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<Department> query = _dbContext.Departments;
            if (!includeDeleted)
                query = query.Where(d => d.DeletedAt == null);

            if (!tracking)
                query = query.AsNoTracking();

            query = query.Include(d => d.Users);

            // Filtering
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(d => d.Name.Contains(name));
            }

            var totalCount = await query.CountAsync();

            if (parameters == null)
                return (await query.Select(d => new DepartmentDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    EmployeeCount = d.Users.Count,
                    DeletedAt = d.DeletedAt
                }).ToListAsync(), totalCount);

            // Sorting
            if (string.IsNullOrEmpty(parameters.SortBy))
                query = query.OrderBy(d => d.Id);
            else
                query = query.ApplySorting<Department>(parameters.SortBy, parameters.SortDescending ?? false);

            // Pagination
            if (parameters.Page == null || parameters.Page <= 0 || parameters.PageSize == null || parameters.PageSize <= 0)
            {
                return (await query.Select(d => new DepartmentDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    EmployeeCount = d.Users.Count,
                    DeletedAt = d.DeletedAt
                }).ToListAsync(), totalCount);
            }
            else
            {
                var pagedData = await query
                    .Skip((int)parameters.PageSize * ((int)parameters.Page - 1))
                    .Take((int)parameters.PageSize)
                    .Select(d => new DepartmentDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Description = d.Description,
                        EmployeeCount = d.Users.Count,
                        DeletedAt = d.DeletedAt
                    }).ToListAsync();

                return (pagedData, totalCount);
            }
        }

        public async Task<List<Department>> GetAllDepartmentsAsync(List<int> ids, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<Department> query = _dbContext.Departments;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(d => d.DeletedAt == null);

            return await query.Where(d => ids.Contains(d.Id)).ToListAsync();
        }
    }
}
