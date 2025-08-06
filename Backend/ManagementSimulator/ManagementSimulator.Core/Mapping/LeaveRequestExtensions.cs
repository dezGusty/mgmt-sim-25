using ManagementSimulator.Core.Dtos.Responses.LeaveRequest;
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
                FullName = entity.User?.FullName ?? string.Empty,
                ReviewerId = entity.ReviewerId,
                LeaveRequestTypeId = entity.LeaveRequestTypeId,
                LeaveRequestTypeName = entity.LeaveRequestType?.Title ?? string.Empty,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                Reason = entity.Reason ?? string.Empty,
                RequestStatus = entity.RequestStatus,
                ReviewerComment = entity.ReviewerComment ?? string.Empty,
                CreatedAt = entity.CreatedAt,
                DepartmentName = entity.User?.Department?.Name ?? string.Empty,
            };
        }

        public static CreateLeaveRequestResponseDto ToCreateLeaveRequestResponseDto(this LeaveRequest entity)
        {
            return new CreateLeaveRequestResponseDto
            {
                Id = entity.Id,
                CreatedAt = entity.CreatedAt,
                FullName = entity.User?.FullName ?? string.Empty,
                LeaveRequestTypeId = entity.LeaveRequestTypeId,
                LeaveRequestTypeName = entity.LeaveRequestType?.Title ?? string.Empty,
                LeaveRequestTypeIsPaid = entity.LeaveRequestType?.IsPaid ?? false,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                Reason = entity.Reason ?? string.Empty,
                RequestStatus = entity.RequestStatus,
                ReviewerComment = entity.ReviewerComment ?? string.Empty,
                DepartmentName = entity.User?.Department?.Name ?? string.Empty,
            };
        }
    }
}
                
