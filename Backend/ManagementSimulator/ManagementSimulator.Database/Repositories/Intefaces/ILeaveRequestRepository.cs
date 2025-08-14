using ManagementSimulator.Database.Dtos.QueryParams;
using ManagementSimulator.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Database.Repositories.Intefaces
{
    public interface ILeaveRequestRepository : IBaseRepostory<LeaveRequest>
    {
        Task<(List<LeaveRequest> Data, int TotalCount)> GetAllLeaveRequestsWithRelationshipsFilteredAsync(List<int> employeeIds, string? lastName, string? email, QueryParams parameters, bool includeDeleted = false, bool tracking = false);
        Task<List<LeaveRequest>> GetOverlappingRequestsAsync(int userId, DateTime startDate, DateTime endDate, bool tracking = false);
        Task<List<LeaveRequest>> GetLeaveRequestsByUserAndTypeAsync(int userId, int leaveRequestTypeId, int year, bool includeDeleted = false, bool tracking = false);
        Task<List<LeaveRequest>> GetAllWithRelationshipsAsync(bool includeDeleted = false, bool tracking = false);
        Task<LeaveRequest> GetLeaveRequestWithDetailsAsync(int id, bool includeDeleted = false, bool tracking = false);
        Task<(List<LeaveRequest> Items, int TotalCount)> GetFilteredLeaveRequestsAsync(string status, int pageSize, int pageNumber = 1, List<int>? employeeIds = null);
        Task<List<LeaveRequest>> GetAllWithRelationshipsByUserIdsAsync(List<int> userIds, string? name = null, bool includeDeleted = false, bool tracking = false);
    }
}
