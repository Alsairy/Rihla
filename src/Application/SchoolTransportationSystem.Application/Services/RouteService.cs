using SchoolTransportationSystem.Application.DTOs;
using SchoolTransportationSystem.Core.Entities;
using SchoolTransportationSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SchoolTransportationSystem.Application.Services
{
    public class RouteService
    {
        private readonly ApplicationDbContext _context;

        public RouteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RouteDto>> GetAllRoutesAsync()
        {
            var routes = await _context.Routes
                .Where(r => !r.IsDeleted)
                .Include(r => r.AssignedVehicle)
                .Include(r => r.AssignedDriver)
                .ToListAsync();

            return routes.Select(r => new RouteDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Status = r.Status.ToString(),
                EstimatedDuration = r.EstimatedDuration,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                AssignedVehicleId = r.AssignedVehicleId,
                AssignedVehicleName = r.AssignedVehicle?.PlateNumber ?? "Not Assigned",
                AssignedDriverId = r.AssignedDriverId,
                AssignedDriverName = r.AssignedDriver != null ? $"{r.AssignedDriver.FirstName} {r.AssignedDriver.LastName}" : "Not Assigned"
            });
        }

        public async Task<RouteDto?> GetRouteByIdAsync(int id)
        {
            var route = await _context.Routes
                .Where(r => r.Id == id && !r.IsDeleted)
                .Include(r => r.AssignedVehicle)
                .Include(r => r.AssignedDriver)
                .FirstOrDefaultAsync();

            if (route == null) return null;

            return new RouteDto
            {
                Id = route.Id,
                Name = route.Name,
                Description = route.Description,
                Status = route.Status.ToString(),
                EstimatedDuration = route.EstimatedDuration,
                CreatedAt = route.CreatedAt,
                UpdatedAt = route.UpdatedAt,
                AssignedVehicleId = route.AssignedVehicleId,
                AssignedVehicleName = route.AssignedVehicle?.PlateNumber ?? "Not Assigned",
                AssignedDriverId = route.AssignedDriverId,
                AssignedDriverName = route.AssignedDriver != null ? $"{route.AssignedDriver.FirstName} {route.AssignedDriver.LastName}" : "Not Assigned"
            };
        }

        public async Task<RouteDto> CreateRouteAsync(CreateRouteDto createRouteDto)
        {
            var route = new Route
            {
                Name = createRouteDto.Name,
                Description = createRouteDto.Description,
                Status = Enum.Parse<RouteStatus>(createRouteDto.Status),
                EstimatedDuration = createRouteDto.EstimatedDuration,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

            return await GetRouteByIdAsync(route.Id) ?? throw new InvalidOperationException("Failed to create route");
        }

        public async Task<RouteDto?> UpdateRouteAsync(int id, UpdateRouteDto updateRouteDto)
        {
            var route = await _context.Routes
                .Where(r => r.Id == id && !r.IsDeleted)
                .FirstOrDefaultAsync();

            if (route == null) return null;

            route.Name = updateRouteDto.Name;
            route.Description = updateRouteDto.Description;
            route.Status = Enum.Parse<RouteStatus>(updateRouteDto.Status);
            route.EstimatedDuration = updateRouteDto.EstimatedDuration;
            route.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetRouteByIdAsync(route.Id);
        }

        public async Task<bool> DeleteRouteAsync(int id)
        {
            var route = await _context.Routes
                .Where(r => r.Id == id && !r.IsDeleted)
                .FirstOrDefaultAsync();

            if (route == null) return false;

            route.IsDeleted = true;
            route.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<object> GetRouteStatisticsAsync()
        {
            var totalRoutes = await _context.Routes.Where(r => !r.IsDeleted).CountAsync();
            var activeRoutes = await _context.Routes.Where(r => !r.IsDeleted && r.Status == RouteStatus.Active).CountAsync();
            var inactiveRoutes = await _context.Routes.Where(r => !r.IsDeleted && r.Status == RouteStatus.Inactive).CountAsync();
            var routesWithVehicles = await _context.Routes.Where(r => !r.IsDeleted && r.AssignedVehicleId != null).CountAsync();
            var routesWithDrivers = await _context.Routes.Where(r => !r.IsDeleted && r.AssignedDriverId != null).CountAsync();

            return new
            {
                TotalRoutes = totalRoutes,
                ActiveRoutes = activeRoutes,
                InactiveRoutes = inactiveRoutes,
                RoutesWithVehicles = routesWithVehicles,
                RoutesWithDrivers = routesWithDrivers,
                AssignmentCompletionRate = totalRoutes > 0 ? Math.Round((double)(routesWithVehicles + routesWithDrivers) / (totalRoutes * 2) * 100, 2) : 0
            };
        }

        public async Task<IEnumerable<RouteDto>> SearchRoutesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllRoutesAsync();
            }

            var routes = await _context.Routes
                .Where(r => !r.IsDeleted && 
                           (r.Name.Contains(searchTerm) || 
                            r.Description.Contains(searchTerm)))
                .Include(r => r.AssignedVehicle)
                .Include(r => r.AssignedDriver)
                .ToListAsync();

            return routes.Select(r => new RouteDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Status = r.Status.ToString(),
                EstimatedDuration = r.EstimatedDuration,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                AssignedVehicleId = r.AssignedVehicleId,
                AssignedVehicleName = r.AssignedVehicle?.PlateNumber ?? "Not Assigned",
                AssignedDriverId = r.AssignedDriverId,
                AssignedDriverName = r.AssignedDriver != null ? $"{r.AssignedDriver.FirstName} {r.AssignedDriver.LastName}" : "Not Assigned"
            });
        }

        public async Task<bool> AssignVehicleToRouteAsync(int routeId, int vehicleId)
        {
            var route = await _context.Routes
                .Where(r => r.Id == routeId && !r.IsDeleted)
                .FirstOrDefaultAsync();

            if (route == null) return false;

            var vehicle = await _context.Vehicles
                .Where(v => v.Id == vehicleId && !v.IsDeleted)
                .FirstOrDefaultAsync();

            if (vehicle == null) return false;

            route.AssignedVehicleId = vehicleId;
            route.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignDriverToRouteAsync(int routeId, int driverId)
        {
            var route = await _context.Routes
                .Where(r => r.Id == routeId && !r.IsDeleted)
                .FirstOrDefaultAsync();

            if (route == null) return false;

            var driver = await _context.Drivers
                .Where(d => d.Id == driverId && !d.IsDeleted)
                .FirstOrDefaultAsync();

            if (driver == null) return false;

            route.AssignedDriverId = driverId;
            route.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<object>> GetRouteStudentsAsync(int routeId)
        {
            var students = await _context.Students
                .Where(s => s.RouteId == routeId && !s.IsDeleted)
                .Select(s => new
                {
                    s.Id,
                    s.FirstName,
                    s.LastName,
                    s.StudentId,
                    s.Grade,
                    s.Status
                })
                .ToListAsync();

            return students;
        }

        public async Task<object> OptimizeRouteAsync(int routeId)
        {
            // This is a placeholder for route optimization logic
            // In a real implementation, this would use algorithms like:
            // - Traveling Salesman Problem (TSP) solvers
            // - Google Maps API for real-time traffic
            // - Machine learning for historical data analysis

            var route = await GetRouteByIdAsync(routeId);
            if (route == null) return new { message = "Route not found" };

            var students = await GetRouteStudentsAsync(routeId);
            var studentCount = students.Count();

            // Simulate optimization results
            var optimizedDuration = route.EstimatedDuration.Subtract(TimeSpan.FromMinutes(5));
            var fuelSavings = Math.Round(studentCount * 0.5, 2); // Simulated fuel savings

            return new
            {
                RouteId = routeId,
                RouteName = route.Name,
                OriginalDuration = route.EstimatedDuration,
                OptimizedDuration = optimizedDuration,
                TimeSaved = TimeSpan.FromMinutes(5),
                EstimatedFuelSavings = $"{fuelSavings} liters",
                StudentCount = studentCount,
                OptimizationScore = 85.5, // Simulated score
                Recommendations = new[]
                {
                    "Adjust pickup order to reduce backtracking",
                    "Consider traffic patterns during peak hours",
                    "Optimize stop locations for efficiency"
                }
            };
        }
    }
}

