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
        Task<(List<LeaveRequest> Data, int TotalCount)> GetAllLeaveRequestsWithRelationshipsFilteredAsync(List<int> employeeIds, string? lastName, string? email, QueryParams parameters, bool includeDeleted = false);
        Task<List<LeaveRequest>> GetOverlappingRequestsAsync(int userId, DateTime startDate, DateTime endDate);
        Task<List<LeaveRequest>> GetLeaveRequestsByUserAndTypeAsync(int userId, int leaveRequestTypeId, int year, bool includeDeleted = false);
    }
}
