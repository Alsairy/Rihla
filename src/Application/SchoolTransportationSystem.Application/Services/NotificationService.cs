using Rihla.Application.Interfaces;
using Rihla.Core.Entities;

namespace Rihla.Application.Services
{
    public class NotificationService : INotificationService
    {
        public async Task SendTripUpdateAsync(string tenantId, Trip trip)
        {
            await Task.CompletedTask;
        }

        public async Task SendAttendanceUpdateAsync(string tenantId, Attendance attendance)
        {
            await Task.CompletedTask;
        }

        public async Task SendEmergencyAlertAsync(string tenantId, string message)
        {
            await Task.CompletedTask;
        }

        public async Task SendMaintenanceAlertAsync(string tenantId, MaintenanceRecord maintenance)
        {
            await Task.CompletedTask;
        }

        public async Task SendTripLocationUpdateAsync(string tripId, double latitude, double longitude)
        {
            await Task.CompletedTask;
        }

        public async Task SendStudentPickupNotificationAsync(string tenantId, int studentId, string status)
        {
            await Task.CompletedTask;
        }
    }
}
