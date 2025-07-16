using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Mapping
{
    public static class LeaveRequestExtensions
    {
        public static LeaveRequestResponseDto ToLeaveRequestResponseDto(this LeaveRequest entity)
        {
            return new LeaveRequestResponseDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                UserName = entity.User?.FullName ?? string.Empty,

                ReviewerId = entity.ReviewerId,
                ReviewerName = entity.Reviewer?.FullName ?? string.Empty,

                LeaveRequestTypeId = entity.LeaveRequestTypeId,
                LeaveRequestTypeName = entity.LeaveRequestType?.Description ?? string.Empty,

                StartDate = entity.StartDate,
                EndDate = entity.EndDate,

                Reason = entity.Reason ?? string.Empty,
                IsApproved = entity.IsApproved,
                RequestStatus = entity.RequestStatus,
                ReviewerComment = entity.ReviewerComment ?? string.Empty
            };
        }
    }
}
