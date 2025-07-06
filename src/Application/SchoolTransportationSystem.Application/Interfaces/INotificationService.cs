using Rihla.Core.Entities;

namespace Rihla.Application.Interfaces
{
    public interface INotificationService
    {
        Task SendTripUpdateAsync(string tenantId, Trip trip);
        Task SendAttendanceUpdateAsync(string tenantId, Attendance attendance);
        Task SendEmergencyAlertAsync(string tenantId, string message);
        Task SendMaintenanceAlertAsync(string tenantId, MaintenanceRecord maintenance);
        Task SendTripLocationUpdateAsync(string tripId, double latitude, double longitude);
        Task SendStudentPickupNotificationAsync(string tenantId, int studentId, string status);
    }
}
