using Microsoft.AspNetCore.SignalR;
using Rihla.Application.Interfaces;
using Rihla.Core.Entities;

namespace Rihla.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<Hub> _hubContext;

        public NotificationService(IHubContext<Hub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendTripUpdateAsync(string tenantId, Trip trip)
        {
            await _hubContext.Clients.Group($"tenant_{tenantId}")
                .SendAsync("TripUpdate", new
                {
                    TripId = trip.Id,
                    Status = trip.Status.ToString(),
                    CurrentLocation = new { trip.Vehicle?.CurrentLatitude, trip.Vehicle?.CurrentLongitude },
                    StudentsPickedUp = trip.StudentsPickedUp,
                    StudentsDroppedOff = trip.StudentsDroppedOff,
                    Timestamp = DateTime.UtcNow
                });
        }

        public async Task SendAttendanceUpdateAsync(string tenantId, Attendance attendance)
        {
            await _hubContext.Clients.Group($"tenant_{tenantId}")
                .SendAsync("AttendanceUpdate", new
                {
                    StudentId = attendance.StudentId,
                    TripId = attendance.TripId,
                    Status = attendance.Status.ToString(),
                    Timestamp = attendance.Timestamp,
                    Location = new { attendance.Latitude, attendance.Longitude }
                });
        }

        public async Task SendEmergencyAlertAsync(string tenantId, string message)
        {
            await _hubContext.Clients.Group($"tenant_{tenantId}")
                .SendAsync("EmergencyAlert", new
                {
                    Message = message,
                    Timestamp = DateTime.UtcNow,
                    Type = "Emergency"
                });
        }

        public async Task SendMaintenanceAlertAsync(string tenantId, MaintenanceRecord maintenance)
        {
            await _hubContext.Clients.Group($"tenant_{tenantId}")
                .SendAsync("MaintenanceAlert", new
                {
                    VehicleId = maintenance.VehicleId,
                    Type = maintenance.Type.ToString(),
                    Priority = maintenance.Priority.ToString(),
                    Description = maintenance.Description,
                    ScheduledDate = maintenance.ScheduledDate,
                    Timestamp = DateTime.UtcNow
                });
        }

        public async Task SendTripLocationUpdateAsync(string tripId, double latitude, double longitude)
        {
            await _hubContext.Clients.Group($"trip_{tripId}")
                .SendAsync("LocationUpdate", new
                {
                    TripId = tripId,
                    Latitude = latitude,
                    Longitude = longitude,
                    Timestamp = DateTime.UtcNow
                });
        }

        public async Task SendStudentPickupNotificationAsync(string tenantId, int studentId, string status)
        {
            await _hubContext.Clients.Group($"tenant_{tenantId}")
                .SendAsync("StudentPickupNotification", new
                {
                    StudentId = studentId,
                    Status = status,
                    Timestamp = DateTime.UtcNow
                });
        }
    }
}
