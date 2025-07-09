using SchoolTransportationSystem.Core.Entities;
using SchoolTransportationSystem.Core.Common;
using SchoolTransportationSystem.Application.DTOs;

namespace SchoolTransportationSystem.Application.Interfaces
{
    public interface INotificationService
    {
        Task<Result<bool>> SendTripUpdateAsync(string tenantId, Trip trip);
        Task<Result<bool>> SendAttendanceUpdateAsync(string tenantId, Attendance attendance);
        Task<Result<bool>> SendEmergencyAlertAsync(string tenantId, string message);
        Task<Result<bool>> SendMaintenanceAlertAsync(string tenantId, MaintenanceRecord maintenance);
        Task<Result<bool>> SendTripLocationUpdateAsync(string tripId, double latitude, double longitude);
        Task<Result<bool>> SendStudentPickupNotificationAsync(string tenantId, int studentId, string status);
        Task<Result<List<NotificationDto>>> GetNotificationsAsync(string tenantId, int userId, bool unreadOnly = false);
        Task<Result<bool>> MarkNotificationAsReadAsync(int notificationId, string username);
    }
}
