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

        public async Task<List<LeaveRequestType>> GetAllLeaveRequestTypesFilteredAsync(string? description, QueryParams parameters)
        {
            IQueryable<LeaveRequestType> query = GetRecords();

            // Filtering
            if(!description.IsNullOrEmpty())
            {
                query = query.Where(lrt => lrt.Description.Contains(description));
            }

            if (parameters == null)
                return await query.ToListAsync();

            // sorting
            if (!parameters.SortBy.IsNullOrEmpty())
            {
                query = query.ApplySorting<LeaveRequestType>(parameters.SortBy, parameters.SortDescending ?? false);
            }
            else
            {
                query = query.OrderBy(lrt => lrt.Id);
            }

            // filtering
            if (parameters.Page == null || parameters.Page <= 0 || parameters.PageSize == null || parameters.PageSize <= 0)
            {
                return await query.ToListAsync();
            }
            else
            {
                return await query.Skip(((int)parameters.Page - 1) * (int)parameters.PageSize)
                                   .Take((int)parameters.PageSize)
                                   .ToListAsync();
            }
        }

        public async Task<LeaveRequestType?> GetLeaveRequestTypesByDescriptionAsync(string description)
        {
            return await _dbContext.LeaveRequestTypes.FirstOrDefaultAsync(lrt => lrt.Description == description);
        }
    }
}
