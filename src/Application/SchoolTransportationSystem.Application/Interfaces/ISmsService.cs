namespace Rihla.Application.Interfaces
{
    public interface ISmsService
    {
        Task SendSmsAsync(string phoneNumber, string message);
        Task SendBulkSmsAsync(List<string> phoneNumbers, string message);
        Task SendEmergencyAlertAsync(List<string> phoneNumbers, string message);
        Task SendParentNotificationAsync(string parentPhone, string studentName, string message);
        Task SendTripDelayNotificationAsync(string parentPhone, string studentName, int delayMinutes);
        Task SendPickupConfirmationAsync(string parentPhone, string studentName, DateTime pickupTime);
    }
}
