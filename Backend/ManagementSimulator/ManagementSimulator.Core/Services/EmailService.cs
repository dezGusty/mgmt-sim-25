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
                _logger.LogInformation($"Email trimis cu succes către {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eroare la trimiterea email-ului către {toEmail}: {ex.Message}");
                throw;
            }
        }

        public async Task SendWelcomeEmailWithPasswordAsync(string email, string firstName, string temporaryPassword)
        {
            var subject = "Contul dvs. a fost creat";

            var body = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; }}
                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                .header {{ background-color: #f8f9fa; padding: 20px; text-align: center; }}
                .content {{ padding: 20px; }}
                .password-box {{ 
                    background-color: #e9ecef; 
                    border: 1px solid #ced4da; 
                    padding: 15px; 
                    margin: 20px 0; 
                    font-family: monospace; 
                    font-size: 16px;
                    border-radius: 5px;
                }}
                .warning {{ 
                    background-color: #fff3cd; 
                    border: 1px solid #ffeaa7; 
                    padding: 15px; 
                    margin: 20px 0; 
                    border-radius: 5px;
                }}
                .footer {{ 
                    background-color: #f8f9fa; 
                    padding: 15px; 
                    text-align: center; 
                    font-size: 12px; 
                    color: #6c757d;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h2>Welcome!</h2>
                </div>
        
                <div class='content'>
                    <p>Hello <strong>{firstName}</strong>,</p>
            
                    <p>Your account has been successfully created in the Siemens MGMT system.</p>
            
                    <p><strong>Account details:</strong></p>
                    <ul>
                        <li><strong>Email:</strong> {email}</li>
                    </ul>
            
                    <p><strong>Steps for login:</strong></p>
                    <ol>
                        <li>Access the application and click on <strong>Reset password</strong> in the bottom-right corner</li>
                        <li>Enter the email address on which you received this message</li>
                        <li>Click on the <strong>Send Reset Code</strong> button</li>
                        <li>Enter the code received along with your new password</li>
                        <li>After redirection, enter your email and new password on the login page</li>
                    </ol>

            
                    <p>If you have any questions or issues, please contact us.</p>
            
                    <p>Best regards,<br>The Siemens MGMT Administration Team</p>
                </div>
        
                <div class='footer'>
                    <p>This email was generated automatically. Please do not reply to this message.</p>
                </div>
            </div>
        </body>
        </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPasswordResetCodeAsync(string email, string firstName, string resetCode)
        {
            var subject = "Cod de resetare parolă - Siemens MGMT";

            var body = $@"
    <!DOCTYPE html>
    <html>
    <head>
        <style>
            body {{ font-family: Arial, sans-serif; }}
            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
            .header {{ background-color: #f8f9fa; padding: 20px; text-align: center; }}
            .content {{ padding: 20px; }}
            .password-box {{ 
                background-color: #e9ecef; 
                border: 1px solid #ced4da; 
                padding: 15px; 
                margin: 20px 0; 
                font-family: monospace; 
                font-size: 16px;
                border-radius: 5px;
            }}
            .warning {{ 
                background-color: #fff3cd; 
                border: 1px solid #ffeaa7; 
                padding: 15px; 
                margin: 20px 0; 
                border-radius: 5px;
            }}
            .footer {{ 
                background-color: #f8f9fa; 
                padding: 15px; 
                text-align: center; 
                font-size: 12px; 
                color: #6c757d;
            }}
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='header'>
                <h2>Password Reset - Siemens MGMT</h2>
            </div>
    
            <div class='content'>
                <p>Hello <strong>{firstName}</strong>,</p>
        
                <p>You have requested to reset your password.</p>
        
                <p><strong>Your verification code:</strong></p>
        
                <div class='password-box'>
                    {resetCode}
                </div>
        
                <div class='warning'>
                    <strong>Important:</strong> This code is valid for 15 minutes.
                </div>
        
                <p>Use this code on the reset password page to set your new password.</p>
        
                <p>Best regards,<br>The Siemens MGMT Team</p>
            </div>
    
            <div class='footer'>
                <p>This email was generated automatically. Please do not reply to this message.</p>
            </div>
        </div>
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
                                                        <a href='http://localhost:4200/manager/leave' 
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
    }
}
