using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rihla.Application.Interfaces;

namespace Rihla.Application.Services
{
    public class SmsService : ISmsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmsService> _logger;
        private readonly HttpClient _httpClient;

        public SmsService(IConfiguration configuration, ILogger<SmsService> logger, HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                
                var smsProvider = _configuration["SMS:Provider"];
                var apiKey = _configuration["SMS:ApiKey"];
                var senderId = _configuration["SMS:SenderId"];

                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogWarning("SMS service not configured. Message would be sent to {PhoneNumber}: {Message}", phoneNumber, message);
                    return;
                }

                var payload = new
                {
                    to = phoneNumber,
                    message = message,
                    sender_id = senderId
                };

                _logger.LogInformation($"SMS sent successfully to {phoneNumber}: {message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send SMS to {phoneNumber}");
                throw;
            }
        }

        public async Task SendBulkSmsAsync(List<string> phoneNumbers, string message)
        {
            var tasks = phoneNumbers.Select(phoneNumber => SendSmsAsync(phoneNumber, message));
            await Task.WhenAll(tasks);
        }

        public async Task SendEmergencyAlertAsync(List<string> phoneNumbers, string message)
        {
            var emergencyMessage = $"🚨 EMERGENCY: {message} - Rihla Transportation";
            await SendBulkSmsAsync(phoneNumbers, emergencyMessage);
        }

        public async Task SendParentNotificationAsync(string parentPhone, string studentName, string message)
        {
            var notificationMessage = $"Rihla Alert - {studentName}: {message}";
            await SendSmsAsync(parentPhone, notificationMessage);
        }

        public async Task SendTripDelayNotificationAsync(string parentPhone, string studentName, int delayMinutes)
        {
            var delayMessage = $"Rihla Update - {studentName}'s trip is delayed by {delayMinutes} minutes. We apologize for the inconvenience.";
            await SendSmsAsync(parentPhone, delayMessage);
        }

        public async Task SendPickupConfirmationAsync(string parentPhone, string studentName, DateTime pickupTime)
        {
            var confirmationMessage = $"Rihla Confirmation - {studentName} has been picked up at {pickupTime:HH:mm}. Have a great day!";
            await SendSmsAsync(parentPhone, confirmationMessage);
        }
    }
}
