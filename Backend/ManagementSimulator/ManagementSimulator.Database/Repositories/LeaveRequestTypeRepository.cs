using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagementSimulator.Database.Repositories.Intefaces;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Context;
using Microsoft.EntityFrameworkCore;
using ManagementSimulator.Database.Dtos.QueryParams;
using Microsoft.IdentityModel.Tokens;
using ManagementSimulator.Database.Extensions;


namespace ManagementSimulator.Database.Repositories
{
    internal class LeaveRequestTypeRepository : BaseRepository<LeaveRequestType>, ILeaveRequestTypeRepository
    {
        private readonly MGMTSimulatorDbContext _dbContext;
        public LeaveRequestTypeRepository(MGMTSimulatorDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(List<LeaveRequestType> Data, int TotalCount)> GetAllLeaveRequestTypesFilteredAsync(string? title, QueryParams parameters, bool includeDeleted = false)
        {
            IQueryable<LeaveRequestType> query = GetRecords(includeDeletedEntities: includeDeleted);

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(lrt =>
                    lrt.Title.Contains(title) ||
                    lrt.Description.Contains(title)
                );
            }

            var totalCount = await query.CountAsync();

            if (parameters == null)
                return (await query.ToListAsync(), totalCount);

            // sorting
            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                query = query.ApplySorting<LeaveRequestType>(parameters.SortBy, parameters.SortDescending ?? false);
            }
            else
            {
                query = query.OrderBy(lrt => lrt.Id);
            }

            // pagination
            if (parameters.Page == null || parameters.Page <= 0 || parameters.PageSize == null || parameters.PageSize <= 0)
            {
                return (await query.ToListAsync(), totalCount);
            }
            else
            {
                var pagedData = await query.Skip(((int)parameters.Page - 1) * (int)parameters.PageSize)
                               .Take((int)parameters.PageSize)
                               .ToListAsync();
                return (pagedData, totalCount);
            }
        }

        public async Task<LeaveRequestType?> GetLeaveRequestTypesByTitleAsync(string title, bool includeDeleted = false)
        {
            IQueryable<LeaveRequestType?> query = _dbContext.LeaveRequestTypes;
            if (!includeDeleted)
                query = query.Where(lrt => lrt.DeletedAt == null);

            return await query.FirstOrDefaultAsync(lrt => lrt.Title == title);
        }
    }
}
