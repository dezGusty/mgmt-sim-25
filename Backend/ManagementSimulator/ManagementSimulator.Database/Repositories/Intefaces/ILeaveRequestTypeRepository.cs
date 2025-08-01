using ManagementSimulator.Database.Dtos.QueryParams;
using ManagementSimulator.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Database.Repositories.Intefaces
{
    public interface ILeaveRequestTypeRepository : IBaseRepostory<LeaveRequestType>
    {
        Task<LeaveRequestType?> GetLeaveRequestTypesByTitleAsync(string title, bool includeDeleted = false);
        Task<(List<LeaveRequestType> Data, int TotalCount)> GetAllLeaveRequestTypesFilteredAsync(string? title, QueryParams parameters, bool includeDeleted = false);
    }
}
