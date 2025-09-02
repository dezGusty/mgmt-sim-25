using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendWelcomeEmailWithPasswordAsync(string email, string firstName, string temporaryPassword);
        Task SendPasswordResetCodeAsync(string email, string firstName, string resetCode);
        Task SendLeaveRequestNotificationToManagerAsync(string managerEmail, string managerName, string employeeName, string leaveType, DateTime startDate, DateTime endDate, int numberOfDays, string reason);
        Task SendLeaveRequestApprovedEmailAsync(string employeeEmail, string employeeName, string managerName, string leaveType, DateTime startDate, DateTime endDate, int numberOfDays, string? reviewerComment);
        Task SendLeaveRequestRejectedEmailAsync(string employeeEmail, string employeeName, string managerName, string leaveType, DateTime startDate, DateTime endDate, int numberOfDays, string? reviewerComment);
    }
}
