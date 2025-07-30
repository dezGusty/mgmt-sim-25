using System;

namespace ManagementSimulator.Infrastructure.Exceptions
{
    public class LeaveRequestOverlapException : Exception
    {
        public LeaveRequestOverlapException(string message) : base(message)
        {
        }

        public LeaveRequestOverlapException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
