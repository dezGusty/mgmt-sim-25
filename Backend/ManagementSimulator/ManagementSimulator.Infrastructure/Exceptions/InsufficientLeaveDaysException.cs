using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Infrastructure.Exceptions
{
    public class InsufficientLeaveDaysException : Exception
    {
        public int UserId { get; }
        public int LeaveRequestTypeId { get; }
        public int RequestedDays { get; }
        public int RemainingDays { get; }

        public InsufficientLeaveDaysException(int userId, int leaveRequestTypeId, int requestedDays, int remainingDays)
            : base($"User {userId} has insufficient leave days. Requested: {requestedDays}, Remaining: {remainingDays}")
        {
            UserId = userId;
            LeaveRequestTypeId = leaveRequestTypeId;
            RequestedDays = requestedDays;
            RemainingDays = remainingDays;
        }

        public InsufficientLeaveDaysException(int userId, int leaveRequestTypeId, int requestedDays, int remainingDays, string message)
            : base(message)
        {
            UserId = userId;
            LeaveRequestTypeId = leaveRequestTypeId;
            RequestedDays = requestedDays;
            RemainingDays = remainingDays;
        }

        public InsufficientLeaveDaysException(int userId, int leaveRequestTypeId, int requestedDays, int remainingDays, string message, Exception innerException)
            : base(message, innerException)
        {
            UserId = userId;
            LeaveRequestTypeId = leaveRequestTypeId;
            RequestedDays = requestedDays;
            RemainingDays = remainingDays;
        }
    }
}
