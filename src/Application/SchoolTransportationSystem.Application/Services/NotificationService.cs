using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rihla.Application.DTOs;
using Rihla.Application.Interfaces;
using Rihla.Core.Common;
using Rihla.Core.Entities;
using Rihla.Core.Enums;
using Rihla.Infrastructure.Data;

namespace Rihla.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            ApplicationDbContext context,
            IEmailService emailService,
            ISmsService smsService,
            ILogger<NotificationService> logger)
        {
            _context = context;
            _emailService = emailService;
            _smsService = smsService;
            _logger = logger;
        }

        public async Task<Result<bool>> SendTripUpdateAsync(string tenantId, Trip trip)
        {
            try
            {
                _logger.LogInformation("Sending trip update notification for trip {TripId} in tenant {TenantId}", trip.Id, tenantId);

                var students = await _context.Students
                    .Where(s => s.RouteId == trip.RouteId && s.TenantId.ToString() == tenantId && !s.IsDeleted)
                    .ToListAsync();

                var notifications = new List<Notification>();
                var emailTasks = new List<Task>();
                var smsTasks = new List<Task>();

                foreach (var student in students)
                {
                    var notification = new Notification
                    {
                        Type = "TripUpdate",
                        Title = $"Trip Update - Route {trip.Route?.RouteNumber}",
                        Message = $"Trip status updated to {trip.Status}. Scheduled time: {trip.ScheduledStartTime:HH:mm}",
                        Priority = trip.Status == TripStatus.Cancelled ? NotificationPriority.High : NotificationPriority.Normal,
                        UserId = student.Id,
                        RelatedEntityId = trip.Id,
                        RelatedEntityType = "Trip",
                        Channel = NotificationChannel.InApp,
                        TenantId = int.Parse(tenantId),
                        CreatedBy = "System"
                    };

                    notifications.Add(notification);

                    if (!string.IsNullOrEmpty(student.ParentEmail))
                    {
                        emailTasks.Add(_emailService.SendEmailAsync(
                            student.ParentEmail,
                            notification.Title,
                            notification.Message));
                    }

                    if (!string.IsNullOrEmpty(student.ParentPhone))
                    {
                        smsTasks.Add(_smsService.SendSmsAsync(student.ParentPhone, notification.Message));
                    }
                }

                _context.Notifications.AddRange(notifications);
                await _context.SaveChangesAsync();

                await Task.WhenAll(emailTasks.Concat(smsTasks));

                _logger.LogInformation("Successfully sent trip update notifications to {Count} students", students.Count);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending trip update notification for trip {TripId}", trip.Id);
                return Result<bool>.Failure($"Failed to send trip update notification: {ex.Message}");
            }
        }

        public async Task<Result<bool>> SendAttendanceUpdateAsync(string tenantId, Attendance attendance)
        {
            try
            {
                _logger.LogInformation("Sending attendance update notification for student {StudentId}", attendance.StudentId);

                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.Id == attendance.StudentId && s.TenantId.ToString() == tenantId && !s.IsDeleted);

                if (student == null)
                {
                    return Result<bool>.Failure("Student not found");
                }

                var notification = new Notification
                {
                    Type = "AttendanceUpdate",
                    Title = "Attendance Update",
                    Message = $"Student {student.FullName.FirstName} {student.FullName.LastName} marked as {attendance.Status} on {attendance.Date:yyyy-MM-dd}",
                    Priority = attendance.Status == AttendanceStatus.Absent ? NotificationPriority.High : NotificationPriority.Normal,
                    UserId = student.Id,
                    RelatedEntityId = attendance.Id,
                    RelatedEntityType = "Attendance",
                    Channel = NotificationChannel.InApp,
                    TenantId = int.Parse(tenantId),
                    CreatedBy = attendance.RecordedBy
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                var tasks = new List<Task>();

                if (!string.IsNullOrEmpty(student.ParentEmail))
                {
                    tasks.Add(_emailService.SendEmailAsync(
                        student.ParentEmail,
                        notification.Title,
                        notification.Message));
                }

                if (!string.IsNullOrEmpty(student.ParentPhone))
                {
                    tasks.Add(_smsService.SendSmsAsync(student.ParentPhone, notification.Message));
                }

                await Task.WhenAll(tasks);

                _logger.LogInformation("Successfully sent attendance update notification for student {StudentId}", attendance.StudentId);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending attendance update notification for student {StudentId}", attendance.StudentId);
                return Result<bool>.Failure($"Failed to send attendance update notification: {ex.Message}");
            }
        }

        public async Task<Result<bool>> SendEmergencyAlertAsync(string tenantId, string message)
        {
            try
            {
                _logger.LogInformation("Sending emergency alert to all users in tenant {TenantId}", tenantId);

                var users = await _context.Users
                    .Where(u => u.TenantId == tenantId && u.IsActive)
                    .ToListAsync();

                var students = await _context.Students
                    .Where(s => s.TenantId.ToString() == tenantId && !s.IsDeleted)
                    .ToListAsync();

                var drivers = await _context.Drivers
                    .Where(d => d.TenantId.ToString() == tenantId && !d.IsDeleted)
                    .ToListAsync();

                var notifications = new List<Notification>();
                var emailTasks = new List<Task>();
                var smsTasks = new List<Task>();

                foreach (var user in users)
                {
                    var notification = new Notification
                    {
                        Type = "EmergencyAlert",
                        Title = "EMERGENCY ALERT",
                        Message = message,
                        Priority = NotificationPriority.Critical,
                        UserId = user.Id,
                        Channel = NotificationChannel.InApp,
                        TenantId = int.Parse(tenantId),
                        CreatedBy = "System"
                    };

                    notifications.Add(notification);

                    emailTasks.Add(_emailService.SendEmailAsync(user.Email, notification.Title, notification.Message));
                }

                foreach (var student in students)
                {
                    if (!string.IsNullOrEmpty(student.ParentEmail))
                    {
                        emailTasks.Add(_emailService.SendEmailAsync(student.ParentEmail, "EMERGENCY ALERT", message));
                    }
                    if (!string.IsNullOrEmpty(student.ParentPhone))
                    {
                        smsTasks.Add(_smsService.SendSmsAsync(student.ParentPhone, $"EMERGENCY: {message}"));
                    }
                }

                foreach (var driver in drivers)
                {
                    if (!string.IsNullOrEmpty(driver.Email))
                    {
                        emailTasks.Add(_emailService.SendEmailAsync(driver.Email, "EMERGENCY ALERT", message));
                    }
                    if (!string.IsNullOrEmpty(driver.Phone))
                    {
                        smsTasks.Add(_smsService.SendSmsAsync(driver.Phone, $"EMERGENCY: {message}"));
                    }
                }

                _context.Notifications.AddRange(notifications);
                await _context.SaveChangesAsync();

                await Task.WhenAll(emailTasks.Concat(smsTasks));

                _logger.LogInformation("Successfully sent emergency alert to {UserCount} users, {StudentCount} parents, {DriverCount} drivers", 
                    users.Count, students.Count, drivers.Count);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending emergency alert in tenant {TenantId}", tenantId);
                return Result<bool>.Failure($"Failed to send emergency alert: {ex.Message}");
            }
        }

        public async Task<Result<bool>> SendMaintenanceAlertAsync(string tenantId, MaintenanceRecord maintenance)
        {
            try
            {
                _logger.LogInformation("Sending maintenance alert for vehicle {VehicleId}", maintenance.VehicleId);

                var vehicle = await _context.Vehicles
                    .Include(v => v.AssignedDriver)
                    .FirstOrDefaultAsync(v => v.Id == maintenance.VehicleId && v.TenantId.ToString() == tenantId && !v.IsDeleted);

                if (vehicle == null)
                {
                    return Result<bool>.Failure("Vehicle not found");
                }

                var managers = await _context.Users
                    .Where(u => u.TenantId == tenantId && (u.Role == "TenantAdmin" || u.Role == "SystemAdmin" || u.Role == "Maintenance"))
                    .ToListAsync();

                var notifications = new List<Notification>();
                var emailTasks = new List<Task>();
                var smsTasks = new List<Task>();

                var alertMessage = $"Maintenance alert for vehicle {vehicle.VehicleNumber}: {maintenance.Description}. " +
                                 $"Type: {maintenance.MaintenanceType}, Status: {(maintenance.IsCompleted ? "Completed" : "Pending")}";

                foreach (var manager in managers)
                {
                    var notification = new Notification
                    {
                        Type = "MaintenanceAlert",
                        Title = $"Maintenance Alert - Vehicle {vehicle.VehicleNumber}",
                        Message = alertMessage,
                        Priority = maintenance.MaintenanceType.ToLower().Contains("emergency") ? NotificationPriority.Critical : NotificationPriority.High,
                        UserId = manager.Id,
                        RelatedEntityId = maintenance.Id,
                        RelatedEntityType = "MaintenanceRecord",
                        Channel = NotificationChannel.InApp,
                        TenantId = int.Parse(tenantId),
                        CreatedBy = "System"
                    };

                    notifications.Add(notification);
                    emailTasks.Add(_emailService.SendEmailAsync(manager.Email, notification.Title, notification.Message));
                }

                if (vehicle.AssignedDriver != null)
                {
                    var driverNotification = new Notification
                    {
                        Type = "MaintenanceAlert",
                        Title = $"Maintenance Alert - Your Vehicle {vehicle.VehicleNumber}",
                        Message = alertMessage,
                        Priority = maintenance.MaintenanceType.ToLower().Contains("emergency") ? NotificationPriority.Critical : NotificationPriority.High,
                        UserId = vehicle.AssignedDriver.Id,
                        RelatedEntityId = maintenance.Id,
                        RelatedEntityType = "MaintenanceRecord",
                        Channel = NotificationChannel.InApp,
                        TenantId = int.Parse(tenantId),
                        CreatedBy = "System"
                    };

                    notifications.Add(driverNotification);
                    emailTasks.Add(_emailService.SendEmailAsync(vehicle.AssignedDriver.Email, driverNotification.Title, driverNotification.Message));
                    smsTasks.Add(_smsService.SendSmsAsync(vehicle.AssignedDriver.Phone, alertMessage));
                }

                _context.Notifications.AddRange(notifications);
                await _context.SaveChangesAsync();

                await Task.WhenAll(emailTasks.Concat(smsTasks));

                _logger.LogInformation("Successfully sent maintenance alert for vehicle {VehicleId} to {Count} recipients", 
                    maintenance.VehicleId, notifications.Count);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending maintenance alert for vehicle {VehicleId}", maintenance.VehicleId);
                return Result<bool>.Failure($"Failed to send maintenance alert: {ex.Message}");
            }
        }

        public async Task<Result<bool>> SendTripLocationUpdateAsync(string tripId, double latitude, double longitude)
        {
            try
            {
                _logger.LogInformation("Sending trip location update for trip {TripId}", tripId);

                var trip = await _context.Trips
                    .Include(t => t.Route)
                    .ThenInclude(r => r.Students)
                    .FirstOrDefaultAsync(t => t.Id.ToString() == tripId && !t.IsDeleted);

                if (trip == null)
                {
                    return Result<bool>.Failure("Trip not found");
                }

                var students = trip.Route?.Students?.Where(s => !s.IsDeleted).ToList() ?? new List<Student>();
                var notifications = new List<Notification>();
                var smsTasks = new List<Task>();

                var locationMessage = $"Bus location update: Route {trip.Route?.RouteNumber} is currently at coordinates {latitude:F6}, {longitude:F6}";

                foreach (var student in students)
                {
                    var notification = new Notification
                    {
                        Type = "LocationUpdate",
                        Title = "Bus Location Update",
                        Message = locationMessage,
                        Priority = NotificationPriority.Normal,
                        UserId = student.Id,
                        RelatedEntityId = trip.Id,
                        RelatedEntityType = "Trip",
                        Channel = NotificationChannel.InApp,
                        TenantId = student.TenantId,
                        CreatedBy = "System",
                        Metadata = $"{{\"latitude\":{latitude},\"longitude\":{longitude}}}"
                    };

                    notifications.Add(notification);

                    if (!string.IsNullOrEmpty(student.ParentPhone))
                    {
                        smsTasks.Add(_smsService.SendSmsAsync(student.ParentPhone, locationMessage));
                    }
                }

                _context.Notifications.AddRange(notifications);
                await _context.SaveChangesAsync();

                await Task.WhenAll(smsTasks);

                _logger.LogInformation("Successfully sent location update for trip {TripId} to {Count} students", tripId, students.Count);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending trip location update for trip {TripId}", tripId);
                return Result<bool>.Failure($"Failed to send trip location update: {ex.Message}");
            }
        }

        public async Task<Result<bool>> SendStudentPickupNotificationAsync(string tenantId, int studentId, string status)
        {
            try
            {
                _logger.LogInformation("Sending pickup notification for student {StudentId} with status {Status}", studentId, status);

                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.Id == studentId && s.TenantId.ToString() == tenantId && !s.IsDeleted);

                if (student == null)
                {
                    return Result<bool>.Failure("Student not found");
                }

                var notification = new Notification
                {
                    Type = "PickupNotification",
                    Title = "Student Pickup Update",
                    Message = $"Student {student.FullName.FirstName} {student.FullName.LastName} has been {status}",
                    Priority = NotificationPriority.High,
                    UserId = student.Id,
                    RelatedEntityId = studentId,
                    RelatedEntityType = "Student",
                    Channel = NotificationChannel.InApp,
                    TenantId = int.Parse(tenantId),
                    CreatedBy = "System"
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                var tasks = new List<Task>();

                if (!string.IsNullOrEmpty(student.ParentEmail))
                {
                    tasks.Add(_emailService.SendEmailAsync(
                        student.ParentEmail,
                        notification.Title,
                        notification.Message));
                }

                if (!string.IsNullOrEmpty(student.ParentPhone))
                {
                    tasks.Add(_smsService.SendSmsAsync(student.ParentPhone, notification.Message));
                }

                await Task.WhenAll(tasks);

                _logger.LogInformation("Successfully sent pickup notification for student {StudentId}", studentId);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending pickup notification for student {StudentId}", studentId);
                return Result<bool>.Failure($"Failed to send pickup notification: {ex.Message}");
            }
        }

        public async Task<Result<List<NotificationDto>>> GetNotificationsAsync(string tenantId, int userId, bool unreadOnly = false)
        {
            try
            {
                var query = _context.Notifications
                    .Where(n => n.TenantId.ToString() == tenantId && n.UserId == userId && !n.IsDeleted);

                if (unreadOnly)
                {
                    query = query.Where(n => !n.IsRead);
                }

                var notifications = await query
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(50)
                    .Select(n => new NotificationDto
                    {
                        Id = n.Id,
                        Type = n.Type,
                        Title = n.Title,
                        Message = n.Message,
                        Priority = n.Priority,
                        IsRead = n.IsRead,
                        ReadAt = n.ReadAt,
                        UserId = n.UserId,
                        RelatedEntityId = n.RelatedEntityId,
                        RelatedEntityType = n.RelatedEntityType,
                        Channel = n.Channel,
                        EmailSent = n.EmailSent,
                        SmsSent = n.SmsSent,
                        EmailSentAt = n.EmailSentAt,
                        SmsSentAt = n.SmsSentAt,
                        EmailError = n.EmailError,
                        SmsError = n.SmsError,
                        Metadata = n.Metadata,
                        CreatedAt = n.CreatedAt,
                        UpdatedAt = n.UpdatedAt,
                        CreatedBy = n.CreatedBy,
                        UpdatedBy = n.UpdatedBy,
                        TenantId = n.TenantId
                    })
                    .ToListAsync();

                return Result<List<NotificationDto>>.Success(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for user {UserId} in tenant {TenantId}", userId, tenantId);
                return Result<List<NotificationDto>>.Failure($"Failed to get notifications: {ex.Message}");
            }
        }

        public async Task<Result<bool>> MarkNotificationAsReadAsync(int notificationId, string readBy)
        {
            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId && !n.IsDeleted);

                if (notification == null)
                {
                    return Result<bool>.Failure("Notification not found");
                }

                notification.MarkAsRead(readBy);
                await _context.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read", notificationId);
                return Result<bool>.Failure($"Failed to mark notification as read: {ex.Message}");
            }
        }
    }
}
