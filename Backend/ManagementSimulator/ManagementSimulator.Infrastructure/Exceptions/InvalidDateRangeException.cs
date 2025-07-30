using System;

namespace ManagementSimulator.Infrastructure.Exceptions
{
    public class InvalidDateRangeException : Exception
    {
        public InvalidDateRangeException(string message) : base(message)
        {
        }

        public InvalidDateRangeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
