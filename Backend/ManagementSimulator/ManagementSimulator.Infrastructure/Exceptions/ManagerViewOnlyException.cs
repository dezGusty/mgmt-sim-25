using System;

namespace ManagementSimulator.Infrastructure.Exceptions
{
    public class ManagerViewOnlyException : Exception
    {
        public int ManagerId { get; }
        public int? ActiveSecondManagerId { get; }

        public ManagerViewOnlyException(int managerId, int? activeSecondManagerId = null)
             : base($"The manager with ID {managerId} is temporarily in view-only mode due to an active second manager.")
        {
            ManagerId = managerId;
            ActiveSecondManagerId = activeSecondManagerId;
        }

        public ManagerViewOnlyException(int managerId, int? activeSecondManagerId, string customMessage)
             : base(customMessage)
        {
            ManagerId = managerId;
            ActiveSecondManagerId = activeSecondManagerId;
        }
    }
}