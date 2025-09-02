using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ManagementSimulator.Core.Services.Interfaces;

namespace ManagementSimulator.Core.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");

                using var client = new SmtpClient(smtpSettings["Host"], int.Parse(smtpSettings["Port"]))
                {
                    Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]),
                    EnableSsl = bool.Parse(smtpSettings["EnableSsl"])
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpSettings["FromEmail"], smtpSettings["FromName"]),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"The email was successfully sent to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email to {toEmail}: {ex.Message}");
                throw;
            }
        }

        public async Task SendWelcomeEmailWithPasswordAsync(string email, string firstName, string temporaryPassword)
        {
            var subject = "Your account has been created";

            var body = $@"
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset='UTF-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <title>Welcome - Account Created</title>
    </head>
    <body style='margin:0; padding:0; font-family: Arial, sans-serif; background-color:#f4f4f4;'>
        <table width='100%' cellpadding='0' cellspacing='0' style='background-color:#f4f4f4;'>
            <tr>
                <td align='center' style='padding:20px 0;'>
                    <table width='600' cellpadding='0' cellspacing='0' style='background-color:#ffffff; border-radius:8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
                        <tr>
                            <td style='background-color:#20B486; padding:30px; text-align:center; border-radius: 8px 8px 0 0;'>
                                <h1 style='color:#ffffff; margin:0; font-size:24px; font-weight:bold;'>Welcome to FTD Management System</h1>
                                <p style='color:#ffffff; margin:10px 0 0 0; font-size:16px; opacity:0.9;'>Your account has been created</p>
                            </td>
                        </tr>
                        <tr>
                            <td style='padding:30px;'>
                                <p style='font-size:16px; color:#333333; margin:0 0 20px 0;'>Hello <strong>{firstName}</strong>,</p>
                                <p style='font-size:16px; color:#666666; margin:0 0 25px 0; line-height:1.5;'>We're excited to let you know that your account has been successfully created in the <strong>FTD Management System</strong>.</p>

                                <table width='100%' cellpadding='0' cellspacing='0' style='background-color:#f8f9fa; border:1px solid #e9ecef; border-radius:6px; margin-bottom:25px;'>
                                    <tr>
                                        <td style='padding:20px;'>
                                            <h3 style='color:#333333; margin:0 0 15px 0; font-size:18px;'>Account Details</h3>
                                            <table width='100%' cellpadding='0' cellspacing='0'>
                                                <tr>
                                                    <td style='padding:6px 0; width:35%; font-size:12px; color:#666666; text-transform:uppercase; font-weight:bold;'>Email</td>
                                                    <td style='padding:6px 0; font-size:14px; color:#333333; font-weight:500;'>{email}</td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>

                                <table width='100%' cellpadding='0' cellspacing='0' style='background-color:#e3f2fd; border:1px solid #2196f3; border-radius:6px; margin-bottom:25px;'>
                                    <tr>
                                        <td style='padding:20px;'>
                                            <h3 style='color:#1976d2; margin:0 0 10px 0; font-size:18px;'>Next Steps</h3>
                                            <ol style='margin:0 0 0 20px; color:#333333; font-size:14px; line-height:1.8;'>
                                                <li>Open the application and click <strong>Reset password</strong>.</li>
                                                <li>Enter the email address where you received this message.</li>
                                                <li>Click <strong>Send Reset Code</strong> and check your email.</li>
                                                <li>Enter the received code along with your new password.</li>
                                                <li>Log in with your email and new password.</li>
                                            </ol>
                                        </td>
                                    </tr>
                                </table>

                                <p style='font-size:14px; color:#666666; margin:0 0 10px 0;'>If you have any questions or issues, please contact the system administrator.</p>
                                <p style='font-size:14px; color:#333333; margin:0;'>Best regards,<br><strong>FTD Management System</strong> Team</p>
                            </td>
                        </tr>
                        <tr>
                            <td style='background-color:#f8f9fa; padding:20px; text-align:center; border-radius:0 0 8px 8px; border-top:1px solid #e9ecef;'>
                                <p style='font-size:12px; color:#666666; margin:0; line-height:1.4;'>
                                    This is an automated notification from the <span style='color:#20B486; font-weight:bold;'><a href='http://localhost:4200/' style='color:#20B486;'>FTD Management System</a></span>.
                                    Please do not reply to this email.
                                </p>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    </body>
    </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPasswordResetCodeAsync(string email, string firstName, string resetCode)
        {
            var subject = "Password Reset Code - FTD Management System";

            var body = $@"
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset='UTF-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <title>Password Reset Code</title>
    </head>
    <body style='margin:0; padding:0; font-family: Arial, sans-serif; background-color:#f4f4f4;'>
        <table width='100%' cellpadding='0' cellspacing='0' style='background-color:#f4f4f4;'>
            <tr>
                <td align='center' style='padding:20px 0;'>
                    <table width='600' cellpadding='0' cellspacing='0' style='background-color:#ffffff; border-radius:8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
                        <tr>
                            <td style='background-color:#2196f3; padding:30px; text-align:center; border-radius: 8px 8px 0 0;'>
                                <h1 style='color:#ffffff; margin:0; font-size:24px; font-weight:bold;'>Password Reset</h1>
                                <p style='color:#ffffff; margin:10px 0 0 0; font-size:16px; opacity:0.9;'>Use the code below to reset your password</p>
                            </td>
                        </tr>
                        <tr>
                            <td style='padding:30px;'>
                                <p style='font-size:16px; color:#333333; margin:0 0 20px 0;'>Hello <strong>{firstName}</strong>,</p>
                                <p style='font-size:16px; color:#666666; margin:0 0 20px 0; line-height:1.5;'>You have requested to reset your password. Please use the verification code below:</p>

                                <table width='100%' cellpadding='0' cellspacing='0' style='background-color:#f8f9fa; border:1px solid #e9ecef; border-radius:6px; margin-bottom:20px;'>
                                    <tr>
                                        <td style='padding:20px; text-align:center;'>
                                            <div style='font-family: monospace; font-size: 22px; letter-spacing: 3px; color:#111827; background:#eef2ff; border:1px dashed #a5b4fc; display:inline-block; padding:12px 18px; border-radius:6px;'>
                                                {resetCode}
                                            </div>
                                        </td>
                                    </tr>
                                </table>

                                <table width='100%' cellpadding='0' cellspacing='0' style='background-color:#fff3cd; border:1px solid #ffeaa7; border-radius:6px; margin-bottom:25px;'>
                                    <tr>
                                        <td style='padding:20px;'>
                                            <strong style='color:#92400e;'>Important:</strong>
                                            <span style='color:#92400e;'> This code is valid for 15 minutes.</span>
                                        </td>
                                    </tr>
                                </table>

                                <p style='font-size:14px; color:#666666; margin:0 0 10px 0;'>Enter this code on the reset password page to set your new password.</p>
                                <p style='font-size:14px; color:#333333; margin:0;'>Best regards,<br><strong>FTD Management System</strong> Team</p>
                            </td>
                        </tr>
                        <tr>
                            <td style='background-color:#f8f9fa; padding:20px; text-align:center; border-radius:0 0 8px 8px; border-top:1px solid #e9ecef;'>
                                <p style='font-size:12px; color:#666666; margin:0; line-height:1.4;'>
                                    This is an automated notification from the <span style='color:#20B486; font-weight:bold;'><a href='http://localhost:4200/' style='color:#20B486;'>FTD Management System</a></span>.
                                    Please do not reply to this email.
                                </p>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    </body>
    </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendLeaveRequestNotificationToManagerAsync(string managerEmail, string managerName, string employeeName, string leaveType, DateTime startDate, DateTime endDate, int numberOfDays, string reason)
        {
            var subject = $"New Leave Request - {employeeName}";

            var body = $@"
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset='UTF-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <title>Leave Request Notification</title>
    </head>
    <body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
        <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4;'>
            <tr>
                <td align='center' style='padding: 20px 0;'>
                    <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
                        
                        <!-- Header -->
                        <tr>
                            <td style='background-color: #20B486; padding: 30px; text-align: center; border-radius: 8px 8px 0 0;'>
                                <h1 style='color: #ffffff; margin: 0; font-size: 24px; font-weight: bold;'>Leave Request Notification</h1>
                                <p style='color: #ffffff; margin: 10px 0 0 0; font-size: 16px; opacity: 0.9;'>New request awaiting your approval</p>
                            </td>
                        </tr>
                        
                        <!-- Content -->
                        <tr>
                            <td style='padding: 30px;'>
                                
                                <!-- Greeting -->
                                <p style='font-size: 18px; color: #333333; margin: 0 0 20px 0; font-weight: bold;'>
                                    Hello {managerName} 👋
                                </p>
                                
                                <!-- Description -->
                                <p style='font-size: 16px; color: #666666; margin: 0 0 30px 0; line-height: 1.5;'>
                                    A new leave request has been submitted by one of your team members and requires your review and approval.
                                </p>
                                
                                <!-- Details Card -->
                                <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f8f9fa; border: 1px solid #e9ecef; border-radius: 6px; margin-bottom: 25px;'>
                                    <tr>
                                        <td style='padding: 20px;'>
                                            <h3 style='color: #333333; margin: 0 0 20px 0; font-size: 18px;'>📋 Request Details</h3>
                                            
                                            <table width='100%' cellpadding='0' cellspacing='0'>
                                                <tr>
                                                    <td width='50%' style='padding: 8px 0;'>
                                                        <span style='font-size: 12px; color: #666666; text-transform: uppercase; font-weight: bold;'>Employee</span><br>
                                                        <span style='font-size: 14px; color: #333333; font-weight: 500;'>{employeeName}</span>
                                                    </td>
                                                    <td width='50%' style='padding: 8px 0;'>
                                                        <span style='font-size: 12px; color: #666666; text-transform: uppercase; font-weight: bold;'>Leave Type</span><br>
                                                        <span style='font-size: 14px; color: #333333; font-weight: 500;'>{leaveType}</span>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td width='50%' style='padding: 8px 0;'>
                                                        <span style='font-size: 12px; color: #666666; text-transform: uppercase; font-weight: bold;'>Start Date</span><br>
                                                        <span style='font-size: 14px; color: #333333; font-weight: 500;'>{startDate:dd MMM yyyy}</span>
                                                    </td>
                                                    <td width='50%' style='padding: 8px 0;'>
                                                        <span style='font-size: 12px; color: #666666; text-transform: uppercase; font-weight: bold;'>End Date</span><br>
                                                        <span style='font-size: 14px; color: #333333; font-weight: 500;'>{endDate:dd MMM yyyy}</span>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td width='50%' style='padding: 8px 0;'>
                                                        <span style='font-size: 12px; color: #666666; text-transform: uppercase; font-weight: bold;'>Duration</span><br>
                                                        <span style='font-size: 14px; color: #333333; font-weight: 500;'>{numberOfDays} day{(numberOfDays == 1 ? "" : "s")}</span>
                                                    </td>
                                                    <td width='50%' style='padding: 8px 0;'>
                                                        <span style='font-size: 12px; color: #666666; text-transform: uppercase; font-weight: bold;'>Status</span><br>
                                                        <span style='font-size: 14px; color: #f59e0b; font-weight: 600;'>⏳ Pending</span>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                                
                                {(string.IsNullOrEmpty(reason) ? "" : $@"
                                <!-- Reason Section -->
                                <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #fef3c7; border: 1px solid #f59e0b; border-radius: 6px; margin-bottom: 25px;'>
                                    <tr>
                                        <td style='padding: 20px;'>
                                            <span style='font-size: 12px; color: #92400e; text-transform: uppercase; font-weight: bold;'>Reason</span><br>
                                            <span style='font-size: 14px; color: #92400e; font-style: italic;'>""{reason}""</span>
                                        </td>
                                    </tr>
                                </table>
                                ")}
                                
                                <!-- Action Section -->
                                <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #e3f2fd; border: 1px solid #2196f3; border-radius: 6px; margin-bottom: 25px;'>
                                    <tr>
                                        <td style='padding: 25px; text-align: center;'>
                                            <h3 style='color: #1976d2; margin: 0 0 15px 0; font-size: 18px;'>⚡ Action Required</h3>
                                            <p style='color: #333333; margin: 0 0 25px 0; font-size: 14px; line-height: 1.5;'>
                                                Please review this request and take appropriate action. You can approve, reject, or request additional information. You will have to login into your account before taking any other actions.
                                            </p>
                                            
                                            <!-- CTA Button -->
                                            <table cellpadding='0' cellspacing='0' style='margin: 0 auto;'>
                                                <tr>
                                                    <td style='background-color: #2196f3; border-radius: 6px; padding: 15px 30px; text-align: center;'>
                                                        <a href='http://localhost:4200/manager/leave?employeeName={Uri.EscapeDataString(employeeName)}' 
                                                           style='color: #ffffff; text-decoration: none; font-size: 16px; font-weight: bold; display: block;'>
                                                            Review Request
                                                        </a>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                                
                            </td>
                        </tr>
                        
                        <!-- Footer -->
                        <tr>
                            <td style='background-color: #f8f9fa; padding: 20px; text-align: center; border-radius: 0 0 8px 8px; border-top: 1px solid #e9ecef;'>
                                <p style='font-size: 12px; color: #666666; margin: 0; line-height: 1.4;'>
                                    This is an automated notification from the <span style='color: #20B486; font-weight: bold;'><a href='http://localhost:4200/' style='color: #20B486;'> FTD Management System</a></span>.<br>
                                    Please do not reply to this email. For support, contact your system administrator.
                                </p>
                            </td>
                        </tr>
                        
                    </table>
                </td>
            </tr>
        </table>
    </body>
    </html>";

            await SendEmailAsync(managerEmail, subject, body);
        }

        public async Task SendLeaveRequestApprovedEmailAsync(string employeeEmail, string employeeName, string managerName, string leaveType, DateTime startDate, DateTime endDate, int numberOfDays, string? reviewerComment)
        {
            var subject = $"Leave Request Approved - {leaveType}";

            var body = $@"
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset='UTF-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <title>Leave Request Approved</title>
    </head>
    <body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
        <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4;'>
            <tr>
                <td align='center' style='padding: 20px 0;'>
                    <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
                        
                        <!-- Header -->
                        <tr>
                            <td style='background-color: #10b981; padding: 30px; text-align: center; border-radius: 8px 8px 0 0;'>
                                <h1 style='color: #ffffff; margin: 0; font-size: 24px; font-weight: bold;'>✅ Leave Request Approved</h1>
                                <p style='color: #ffffff; margin: 10px 0 0 0; font-size: 16px; opacity: 0.9;'>Your request has been approved by your manager</p>
                            </td>
                        </tr>
                        
                        <!-- Content -->
                        <tr>
                            <td style='padding: 30px;'>
                                
                                <!-- Greeting -->
                                <p style='font-size: 18px; color: #333333; margin: 0 0 20px 0; font-weight: bold;'>
                                    Congratulations {employeeName}! 🎉
                                </p>
                                
                                <!-- Description -->
                                <p style='font-size: 16px; color: #666666; margin: 0 0 30px 0; line-height: 1.5;'>
                                    Your leave request has been approved by <strong>{managerName}</strong>. You can now plan your time off accordingly.
                                </p>
                                
                                <!-- Details Card -->
                                <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f0fdf4; border: 1px solid #bbf7d0; border-radius: 6px; margin-bottom: 25px;'>
                                    <tr>
                                        <td style='padding: 20px;'>
                                            <h3 style='color: #166534; margin: 0 0 20px 0; font-size: 18px;'>📋 Approved Request Details</h3>
                                            
                                            <table width='100%' cellpadding='0' cellspacing='0'>
                                                <tr>
                                                    <td width='50%' style='padding: 8px 0;'>
                                                        <span style='font-size: 12px; color: #166534; text-transform: uppercase; font-weight: bold;'>Leave Type</span><br>
                                                        <span style='font-size: 14px; color: #333333; font-weight: 500;'>{leaveType}</span>
                                                    </td>
                                                    <td width='50%' style='padding: 8px 0;'>
                                                        <span style='font-size: 12px; color: #166534; text-transform: uppercase; font-weight: bold;'>Status</span><br>
                                                        <span style='font-size: 14px; color: #10b981; font-weight: 600;'>✅ Approved</span>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td width='50%' style='padding: 8px 0;'>
                                                        <span style='font-size: 12px; color: #166534; text-transform: uppercase; font-weight: bold;'>Start Date</span><br>
                                                        <span style='font-size: 14px; color: #333333; font-weight: 500;'>{startDate:dd MMM yyyy}</span>
                                                    </td>
                                                    <td width='50%' style='padding: 8px 0;'>
                                                        <span style='font-size: 12px; color: #166534; text-transform: uppercase; font-weight: bold;'>End Date</span><br>
                                                        <span style='font-size: 14px; color: #333333; font-weight: 500;'>{endDate:dd MMM yyyy}</span>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td width='50%' style='padding: 8px 0;'>
                                                        <span style='font-size: 12px; color: #166534; text-transform: uppercase; font-weight: bold;'>Duration</span><br>
                                                        <span style='font-size: 14px; color: #333333; font-weight: 500;'>{numberOfDays} day{(numberOfDays == 1 ? "" : "s")}</span>
                                                    </td>
                                                    <td width='50%' style='padding: 8px 0;'>
                                                        <span style='font-size: 12px; color: #166534; text-transform: uppercase; font-weight: bold;'>Approved By</span><br>
                                                        <span style='font-size: 14px; color: #333333; font-weight: 500;'>{managerName}</span>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                                
                                {(string.IsNullOrEmpty(reviewerComment) ? "" : $@"
                                <!-- Manager Comment -->
                                <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #eff6ff; border: 1px solid #3b82f6; border-radius: 6px; margin-bottom: 25px;'>
                                    <tr>
                                        <td style='padding: 20px;'>
                                            <span style='font-size: 12px; color: #1e40af; text-transform: uppercase; font-weight: bold;'>Manager Comment</span><br>
                                            <span style='font-size: 14px; color: #1e40af; font-style: italic;'>""{reviewerComment}""</span>
                                        </td>
                                    </tr>
                                </table>
                                ")}
                                
                                <!-- Next Steps -->
                                <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #fef3c7; border: 1px solid #f59e0b; border-radius: 6px; margin-bottom: 25px;'>
                                    <tr>
                                        <td style='padding: 20px;'>
                                            <h3 style='color: #92400e; margin: 0 0 15px 0; font-size: 18px;'>📝 Next Steps</h3>
                                            <ul style='color: #92400e; margin: 0; padding-left: 20px; line-height: 1.6;'>
                                                <li>Inform your team about your upcoming absence</li>
                                                <li>Set up out-of-office messages if needed</li>
                                                <li>Ensure all pending tasks are completed or delegated</li>
                                                <li>Enjoy your well-deserved time off!</li>
                                            </ul>
                                        </td>
                                    </tr>
                                </table>
                                
                            </td>
                        </tr>
                        
                        <!-- Footer -->
                        <tr>
                            <td style='background-color: #f8f9fa; padding: 20px; text-align: center; border-radius: 0 0 8px 8px; border-top: 1px solid #e9ecef;'>
                                <p style='font-size: 12px; color: #666666; margin: 0; line-height: 1.4;'>
                                    This is an automated notification from the <span style='color: #20B486; font-weight: bold;'><a href='http://localhost:4200/' style='color: #20B486;'>FTD Management System</a></span>.<br>
                                    Please do not reply to this email. For support, contact your system administrator.
                                </p>
                            </td>
                        </tr>
                        
                    </table>
                </td>
            </tr>
        </table>
    </body>
    </html>";

            await SendEmailAsync(employeeEmail, subject, body);
        }

        public async Task SendLeaveRequestRejectedEmailAsync(string employeeEmail, string employeeName, string managerName, string leaveType, DateTime startDate, DateTime endDate, int numberOfDays, string? reviewerComment)
        {
            var subject = $"Leave Request Rejected - {leaveType}";

            var body = $@"
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset='UTF-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <title>Leave Request Rejected</title>
    </head>
    <body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
        <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4;'>
            <tr>
                <td align='center' style='padding: 20px 0;'>
                    <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
                        
                        <!-- Header -->
                        <tr>
                            <td style='background-color: #ef4444; padding: 30px; text-align: center; border-radius: 8px 8px 0 0;'>
                                <h1 style='color: #ffffff; margin: 0; font-size: 24px; font-weight: bold;'>❌ Leave Request Rejected</h1>
                                <p style='color: #ffffff; margin: 10px 0 0 0; font-size: 16px; opacity: 0.9;'>Your request has been reviewed by your manager</p>
                            </td>
                        </tr>
                        
                        <!-- Content -->
                        <tr>
                            <td style='padding: 30px;'>
                                
                                <!-- Greeting -->
                                <p style='font-size: 18px; color: #333333; margin: 0 0 20px 0; font-weight: bold;'>
                                    Hello {employeeName},
                                </p>
                                
                                <!-- Description -->
                                <p style='font-size: 16px; color: #666666; margin: 0 0 30px 0; line-height: 1.5;'>
                                    Unfortunately, your leave request has been rejected by <strong>{managerName}</strong>. Please review the details below and contact your manager if you have any questions.
                                </p>
                                
                                <!-- Details Card -->
                                <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #fef2f2; border: 1px solid #fecaca; border-radius: 6px; margin-bottom: 25px;'>
                                    <tr>
                                        <td style='padding: 20px;'>
                                            <h3 style='color: #991b1b; margin: 0 0 20px 0; font-size: 18px;'>📋 Rejected Request Details</h3>
                                            
                                            <table width='100%' cellpadding='0' cellspacing='0'>
                                                <tr>
                                                    <td width='50%' style='padding: 8px 0;'>
                                                        <span style='font-size: 12px; color: #991b1b; text-transform: uppercase; font-weight: bold;'>Leave Type</span><br>
                                                        <span style='font-size: 14px; color: #333333; font-weight: 500;'>{leaveType}</span>
                                                    </td>
                                                    <td width='50%' style='padding: 8px 0;'>
                                                        <span style='font-size: 12px; color: #991b1b; text-transform: uppercase; font-weight: bold;'>Status</span><br>
                                                        <span style='font-size: 14px; color: #ef4444; font-weight: 600;'>❌ Rejected</span>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td width='50%' style='padding: 8px 0;'>
                                                        <span style='font-size: 12px; color: #991b1b; text-transform: uppercase; font-weight: bold;'>Start Date</span><br>
                                                        <span style='font-size: 14px; color: #333333; font-weight: 500;'>{startDate:dd MMM yyyy}</span>
                                                    </td>
                                                    <td width='50%' style='padding: 8px 0;'>
                                                        <span style='font-size: 12px; color: #991b1b; text-transform: uppercase; font-weight: bold;'>End Date</span><br>
                                                        <span style='font-size: 14px; color: #333333; font-weight: 500;'>{endDate:dd MMM yyyy}</span>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td width='50%' style='padding: 8px 0;'>
                                                        <span style='font-size: 12px; color: #991b1b; text-transform: uppercase; font-weight: bold;'>Duration</span><br>
                                                        <span style='font-size: 14px; color: #333333; font-weight: 500;'>{numberOfDays} day{(numberOfDays == 1 ? "" : "s")}</span>
                                                    </td>
                                                    <td width='50%' style='padding: 8px 0;'>
                                                        <span style='font-size: 12px; color: #991b1b; text-transform: uppercase; font-weight: bold;'>Rejected By</span><br>
                                                        <span style='font-size: 14px; color: #333333; font-weight: 500;'>{managerName}</span>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                                
                                {(string.IsNullOrEmpty(reviewerComment) ? "" : $@"
                                <!-- Manager Comment -->
                                <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #fef3c7; border: 1px solid #f59e0b; border-radius: 6px; margin-bottom: 25px;'>
                                    <tr>
                                        <td style='padding: 20px;'>
                                            <span style='font-size: 12px; color: #92400e; text-transform: uppercase; font-weight: bold;'>Manager Comment</span><br>
                                            <span style='font-size: 14px; color: #92400e; font-style: italic;'>""{reviewerComment}""</span>
                                        </td>
                                    </tr>
                                </table>
                                ")}
                                
                                <!-- Next Steps -->
                                <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #eff6ff; border: 1px solid #3b82f6; border-radius: 6px; margin-bottom: 25px;'>
                                    <tr>
                                        <td style='padding: 20px;'>
                                            <h3 style='color: #1e40af; margin: 0 0 15px 0; font-size: 18px;'>💡 Next Steps</h3>
                                            <ul style='color: #1e40af; margin: 0; padding-left: 20px; line-height: 1.6;'>
                                                <li>Contact your manager to discuss the rejection</li>
                                                <li>Consider submitting a new request with different dates</li>
                                                <li>Ensure you have sufficient leave days available</li>
                                                <li>Check if there are any conflicting requests</li>
                                            </ul>
                                        </td>
                                    </tr>
                                </table>
                                
                            </td>
                        </tr>
                        
                        <!-- Footer -->
                        <tr>
                            <td style='background-color: #f8f9fa; padding: 20px; text-align: center; border-radius: 0 0 8px 8px; border-top: 1px solid #e9ecef;'>
                                <p style='font-size: 12px; color: #666666; margin: 0; line-height: 1.4;'>
                                    This is an automated notification from the <span style='color: #20B486; font-weight: bold;'><a href='http://localhost:4200/' style='color: #20B486;'>FTD Management System</a></span>.<br>
                                    Please do not reply to this email. For support, contact your system administrator.
                                </p>
                            </td>
                        </tr>
                        
                    </table>
                </td>
            </tr>
        </table>
    </body>
    </html>";

            await SendEmailAsync(employeeEmail, subject, body);
        }
    }
}
