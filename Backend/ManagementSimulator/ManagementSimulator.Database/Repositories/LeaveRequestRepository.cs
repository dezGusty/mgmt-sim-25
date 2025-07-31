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

        public async Task<(List<LeaveRequest>? Data, int TotalCount)> GetAllLeaveRequestsWithRelationshipsFilteredAsync(List<int> employeeIds, string? lastName, string? email, 
            QueryParams parameters, bool includeDeleted = false)
        {
            IQueryable<LeaveRequest> query = _dbcontext.LeaveRequests;

            if (!includeDeleted)
                query = query.Where(lr => lr.DeletedAt == null);
            query = query.Include(lr => lr.User)
                         .Where(lr => employeeIds.Contains(lr.UserId));

            // filtering
            if (!string.IsNullOrEmpty(lastName))
            {
                query = query.Where(lr => lr.User.LastName.Contains(lastName));
            }
            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(lr => lr.User.Email.Contains(email));
            }

            var totalCount = await query.CountAsync();

            if (parameters == null)
                return (await query.ToListAsync(), totalCount);

            // sorting
            if (!string.IsNullOrEmpty(parameters.SortBy))
                query = query.ApplySorting<LeaveRequest>(parameters.SortBy, parameters.SortDescending ?? false);
            else
                query = query.OrderBy(lr => lr.Id);

            // paging 
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

        public async Task<List<LeaveRequest>> GetOverlappingRequestsAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _dbcontext.LeaveRequests
                .Where(lr => lr.UserId == userId &&
                             ((lr.StartDate <= endDate && lr.EndDate >= startDate)))
                .ToListAsync();
        }

        public async Task<List<LeaveRequest>> GetLeaveRequestsByUserAndTypeAsync(int userId, int leaveRequestTypeId, int year, bool includeDeleted = false)
        {
            IQueryable<LeaveRequest> query = _dbcontext.LeaveRequests;

            if (!includeDeleted)
                query = query.Where(lr => lr.DeletedAt == null);

            var startOfYear = new DateTime(year, 1, 1);
            var endOfYear = new DateTime(year, 12, 31);

            return await query
                .Include(lr => lr.LeaveRequestType)
                .Where(lr => lr.UserId == userId && 
                            lr.LeaveRequestTypeId == leaveRequestTypeId &&
                            (lr.RequestStatus == Database.Enums.RequestStatus.Pending || 
                             lr.RequestStatus == Database.Enums.RequestStatus.Approved) &&
                            lr.StartDate <= endOfYear && lr.EndDate >= startOfYear)
                .ToListAsync();
        }
    }
}
