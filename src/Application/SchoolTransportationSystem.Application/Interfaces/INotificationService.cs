using Rihla.Core.Entities;
using Rihla.Core.Common;
using Rihla.Application.DTOs;

namespace Rihla.Application.Interfaces
{
    public interface INotificationService
    {
        Task<Result<bool>> SendTripUpdateAsync(string tenantId, Trip trip);
        Task<Result<bool>> SendAttendanceUpdateAsync(string tenantId, Attendance attendance);
        Task<Result<bool>> SendEmergencyAlertAsync(string tenantId, string message);
        Task<Result<bool>> SendMaintenanceAlertAsync(string tenantId, MaintenanceRecord maintenance);
        Task<Result<bool>> SendTripLocationUpdateAsync(string tripId, double latitude, double longitude, string tenantId);
        Task<Result<bool>> SendStudentPickupNotificationAsync(string tenantId, int studentId, string status);
        Task<Result<List<NotificationDto>>> GetNotificationsAsync(string tenantId, int userId, bool unreadOnly = false);
        Task<Result<bool>> MarkNotificationAsReadAsync(int notificationId, string readBy, string tenantId);
    }
}
