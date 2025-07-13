using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SchoolTransportationSystem.Application.Interfaces;
using System.Net;
using System.Net.Mail;

namespace SchoolTransportationSystem.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly SmtpClient _smtpClient;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            _smtpClient = new SmtpClient
            {
                Host = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com",
                Port = int.TryParse(_configuration["Email:SmtpPort"], out var port) ? port : 587,
                EnableSsl = bool.TryParse(_configuration["Email:EnableSsl"], out var enableSsl) ? enableSsl : true,
                Credentials = new NetworkCredential(
                    _configuration["Email:Username"],
                    _configuration["Email:Password"]
                )
            };
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
        {
            try
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_configuration["Email:FromAddress"] ?? "noreply@rihla.com", "Rihla Transportation"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                mailMessage.To.Add(to);

                await _smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent successfully to {to}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {to}");
                throw;
            }
        }

        public async Task SendBulkEmailAsync(List<string> recipients, string subject, string body, bool isHtml = false)
        {
            var tasks = recipients.Select(recipient => SendEmailAsync(recipient, subject, body, isHtml));
            await Task.WhenAll(tasks);
        }

        public async Task SendParentNotificationAsync(string parentEmail, string studentName, string message)
        {
            var subject = $"Notification for {studentName} - Rihla Transportation";
            var body = $@"
                <h2>Student Transportation Notification</h2>
                <p><strong>Student:</strong> {studentName}</p>
                <p><strong>Message:</strong> {message}</p>
                <p><strong>Time:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
                <br>
                <p>Best regards,<br>Rihla Transportation Team</p>
            ";

            await SendEmailAsync(parentEmail, subject, body, true);
        }

        public async Task SendEmergencyAlertAsync(List<string> recipients, string message)
        {
            var subject = "🚨 EMERGENCY ALERT - Rihla Transportation";
            var body = $@"
                <h2 style='color: red;'>EMERGENCY ALERT</h2>
                <p><strong>Alert:</strong> {message}</p>
                <p><strong>Time:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
                <p style='color: red;'><strong>Please take immediate action if required.</strong></p>
                <br>
                <p>Rihla Transportation Emergency System</p>
            ";

            await SendBulkEmailAsync(recipients, subject, body, true);
        }

        public async Task SendTripUpdateAsync(string parentEmail, string studentName, string tripStatus, string estimatedTime)
        {
            var subject = $"Trip Update for {studentName} - Rihla Transportation";
            var body = $@"
                <h2>Trip Status Update</h2>
                <p><strong>Student:</strong> {studentName}</p>
                <p><strong>Status:</strong> {tripStatus}</p>
                <p><strong>Estimated Time:</strong> {estimatedTime}</p>
                <p><strong>Update Time:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
                <br>
                <p>Best regards,<br>Rihla Transportation Team</p>
            ";

            await SendEmailAsync(parentEmail, subject, body, true);
        }

        public async Task SendMaintenanceReminderAsync(string driverEmail, string vehicleNumber, string maintenanceType, DateTime scheduledDate)
        {
            var subject = $"Maintenance Reminder - Vehicle {vehicleNumber}";
            var body = $@"
                <h2>Vehicle Maintenance Reminder</h2>
                <p><strong>Vehicle:</strong> {vehicleNumber}</p>
                <p><strong>Maintenance Type:</strong> {maintenanceType}</p>
                <p><strong>Scheduled Date:</strong> {scheduledDate:yyyy-MM-dd}</p>
                <p><strong>Reminder Sent:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
                <br>
                <p>Please ensure the vehicle is ready for maintenance on the scheduled date.</p>
                <p>Best regards,<br>Rihla Transportation Team</p>
            ";

            await SendEmailAsync(driverEmail, subject, body, true);
        }

        public async Task SendParentAccountCreatedAsync(string parentEmail, string parentName, string studentName, string tempPassword)
        {
            var subject = "Welcome to Rihla Transportation System - Account Created";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #667eea;'>Welcome to Rihla Transportation System</h2>
                    <p>Dear {parentName},</p>
                    <p>A parent account has been created for you to access your child's transportation information.</p>
                    
                    <div style='background-color: #f8fafc; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                        <h3 style='color: #4a5568; margin-top: 0;'>Student Information:</h3>
                        <p><strong>Student:</strong> {studentName}</p>
                    </div>
                    
                    <div style='background-color: #e6fffa; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                        <h3 style='color: #2d3748; margin-top: 0;'>Account Details:</h3>
                        <p><strong>Email:</strong> {parentEmail}</p>
                        <p><strong>Temporary Password:</strong> <code style='background-color: #f7fafc; padding: 4px 8px; border-radius: 4px;'>{tempPassword}</code></p>
                    </div>
                    
                    <div style='background-color: #fff5f5; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                        <h3 style='color: #c53030; margin-top: 0;'>Important: Account Activation Required</h3>
                        <ol>
                            <li>Visit the parent portal</li>
                            <li>Log in with your email and temporary password</li>
                            <li>Change your password immediately</li>
                            <li>Complete your profile setup</li>
                        </ol>
                    </div>
                    
                    <p>If you have any questions, please contact the school administration.</p>
                    <p>Best regards,<br><strong>Rihla Transportation Team</strong></p>
                </div>
            ";

            await SendEmailAsync(parentEmail, subject, body, true);
        }

        public async Task SendParentAccountActivationReminderAsync(string parentEmail, string parentName, string studentName)
        {
            var subject = "Reminder: Activate Your Rihla Parent Account";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #f56565;'>Account Activation Reminder</h2>
                    <p>Dear {parentName},</p>
                    <p>Your parent account for {studentName} has not been activated yet.</p>
                    <p>Please log in to the parent portal and complete your account setup to access your child's transportation information.</p>
                    <p>If you need assistance, please contact the school administration.</p>
                    <p>Best regards,<br><strong>Rihla Transportation Team</strong></p>
                </div>
            ";

            await SendEmailAsync(parentEmail, subject, body, true);
        }

        public void Dispose()
        {
            _smtpClient?.Dispose();
        }
    }
}
