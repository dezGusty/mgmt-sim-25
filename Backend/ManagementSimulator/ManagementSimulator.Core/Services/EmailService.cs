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

        // ...existing code...

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
    }
}
