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

        public void Dispose()
        {
            _smtpClient?.Dispose();
        }
    }
}
