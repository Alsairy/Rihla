using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rihla.Infrastructure.Data;
using Rihla.Core.Enums;

namespace Rihla.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
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

                // Trip statistics (mock data for now)
                var todaysTrips = 64; // Mock data
                var completedTrips = 45; // Mock data
                var inProgressTrips = 12; // Mock data
                var scheduledTrips = 7; // Mock data

                // Attendance statistics (mock data)
                var attendanceRate = 94.5; // Mock data

                var statistics = new
                {
                    Students = new
                    {
                        Total = totalStudents,
                        Active = activeStudents,
                        ChangeFromLastMonth = totalStudents > 0 ? "+5.2%" : "0%"
                    },
                    Drivers = new
                    {
                        Total = totalDrivers,
                        Active = activeDrivers,
                        ChangeFromLastMonth = totalDrivers > 0 ? "+2.1%" : "0%"
                    },
                    Vehicles = new
                    {
                        Total = totalVehicles,
                        Active = activeVehicles,
                        Maintenance = maintenanceVehicles,
                        OutOfService = outOfServiceVehicles,
                        ChangeFromLastMonth = "0%"
                    },
                    Routes = new
                    {
                        Total = totalRoutes,
                        Active = activeRoutes,
                        ChangeFromLastMonth = totalRoutes > 0 ? "-1.2%" : "0%"
                    },
                    Trips = new
                    {
                        Today = todaysTrips,
                        Completed = completedTrips,
                        InProgress = inProgressTrips,
                        Scheduled = scheduledTrips,
                        ChangeFromLastMonth = "+8.3%"
                    },
                    Attendance = new
                    {
                        Rate = attendanceRate,
                        ChangeFromLastMonth = "+1.5%"
                    }
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
                // Mock alerts data - replace with real alerts from database
                var alerts = new[]
                {
                    new
                    {
                        Id = 1,
                        Type = "warning",
                        Title = "Vehicle Maintenance",
                        Message = "Vehicle BUS-001 maintenance due",
                        Time = "2 hours ago",
                        IsRead = false
                    },
                    new
                    {
                        Id = 2,
                        Type = "info",
                        Title = "New Registration",
                        Message = "New student registration pending",
                        Time = "4 hours ago",
                        IsRead = false
                    },
                    new
                    {
                        Id = 3,
                        Type = "error",
                        Title = "Route Delay",
                        Message = "Route 5 delayed by 15 minutes",
                        Time = "6 hours ago",
                        IsRead = true
                    }
                };

                return Ok(alerts);
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
                // Mock activity data - replace with real activity from database
                var activities = new[]
                {
                    new
                    {
                        Id = 1,
                        Type = "trip_completed",
                        Title = "Trip completed",
                        Description = "Route 3 - Morning",
                        Time = "10 minutes ago",
                        Icon = "checkmark-circle"
                    },
                    new
                    {
                        Id = 2,
                        Type = "student_checkin",
                        Title = "Student checked in",
                        Description = "John Doe - Bus 12",
                        Time = "15 minutes ago",
                        Icon = "person"
                    },
                    new
                    {
                        Id = 3,
                        Type = "vehicle_inspection",
                        Title = "Vehicle inspection",
                        Description = "BUS-005 passed inspection",
                        Time = "1 hour ago",
                        Icon = "shield-checkmark"
                    }
                };

                return Ok(activities);
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
                // Mock weekly attendance data - replace with real data
                var weeklyData = new[]
                {
                    new { Day = "Mon", Attendance = 95.2 },
                    new { Day = "Tue", Attendance = 94.8 },
                    new { Day = "Wed", Attendance = 96.1 },
                    new { Day = "Thu", Attendance = 93.7 },
                    new { Day = "Fri", Attendance = 97.3 },
                    new { Day = "Sat", Attendance = 89.5 },
                    new { Day = "Sun", Attendance = 87.2 }
                };

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
                // Mock daily trip schedule - replace with real data
                var scheduleData = new[]
                {
                    new { Hour = "06:00", Trips = 8 },
                    new { Hour = "07:00", Trips = 15 },
                    new { Hour = "08:00", Trips = 12 },
                    new { Hour = "09:00", Trips = 3 },
                    new { Hour = "14:00", Trips = 5 },
                    new { Hour = "15:00", Trips = 18 },
                    new { Hour = "16:00", Trips = 14 },
                    new { Hour = "17:00", Trips = 9 }
                };

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

