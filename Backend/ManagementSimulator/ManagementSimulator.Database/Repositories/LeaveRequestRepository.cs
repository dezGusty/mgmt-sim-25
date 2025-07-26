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
    internal class LeaveRequestRepository : BaseRepository<LeaveRequest>, ILeaveRequestRepository
    {
        private readonly MGMTSimulatorDbContext _dbcontext;
        public LeaveRequestRepository(MGMTSimulatorDbContext dbcontext) : base(dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<List<LeaveRequest>> GetAllLeaveRequestsWithRelationshipsFilteredAsync(List<int> employeeIds, string? lastName, string? email, QueryParams parameters)
        {
            IQueryable<LeaveRequest> query = _dbcontext.LeaveRequests
                                                        .Include(lr => lr.User)
                                                        .Where(lr => employeeIds.Contains(lr.UserId));

            // filtering
            if(!lastName.IsNullOrEmpty())
            {
                query = query.Where(lr => lr.User.LastName.Contains(lastName));
            }

            if(!email.IsNullOrEmpty())
            {
                query = query.Where(lr => lr.User.Email.Contains(email));
            }

            if (parameters == null)
                return await query.ToListAsync();
            // sorting
            if (!parameters.SortBy.IsNullOrEmpty())
                query = query.ApplySorting<LeaveRequest>(parameters.SortBy, parameters.SortDescending ?? false);
            else
                query = query.OrderBy(lr => lr.Id);

            // paging 
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
    }
}
