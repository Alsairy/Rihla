using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SchoolTransportationSystem.Application.Interfaces;

namespace SchoolTransportationSystem.Application.Services
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
                var baseUrl = _configuration["SMS:BaseUrl"];

                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogWarning("SMS service not configured. Message would be sent to {PhoneNumber}: {Message}", phoneNumber, message);
                    return;
                }

                if (string.IsNullOrEmpty(baseUrl))
                {
                    _logger.LogError("SMS BaseUrl not configured");
                    throw new InvalidOperationException("SMS BaseUrl not configured");
                }

                var payload = new
                {
                    to = phoneNumber,
                    message = message,
                    sender_id = senderId
                };

                var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var maxRetries = 3;
                var retryDelay = TimeSpan.FromSeconds(1);

                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        _logger.LogInformation("Attempting to send SMS to {PhoneNumber} (attempt {Attempt}/{MaxRetries})", phoneNumber, attempt, maxRetries);

                        var response = await _httpClient.PostAsync($"{baseUrl}/messages", content);

                        if (response.IsSuccessStatusCode)
                        {
                            var responseContent = await response.Content.ReadAsStringAsync();
                            _logger.LogInformation("SMS sent successfully to {PhoneNumber}: {Message}. Response: {Response}", phoneNumber, message, responseContent);
                            return;
                        }
                        else
                        {
                            var errorContent = await response.Content.ReadAsStringAsync();
                            _logger.LogWarning("SMS sending failed for {PhoneNumber} with status {StatusCode}: {Error}", phoneNumber, response.StatusCode, errorContent);

                            if (attempt == maxRetries)
                            {
                                throw new HttpRequestException($"Failed to send SMS after {maxRetries} attempts. Status: {response.StatusCode}, Error: {errorContent}");
                            }
                        }
                    }
                    catch (HttpRequestException ex) when (attempt < maxRetries)
                    {
                        _logger.LogWarning(ex, "HTTP request failed for SMS to {PhoneNumber} (attempt {Attempt}/{MaxRetries}). Retrying in {Delay}ms", phoneNumber, attempt, maxRetries, retryDelay.TotalMilliseconds);
                        await Task.Delay(retryDelay);
                        retryDelay = TimeSpan.FromMilliseconds(retryDelay.TotalMilliseconds * 2); // Exponential backoff
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", phoneNumber);
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
