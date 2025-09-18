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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using ManagementSimulator.Database.Enums;

namespace ManagementSimulator.Database.Repositories
{
    public class LeaveRequestRepository : BaseRepository<LeaveRequest>, ILeaveRequestRepository
    {
        private readonly MGMTSimulatorDbContext _dbcontext;
        public LeaveRequestRepository(MGMTSimulatorDbContext dbcontext, IAuditService auditService) : base(dbcontext, auditService)
        {
            _dbcontext = dbcontext;
        }

        public async Task<(List<LeaveRequest> Data, int TotalCount)> GetAllLeaveRequestsWithRelationshipsFilteredAsync(List<int> employeeIds, string? lastName, string? email,
            QueryParams parameters, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<LeaveRequest> query = _dbcontext.LeaveRequests;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(lr => lr.DeletedAt == null);

            query = query.Include(lr => lr.User)
                         .ThenInclude(u => u.Department)
                         .Include(lr => lr.LeaveRequestType)
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

        public async Task<List<LeaveRequest>> GetOverlappingRequestsAsync(int userId, DateTime startDate, DateTime endDate, bool tracking = false)
        {
            IQueryable<LeaveRequest> query = _dbcontext.LeaveRequests;

            if (!tracking)
                query = query.AsNoTracking();

            return await query
                .Where(lr => lr.UserId == userId &&
                             ((lr.StartDate <= endDate && lr.EndDate >= startDate)))
                .ToListAsync();
        }

        public async Task<List<LeaveRequest>> GetLeaveRequestsByUserAndTypeAsync(int userId, int leaveRequestTypeId, int year, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<LeaveRequest> query = _dbcontext.LeaveRequests;

            if (!tracking)
                query = query.AsNoTracking();

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

        public async Task<List<LeaveRequest>> GetAllWithRelationshipsAsync(bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<LeaveRequest> query = _dbcontext.LeaveRequests;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(lr => lr.DeletedAt == null);

            return await query
                .Include(lr => lr.User)
                .ThenInclude(u => u.Department)
                .Include(lr => lr.LeaveRequestType)
                .ToListAsync();
        }

        public async Task<List<LeaveRequest>> GetAllWithRelationshipsByUserIdsAsync(List<int> userIds, string? name = null, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<LeaveRequest> query = _dbcontext.LeaveRequests;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(lr => lr.DeletedAt == null);

            query = query
                .Include(lr => lr.User)
                .ThenInclude(u => u.Department)
                .Include(lr => lr.LeaveRequestType)
                .Where(lr => userIds.Contains(lr.UserId));

            if (!string.IsNullOrWhiteSpace(name))
            {
                var lowered = name.ToLower();
                query = query.Where(lr =>
                    (lr.User.FirstName + " " + lr.User.LastName).ToLower().Contains(lowered)
                    || lr.User.FirstName.ToLower().Contains(lowered)
                    || lr.User.LastName.ToLower().Contains(lowered)
                );
            }

            return await query.ToListAsync();
        }

        public async Task<LeaveRequest> GetLeaveRequestWithDetailsAsync(int id, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<LeaveRequest> query = _dbcontext.LeaveRequests;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(lr => lr.DeletedAt == null);

            var leaveRequest = await query
                .Include(lr => lr.User)
                .ThenInclude(u => u.Department)
                .Include(lr => lr.LeaveRequestType)
                .Include(lr => lr.Reviewer)
                .FirstOrDefaultAsync(lr => lr.Id == id);

            if (leaveRequest == null)
                throw new InvalidOperationException($"Leave request with ID {id} not found");

            return leaveRequest;
        }

        public async Task<(List<LeaveRequest> Items, int TotalCount)> GetFilteredLeaveRequestsAsync(
    string status, int pageSize, int pageNumber = 1, List<int>? employeeIds = null)
        {
            if (pageNumber < 1) throw new ArgumentException("Page number must be greater than 0");
            if (pageSize < 1) throw new ArgumentException("Page size must be greater than 0");

            if (!string.IsNullOrEmpty(status) && status.ToUpper() != "ALL")
            {
                if (!Enum.TryParse<RequestStatus>(status, true, out var _))
                {
                    throw new ArgumentException($"Invalid status value: {status}. Status must be one of: {string.Join(", ", Enum.GetNames<RequestStatus>())}");
                }
            }

            var query = _dbcontext.LeaveRequests
                .AsNoTracking()
                .Include(lr => lr.User)
                    .ThenInclude(u => u.Department)
                .Include(lr => lr.LeaveRequestType)
                .Select(lr => new
                {
                    LeaveRequest = lr,
                    lr.RequestStatus,
                    lr.CreatedAt,
                    lr.UserId
                });

            if (employeeIds != null && employeeIds.Any())
            {
                query = query.Where(r => employeeIds.Contains(r.UserId));
            }

            if (!string.IsNullOrEmpty(status) && status.ToUpper() != "ALL" &&
                Enum.TryParse<RequestStatus>(status, true, out var requestStatus))
            {
                query = query.Where(r => r.RequestStatus == requestStatus);
            }

            var totalCount = await query.CountAsync();

            var skip = (pageNumber - 1) * pageSize;

            var filteredIds = await query
                .OrderByDescending(lr => lr.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .Select(lr => lr.LeaveRequest.Id)
                .ToListAsync();

            var items = await _dbcontext.LeaveRequests
                .AsNoTracking()
                .Where(lr => filteredIds.Contains(lr.Id))
                .Include(lr => lr.User)
                    .ThenInclude(u => u.Department)
                .Include(lr => lr.LeaveRequestType)
                .OrderByDescending(lr => lr.CreatedAt)
                .ToListAsync();

            return (items, totalCount);
        }


    }
}

