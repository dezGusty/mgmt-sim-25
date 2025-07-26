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
        Task<LeaveRequestType?> GetLeaveRequestTypesByDescriptionAsync(string description);
        Task<List<LeaveRequestType>> GetAllLeaveRequestTypesFilteredAsync(string? description, QueryParams parameters);
    }
}
