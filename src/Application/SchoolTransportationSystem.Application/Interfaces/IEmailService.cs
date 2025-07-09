namespace SchoolTransportationSystem.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = false);
        Task SendBulkEmailAsync(List<string> recipients, string subject, string body, bool isHtml = false);
        Task SendParentNotificationAsync(string parentEmail, string studentName, string message);
        Task SendEmergencyAlertAsync(List<string> recipients, string message);
        Task SendTripUpdateAsync(string parentEmail, string studentName, string tripStatus, string estimatedTime);
        Task SendMaintenanceReminderAsync(string driverEmail, string vehicleNumber, string maintenanceType, DateTime scheduledDate);
    }
}
