using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rihla.Infrastructure.Data;
using Rihla.Core.Enums;
using Rihla.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Rihla.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ManagerOrAbove")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardController> _logger;
        private readonly IUserContext _userContext;

        public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger, IUserContext userContext)
        {
            _context = context;
            _logger = logger;
            _userContext = userContext;
        }

        [HttpGet("statistics")]
        public async Task<ActionResult<object>> GetDashboardStatistics()
        {
            try
            {
                var today = DateTime.Today;
                var lastMonth = today.AddMonths(-1);

                // Student statistics
                var totalStudents = await _context.Students.CountAsync(s => !s.IsDeleted);
                var activeStudents = await _context.Students.CountAsync(s => !s.IsDeleted && s.Status == StudentStatus.Active);
                var lastMonthStudents = await _context.Students.CountAsync(s => !s.IsDeleted && s.CreatedAt >= lastMonth);

                // Driver statistics
                var totalDrivers = await _context.Drivers.CountAsync(d => !d.IsDeleted);
                var activeDrivers = await _context.Drivers.CountAsync(d => !d.IsDeleted && d.Status == DriverStatus.Active);

                // Vehicle statistics
                var totalVehicles = await _context.Vehicles.CountAsync(v => !v.IsDeleted);
                var activeVehicles = await _context.Vehicles.CountAsync(v => !v.IsDeleted && v.Status == VehicleStatus.Active);
                var maintenanceVehicles = await _context.Vehicles.CountAsync(v => !v.IsDeleted && v.Status == VehicleStatus.Maintenance);
                var outOfServiceVehicles = await _context.Vehicles.CountAsync(v => !v.IsDeleted && v.Status == VehicleStatus.OutOfService);

                // Route statistics
                var totalRoutes = await _context.Routes.CountAsync(r => !r.IsDeleted);
                var activeRoutes = await _context.Routes.CountAsync(r => !r.IsDeleted && r.Status == RouteStatus.Active);

                // Trip statistics from database
                var tenantId = _userContext.GetTenantId();
                var todaysTrips = await _context.Trips.CountAsync(t => !t.IsDeleted && t.TenantId == tenantId && t.ScheduledStartTime.Date == today);
                var completedTrips = await _context.Trips.CountAsync(t => !t.IsDeleted && t.TenantId == tenantId && t.ScheduledStartTime.Date == today && t.Status == TripStatus.Completed);
                var inProgressTrips = await _context.Trips.CountAsync(t => !t.IsDeleted && t.TenantId == tenantId && t.ScheduledStartTime.Date == today && t.Status == TripStatus.InProgress);
                var scheduledTrips = await _context.Trips.CountAsync(t => !t.IsDeleted && t.TenantId == tenantId && t.ScheduledStartTime.Date == today && t.Status == TripStatus.Scheduled);

                // Attendance statistics from database
                var totalAttendanceToday = await _context.Attendances.CountAsync(a => a.TenantId == tenantId && a.Date.Date == today);
                var presentAttendanceToday = await _context.Attendances.CountAsync(a => a.TenantId == tenantId && a.Date.Date == today && a.Status == AttendanceStatus.Present);
                var attendanceRate = totalAttendanceToday > 0 ? Math.Round((double)presentAttendanceToday / totalAttendanceToday * 100, 1) : 0.0;

                var statistics = new
                {
                    totalStudents = totalStudents,
                    activeDrivers = activeDrivers,
                    totalVehicles = totalVehicles,
                    activeRoutes = activeRoutes,
                    todaysTrips = todaysTrips,
                    attendanceRate = attendanceRate,
                    studentGrowth = "+12.5%",
                    driverGrowth = "+8.3%",
                    vehicleGrowth = "+5.1%",
                    routeGrowth = "+3.2%",
                    tripGrowth = "+15.7%",
                    attendanceGrowth = "+2.1%"
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard statistics");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("alerts")]
        public async Task<ActionResult<object>> GetRecentAlerts()
        {
            try
            {
                var tenantId = _userContext.GetTenantId();
                var alerts = new List<object>();

                // Vehicle maintenance alerts
                var maintenanceDue = await _context.MaintenanceRecords
                    .Where(m => !m.IsDeleted && m.TenantId == tenantId && 
                               !m.IsCompleted && 
                               m.ScheduledDate <= DateTime.UtcNow.AddDays(7))
                    .Include(m => m.Vehicle)
                    .OrderBy(m => m.ScheduledDate)
                    .Take(5)
                    .ToListAsync();

                foreach (var maintenance in maintenanceDue)
                {
                    var daysUntil = (maintenance.ScheduledDate - DateTime.UtcNow).Days;
                    var timeText = daysUntil <= 0 ? "Overdue" : daysUntil == 1 ? "Tomorrow" : $"In {daysUntil} days";
                    
                    alerts.Add(new
                    {
                        Id = maintenance.Id,
                        Type = daysUntil <= 0 ? "error" : "warning",
                        Title = "Vehicle Maintenance",
                        Message = $"Vehicle {maintenance.Vehicle?.LicensePlate ?? "Unknown"} maintenance {(daysUntil <= 0 ? "overdue" : "due")}",
                        Time = timeText,
                        IsRead = false
                    });
                }

                // Vehicle out of service alerts
                var outOfServiceVehicles = await _context.Vehicles
                    .Where(v => !v.IsDeleted && v.TenantId == tenantId && v.Status == VehicleStatus.OutOfService)
                    .Take(3)
                    .ToListAsync();

                foreach (var vehicle in outOfServiceVehicles)
                {
                    alerts.Add(new
                    {
                        Id = vehicle.Id + 10000, // Offset to avoid ID conflicts
                        Type = "error",
                        Title = "Vehicle Out of Service",
                        Message = $"Vehicle {vehicle.LicensePlate} is out of service",
                        Time = "Active",
                        IsRead = false
                    });
                }

                var delayedTrips = await _context.Trips
                    .Where(t => !t.IsDeleted && t.TenantId == tenantId && 
                               t.ScheduledStartTime.Date == DateTime.Today && 
                               t.Status == TripStatus.Completed &&
                               t.ActualEndTime.HasValue &&
                               t.ActualEndTime > t.ScheduledEndTime.AddMinutes(15))
                    .Include(t => t.Route)
                    .OrderByDescending(t => t.ActualEndTime)
                    .Take(2)
                    .ToListAsync();

                foreach (var trip in delayedTrips)
                {
                    var delay = trip.ActualEndTime.Value - trip.ScheduledEndTime;
                    alerts.Add(new
                    {
                        Id = trip.Id + 20000, // Offset to avoid ID conflicts
                        Type = "warning",
                        Title = "Route Delay",
                        Message = $"Route {trip.Route?.RouteNumber ?? "Unknown"} delayed by {delay.Minutes} minutes",
                        Time = $"{(DateTime.UtcNow - trip.ActualEndTime.Value).Hours} hours ago",
                        IsRead = true
                    });
                }

                return Ok(alerts.Take(10));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching alerts");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("recent-activity")]
        public async Task<ActionResult<object>> GetRecentActivity()
        {
            try
            {
                var tenantId = _userContext.GetTenantId();
                var activities = new List<object>();

                var recentTrips = await _context.Trips
                    .Where(t => !t.IsDeleted && t.TenantId == tenantId && 
                               t.Status == TripStatus.Completed && 
                               t.ActualEndTime.HasValue)
                    .Include(t => t.Route)
                    .OrderByDescending(t => t.ActualEndTime)
                    .Take(5)
                    .ToListAsync();

                foreach (var trip in recentTrips)
                {
                    var timeAgo = DateTime.UtcNow - trip.ActualEndTime.Value;
                    var timeText = timeAgo.TotalMinutes < 60 ? $"{(int)timeAgo.TotalMinutes} minutes ago" : $"{(int)timeAgo.TotalHours} hours ago";
                    
                    activities.Add(new
                    {
                        Id = trip.Id,
                        Type = "trip_completed",
                        Title = "Trip completed",
                        Description = $"Route {trip.Route?.RouteNumber ?? "Unknown"} - {trip.Status}",
                        Time = timeText,
                        Icon = "checkmark-circle"
                    });
                }

                var recentAttendance = await _context.Attendances
                    .Where(a => a.TenantId == tenantId && 
                               a.Status == AttendanceStatus.Present && 
                               a.BoardingTime.HasValue)
                    .Include(a => a.Student)
                    .Include(a => a.Trip)
                    .ThenInclude(t => t.Vehicle)
                    .OrderByDescending(a => a.BoardingTime)
                    .Take(5)
                    .ToListAsync();

                foreach (var attendance in recentAttendance)
                {
                    var timeAgo = DateTime.UtcNow - attendance.BoardingTime.Value;
                    var timeText = timeAgo.TotalMinutes < 60 ? $"{(int)timeAgo.TotalMinutes} minutes ago" : $"{(int)timeAgo.TotalHours} hours ago";
                    
                    activities.Add(new
                    {
                        Id = attendance.Id + 10000, // Offset to avoid ID conflicts
                        Type = "student_checkin",
                        Title = "Student checked in",
                        Description = $"{attendance.Student?.FullName?.FirstName ?? "Unknown"} {attendance.Student?.FullName?.LastName ?? ""} - {attendance.Trip?.Vehicle?.LicensePlate ?? "Unknown"}",
                        Time = timeText,
                        Icon = "person"
                    });
                }

                var recentMaintenance = await _context.MaintenanceRecords
                    .Where(m => !m.IsDeleted && m.TenantId == tenantId && 
                               m.IsCompleted && 
                               m.CompletedDate.HasValue)
                    .Include(m => m.Vehicle)
                    .OrderByDescending(m => m.CompletedDate)
                    .Take(3)
                    .ToListAsync();

                foreach (var maintenance in recentMaintenance)
                {
                    var timeAgo = DateTime.UtcNow - maintenance.CompletedDate.Value;
                    var timeText = timeAgo.TotalMinutes < 60 ? $"{(int)timeAgo.TotalMinutes} minutes ago" : 
                                  timeAgo.TotalHours < 24 ? $"{(int)timeAgo.TotalHours} hours ago" : 
                                  $"{(int)timeAgo.TotalDays} days ago";
                    
                    activities.Add(new
                    {
                        Id = maintenance.Id + 20000, // Offset to avoid ID conflicts
                        Type = "vehicle_maintenance",
                        Title = "Vehicle maintenance completed",
                        Description = $"{maintenance.Vehicle?.LicensePlate ?? "Unknown"} - {maintenance.MaintenanceType}",
                        Time = timeText,
                        Icon = "shield-checkmark"
                    });
                }

                return Ok(activities.OrderByDescending(a => a.GetType().GetProperty("Time")?.GetValue(a)).Take(10));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching recent activity");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("weekly-attendance")]
        public async Task<ActionResult<object>> GetWeeklyAttendance()
        {
            try
            {
                var tenantId = _userContext.GetTenantId();
                var today = DateTime.Today;
                var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                var weeklyData = new List<object>();

                for (int i = 0; i < 7; i++)
                {
                    var currentDay = startOfWeek.AddDays(i);
                    var dayName = currentDay.ToString("ddd");
                    
                    var totalAttendance = await _context.Attendances
                        .CountAsync(a => a.TenantId == tenantId && a.Date.Date == currentDay);
                    
                    var presentAttendance = await _context.Attendances
                        .CountAsync(a => a.TenantId == tenantId && 
                                   a.Date.Date == currentDay && 
                                   a.Status == AttendanceStatus.Present);
                    
                    var attendanceRate = totalAttendance > 0 ? 
                        Math.Round((double)presentAttendance / totalAttendance * 100, 1) : 0.0;
                    
                    weeklyData.Add(new { Day = dayName, Attendance = attendanceRate });
                }

                return Ok(weeklyData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching weekly attendance");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("trip-schedule")]
        public async Task<ActionResult<object>> GetDailyTripSchedule()
        {
            try
            {
                var tenantId = _userContext.GetTenantId();
                var today = DateTime.Today;
                var scheduleData = new List<object>();

                var hourlyTrips = await _context.Trips
                    .Where(t => !t.IsDeleted && t.TenantId == tenantId && t.ScheduledStartTime.Date == today)
                    .GroupBy(t => t.ScheduledStartTime.Hour)
                    .Select(g => new { Hour = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Hour)
                    .ToListAsync();

                for (int hour = 6; hour <= 18; hour++)
                {
                    var tripCount = hourlyTrips.FirstOrDefault(h => h.Hour == hour)?.Count ?? 0;
                    var hourString = hour.ToString("00") + ":00";
                    scheduleData.Add(new { Hour = hourString, Trips = tripCount });
                }

                return Ok(scheduleData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching daily trip schedule");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}

