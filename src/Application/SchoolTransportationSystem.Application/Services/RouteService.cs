using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolTransportationSystem.Application.DTOs;
using SchoolTransportationSystem.Application.Interfaces;
using SchoolTransportationSystem.Core.Common;
using SchoolTransportationSystem.Core.Entities;
using SchoolTransportationSystem.Core.Enums;
using SchoolTransportationSystem.Infrastructure.Data;

namespace SchoolTransportationSystem.Application.Services
{
    public class RouteService : IRouteService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RouteService> _logger;

        public RouteService(ApplicationDbContext context, ILogger<RouteService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<RouteDto>> GetByIdAsync(int id, string tenantId)
        {
            try
            {
                var route = await _context.Routes
                    .Include(r => r.AssignedVehicle)
                    .Include(r => r.AssignedDriver)
                    .Include(r => r.RouteStops)
                    .Include(r => r.Students)
                    .Where(r => r.Id == id && r.TenantId == int.Parse(tenantId) && !r.IsDeleted)
                    .FirstOrDefaultAsync();

                if (route == null)
                {
                    return Result<RouteDto>.Failure("Route not found");
                }

                var routeDto = MapToDto(route);
                return Result<RouteDto>.Success(routeDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting route by ID {RouteId}", id);
                return Result<RouteDto>.Failure("An error occurred while retrieving the route");
            }
        }

        public async Task<Result<PagedResult<RouteDto>>> GetAllAsync(RouteSearchDto searchDto, string tenantId)
        {
            try
            {
                var query = _context.Routes
                    .Include(r => r.AssignedVehicle)
                    .Include(r => r.AssignedDriver)
                    .Where(r => r.TenantId == int.Parse(tenantId) && !r.IsDeleted);

                if (!string.IsNullOrEmpty(searchDto.RouteNumber))
                {
                    query = query.Where(r => r.RouteNumber.Contains(searchDto.RouteNumber));
                }

                if (!string.IsNullOrEmpty(searchDto.Name))
                {
                    query = query.Where(r => r.Name.Contains(searchDto.Name));
                }


                if (searchDto.Status.HasValue)
                {
                    query = query.Where(r => r.Status == searchDto.Status.Value);
                }

                if (searchDto.VehicleId.HasValue)
                {
                    query = query.Where(r => r.AssignedVehicleId == searchDto.VehicleId.Value);
                }

                if (searchDto.DriverId.HasValue)
                {
                    query = query.Where(r => r.AssignedDriverId == searchDto.DriverId.Value);
                }


                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize);

                var routes = await query
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                var routeDtos = routes.Select(MapToDto).ToList();

                var pagedResult = new PagedResult<RouteDto>
                {
                    Items = routeDtos,
                    TotalCount = totalCount,
                    Page = searchDto.Page,
                    PageSize = searchDto.PageSize,
                    TotalPages = totalPages
                };

                return Result<PagedResult<RouteDto>>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting routes");
                return Result<PagedResult<RouteDto>>.Failure("An error occurred while retrieving routes");
            }
        }

        public async Task<Result<RouteDto>> CreateAsync(CreateRouteDto createDto, string tenantId)
        {
            try
            {
                var existingRoute = await _context.Routes
                    .Where(r => r.RouteNumber == createDto.RouteNumber && r.TenantId == int.Parse(tenantId) && !r.IsDeleted)
                    .FirstOrDefaultAsync();

                if (existingRoute != null)
                {
                    return Result<RouteDto>.Failure("Route number already exists");
                }

                var route = new Route
                {
                    RouteNumber = createDto.RouteNumber,
                    Name = createDto.Name,
                    Description = createDto.Description ?? string.Empty,
                    Status = createDto.Status,
                    StartTime = createDto.StartTime,
                    EndTime = createDto.EndTime,
                    Distance = createDto.EstimatedDistance,
                    EstimatedDuration = (int)createDto.EstimatedDuration.TotalMinutes,
                    StartLocation = string.Empty,
                    EndLocation = string.Empty,
                    Notes = createDto.Notes,
                    TenantId = int.Parse(tenantId),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Routes.Add(route);
                await _context.SaveChangesAsync();

                var routeDto = MapToDto(route);
                return Result<RouteDto>.Success(routeDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating route");
                return Result<RouteDto>.Failure("An error occurred while creating the route");
            }
        }

        public async Task<Result<RouteDto>> UpdateAsync(int id, UpdateRouteDto updateDto, string tenantId)
        {
            try
            {
                var route = await _context.Routes
                    .Where(r => r.Id == id && r.TenantId == int.Parse(tenantId) && !r.IsDeleted)
                    .FirstOrDefaultAsync();

                if (route == null)
                {
                    return Result<RouteDto>.Failure("Route not found");
                }

                var existingRoute = await _context.Routes
                    .Where(r => r.RouteNumber == updateDto.RouteNumber && r.Id != id && r.TenantId == int.Parse(tenantId) && !r.IsDeleted)
                    .FirstOrDefaultAsync();

                if (existingRoute != null)
                {
                    return Result<RouteDto>.Failure("Route number already exists");
                }

                route.RouteNumber = updateDto.RouteNumber;
                route.Name = updateDto.Name;
                route.Description = updateDto.Description ?? string.Empty;
                route.Status = updateDto.Status;
                route.StartTime = updateDto.StartTime;
                route.EndTime = updateDto.EndTime;
                route.Distance = updateDto.EstimatedDistance;
                route.EstimatedDuration = (int)updateDto.EstimatedDuration.TotalMinutes;
                route.Notes = updateDto.Notes;
                route.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var routeDto = MapToDto(route);
                return Result<RouteDto>.Success(routeDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating route {RouteId}", id);
                return Result<RouteDto>.Failure("An error occurred while updating the route");
            }
        }

        public async Task<Result<bool>> DeleteAsync(int id, string tenantId)
        {
            try
            {
                var route = await _context.Routes
                    .Where(r => r.Id == id && r.TenantId == int.Parse(tenantId) && !r.IsDeleted)
                    .FirstOrDefaultAsync();

                if (route == null)
                {
                    return Result<bool>.Failure("Route not found");
                }

                route.IsDeleted = true;
                route.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting route {RouteId}", id);
                return Result<bool>.Failure("An error occurred while deleting the route");
            }
        }

        public async Task<Result<RouteDto>> GetByRouteNumberAsync(string routeNumber, string tenantId)
        {
            try
            {
                var route = await _context.Routes
                    .Include(r => r.AssignedVehicle)
                    .Include(r => r.AssignedDriver)
                    .Where(r => r.RouteNumber == routeNumber && r.TenantId == int.Parse(tenantId) && !r.IsDeleted)
                    .FirstOrDefaultAsync();

                if (route == null)
                {
                    return Result<RouteDto>.Failure("Route not found");
                }

                var routeDto = MapToDto(route);
                return Result<RouteDto>.Success(routeDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting route by number {RouteNumber}", routeNumber);
                return Result<RouteDto>.Failure("An error occurred while retrieving the route");
            }
        }

        public async Task<Result<List<RouteDto>>> GetActiveRoutesAsync(string tenantId)
        {
            try
            {
                var routes = await _context.Routes
                    .Include(r => r.AssignedVehicle)
                    .Include(r => r.AssignedDriver)
                    .Where(r => r.TenantId == int.Parse(tenantId) && !r.IsDeleted && r.Status == RouteStatus.Active)
                    .ToListAsync();

                var routeDtos = routes.Select(MapToDto).ToList();
                return Result<List<RouteDto>>.Success(routeDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active routes");
                return Result<List<RouteDto>>.Failure("An error occurred while retrieving active routes");
            }
        }

        public async Task<Result<List<StudentDto>>> GetStudentsOnRouteAsync(int routeId, string tenantId)
        {
            try
            {
                var route = await _context.Routes
                    .Where(r => r.Id == routeId && r.TenantId == int.Parse(tenantId) && !r.IsDeleted)
                    .FirstOrDefaultAsync();

                if (route == null)
                {
                    return Result<List<StudentDto>>.Failure("Route not found");
                }

                var students = await _context.Students
                    .Where(s => s.RouteId == routeId && !s.IsDeleted)
                    .ToListAsync();

                var studentDtos = students.Select(s => new StudentDto
                {
                    Id = s.Id,
                    StudentNumber = s.StudentNumber,
                    FirstName = s.FullName.FirstName,
                    LastName = s.FullName.LastName,
                    MiddleName = s.FullName.MiddleName,
                    Grade = s.Grade,
                    Status = s.Status,
                    RouteId = s.RouteId
                }).ToList();

                return Result<List<StudentDto>>.Success(studentDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting students on route {RouteId}", routeId);
                return Result<List<StudentDto>>.Failure("An error occurred while retrieving students on route");
            }
        }

        private RouteDto MapToDto(Route route)
        {
            return new RouteDto
            {
                Id = route.Id,
                RouteNumber = route.RouteNumber,
                Name = route.Name,
                Description = route.Description,
                Status = route.Status,
                StartTime = route.StartTime,
                EndTime = route.EndTime,
                EstimatedDistance = route.Distance,
                EstimatedDuration = TimeSpan.FromMinutes(route.EstimatedDuration),
                Notes = route.Notes,
                IsActive = true,
                CreatedAt = route.CreatedAt,
                UpdatedAt = route.UpdatedAt,
                TenantId = route.TenantId.ToString(),
                AssignedVehicle = route.AssignedVehicle != null ? new VehicleDto
                {
                    Id = route.AssignedVehicle.Id,
                    VehicleNumber = route.AssignedVehicle.VehicleNumber,
                    LicensePlate = route.AssignedVehicle.LicensePlate,
                    Make = route.AssignedVehicle.Make,
                    Model = route.AssignedVehicle.Model,
                    Capacity = route.AssignedVehicle.Capacity
                } : null,
                AssignedDriver = route.AssignedDriver != null ? new DriverDto
                {
                    Id = route.AssignedDriver.Id,
                    EmployeeNumber = route.AssignedDriver.EmployeeNumber,
                    FirstName = route.AssignedDriver.FullName.FirstName,
                    LastName = route.AssignedDriver.FullName.LastName,
                    MiddleName = route.AssignedDriver.FullName.MiddleName,
                    Phone = route.AssignedDriver.Phone
                } : null
            };
        }

        public async Task<Result<RouteDto>> GenerateOptimalRouteAsync(OptimalRouteRequestDto requestDto, string tenantId)
        {
            try
            {
                var stops = requestDto.Stops ?? new List<RouteStopDto>();
                if (!stops.Any())
                {
                    return Result<RouteDto>.Failure("At least one stop is required for route optimization");
                }

                var optimizedStops = await OptimizeStopSequenceAsync(stops, requestDto.VehicleCapacity);
                var totalDistance = CalculateTotalDistance(optimizedStops);
                var estimatedDuration = CalculateEstimatedDuration(optimizedStops, totalDistance);

                var routeNumber = await GenerateRouteNumberAsync(tenantId);
                var route = new Route
                {
                    RouteNumber = routeNumber,
                    Name = requestDto.RouteName ?? $"Optimized Route {routeNumber}",
                    Description = "Auto-generated optimized route",
                    Status = RouteStatus.Draft,
                    StartTime = requestDto.StartTime,
                    EndTime = requestDto.StartTime.Add(estimatedDuration),
                    Distance = totalDistance,
                    EstimatedDuration = (int)estimatedDuration.TotalMinutes,
                    StartLocation = optimizedStops.First().Address,
                    EndLocation = optimizedStops.Last().Address,
                    TenantId = int.Parse(tenantId),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Routes.Add(route);
                await _context.SaveChangesAsync();

                var routeDto = MapToDto(route);
                return Result<RouteDto>.Success(routeDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating optimal route");
                return Result<RouteDto>.Failure("An error occurred while generating the optimal route");
            }
        }

        public async Task<Result<RouteDto>> OptimizeExistingRouteAsync(int routeId, string tenantId)
        {
            try
            {
                var route = await _context.Routes
                    .Include(r => r.RouteStops)
                    .Where(r => r.Id == routeId && r.TenantId == int.Parse(tenantId) && !r.IsDeleted)
                    .FirstOrDefaultAsync();

                if (route == null)
                {
                    return Result<RouteDto>.Failure("Route not found");
                }

                if (!route.RouteStops.Any())
                {
                    return Result<RouteDto>.Failure("Route has no stops to optimize");
                }

                var stopDtos = route.RouteStops.Select(s => new RouteStopDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Address = s.Address,
                    Latitude = s.Latitude,
                    Longitude = s.Longitude,
                    SequenceNumber = s.SequenceNumber,
                    EstimatedArrivalTime = s.EstimatedArrivalTime
                }).ToList();

                var optimizedStops = await OptimizeStopSequenceAsync(stopDtos, 50);
                var totalDistance = CalculateTotalDistance(optimizedStops);
                var estimatedDuration = CalculateEstimatedDuration(optimizedStops, totalDistance);

                route.Distance = totalDistance;
                route.EstimatedDuration = (int)estimatedDuration.TotalMinutes;
                route.EndTime = route.StartTime.Add(estimatedDuration);
                route.UpdatedAt = DateTime.UtcNow;

                for (int i = 0; i < optimizedStops.Count; i++)
                {
                    var stop = route.RouteStops.First(s => s.Id == optimizedStops[i].Id);
                    stop.SequenceNumber = i + 1;
                    stop.EstimatedArrivalTime = optimizedStops[i].EstimatedArrivalTime;
                }

                await _context.SaveChangesAsync();

                var routeDto = MapToDto(route);
                return Result<RouteDto>.Success(routeDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing existing route {RouteId}", routeId);
                return Result<RouteDto>.Failure("An error occurred while optimizing the route");
            }
        }

        public async Task<Result<RouteEfficiencyMetricsDto>> CalculateRouteEfficiencyMetricsAsync(int routeId, string tenantId)
        {
            try
            {
                var route = await _context.Routes
                    .Include(r => r.RouteStops)
                    .Include(r => r.Students)
                    .Where(r => r.Id == routeId && r.TenantId == int.Parse(tenantId) && !r.IsDeleted)
                    .FirstOrDefaultAsync();

                if (route == null)
                {
                    return Result<RouteEfficiencyMetricsDto>.Failure("Route not found");
                }

                var trips = await _context.Trips
                    .Where(t => t.RouteId == routeId && t.TenantId == int.Parse(tenantId) && !t.IsDeleted)
                    .Where(t => t.ActualStartTime.HasValue && t.ActualEndTime.HasValue)
                    .ToListAsync();

                var metrics = new RouteEfficiencyMetricsDto
                {
                    RouteId = routeId,
                    RouteName = route.Name,
                    TotalDistance = route.Distance,
                    EstimatedDuration = TimeSpan.FromMinutes(route.EstimatedDuration),
                    NumberOfStops = route.RouteStops.Count,
                    StudentCapacity = route.Students.Count,
                    AverageActualDuration = trips.Any() ? 
                        TimeSpan.FromMinutes(trips.Average(t => (t.ActualEndTime.Value - t.ActualStartTime.Value).TotalMinutes)) : 
                        TimeSpan.Zero,
                    OnTimePerformance = CalculateOnTimePerformance(trips),
                    FuelEfficiency = CalculateFuelEfficiency(route.Distance, trips.Count),
                    CostPerStudent = CalculateCostPerStudent(route.Distance, route.Students.Count),
                    OptimizationScore = CalculateOptimizationScore(route, trips),
                    LastCalculated = DateTime.UtcNow
                };

                return Result<RouteEfficiencyMetricsDto>.Success(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating route efficiency metrics for route {RouteId}", routeId);
                return Result<RouteEfficiencyMetricsDto>.Failure("An error occurred while calculating route efficiency metrics");
            }
        }

        public async Task<Result<bool>> ValidateRouteCapacityAsync(int routeId, int vehicleId, string tenantId)
        {
            try
            {
                var route = await _context.Routes
                    .Include(r => r.Students)
                    .Where(r => r.Id == routeId && r.TenantId == int.Parse(tenantId) && !r.IsDeleted)
                    .FirstOrDefaultAsync();

                if (route == null)
                {
                    return Result<bool>.Failure("Route not found");
                }

                var vehicle = await _context.Vehicles
                    .Where(v => v.Id == vehicleId && v.TenantId == int.Parse(tenantId) && !v.IsDeleted)
                    .FirstOrDefaultAsync();

                if (vehicle == null)
                {
                    return Result<bool>.Failure("Vehicle not found");
                }

                var activeStudents = route.Students.Count(s => !s.IsDeleted);
                var isValid = activeStudents <= vehicle.Capacity;

                if (!isValid)
                {
                    _logger.LogWarning("Route {RouteId} capacity validation failed: {StudentCount} students exceed vehicle capacity of {VehicleCapacity}", 
                        routeId, activeStudents, vehicle.Capacity);
                }

                return Result<bool>.Success(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating route capacity for route {RouteId} and vehicle {VehicleId}", routeId, vehicleId);
                return Result<bool>.Failure("An error occurred while validating route capacity");
            }
        }

        private async Task<List<RouteStopDto>> OptimizeStopSequenceAsync(List<RouteStopDto> stops, int vehicleCapacity)
        {
            if (stops.Count <= 2) return stops;

            var optimizedStops = new List<RouteStopDto>();
            var remainingStops = new List<RouteStopDto>(stops);

            if (remainingStops.Any())
            {
                var startStop = remainingStops.First();
                optimizedStops.Add(startStop);
                remainingStops.Remove(startStop);
            }

            while (remainingStops.Any())
            {
                var currentStop = optimizedStops.Last();
                var nearestStop = FindNearestStop(currentStop, remainingStops);
                
                optimizedStops.Add(nearestStop);
                remainingStops.Remove(nearestStop);
            }

            for (int i = 0; i < optimizedStops.Count; i++)
            {
                optimizedStops[i].SequenceNumber = i + 1;
                optimizedStops[i].EstimatedArrivalTime = CalculateEstimatedArrivalTime(optimizedStops, i);
            }

            return optimizedStops;
        }

        private RouteStopDto FindNearestStop(RouteStopDto currentStop, List<RouteStopDto> remainingStops)
        {
            var nearestStop = remainingStops.First();
            var shortestDistance = CalculateDistance(currentStop.Latitude, currentStop.Longitude, 
                nearestStop.Latitude, nearestStop.Longitude);

            foreach (var stop in remainingStops.Skip(1))
            {
                var distance = CalculateDistance(currentStop.Latitude, currentStop.Longitude, 
                    stop.Latitude, stop.Longitude);
                
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestStop = stop;
                }
            }

            return nearestStop;
        }

        private decimal CalculateTotalDistance(List<RouteStopDto> stops)
        {
            if (stops.Count < 2) return 0;

            decimal totalDistance = 0;
            for (int i = 0; i < stops.Count - 1; i++)
            {
                totalDistance += CalculateDistance(
                    stops[i].Latitude, stops[i].Longitude,
                    stops[i + 1].Latitude, stops[i + 1].Longitude);
            }

            return totalDistance;
        }

        private decimal CalculateDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            const decimal earthRadius = 6371;
            var dLat = DegreesToRadians((double)(lat2 - lat1));
            var dLon = DegreesToRadians((double)(lon2 - lon1));

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(DegreesToRadians((double)lat1)) * Math.Cos(DegreesToRadians((double)lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return (decimal)(earthRadius * c);
        }

        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        private TimeSpan CalculateEstimatedDuration(List<RouteStopDto> stops, decimal totalDistance)
        {
            const decimal averageSpeedKmh = 30;
            const int stopTimeMinutes = 3;
            
            var travelTimeMinutes = (double)(totalDistance / averageSpeedKmh * 60);
            var stopTimeTotal = stops.Count * stopTimeMinutes;
            
            return TimeSpan.FromMinutes(travelTimeMinutes + stopTimeTotal);
        }

        private TimeSpan CalculateEstimatedArrivalTime(List<RouteStopDto> stops, int stopIndex)
        {
            if (stopIndex == 0) return TimeSpan.FromHours(7);

            var baseTime = TimeSpan.FromHours(7);
            var travelTime = TimeSpan.FromMinutes(stopIndex * 5);
            
            return baseTime.Add(travelTime);
        }

        private async Task<string> GenerateRouteNumberAsync(string tenantId)
        {
            var count = await _context.Routes
                .Where(r => r.TenantId == int.Parse(tenantId))
                .CountAsync();

            return $"RT-{DateTime.UtcNow:yyyyMM}-{(count + 1):D3}";
        }

        private decimal CalculateOnTimePerformance(List<Trip> trips)
        {
            if (!trips.Any()) return 0;

            var onTimeTrips = trips.Count(t => 
                t.ActualStartTime.HasValue && 
                Math.Abs((t.ActualStartTime.Value - t.ScheduledStartTime).TotalMinutes) <= 5);

            return (decimal)onTimeTrips / trips.Count * 100;
        }

        private decimal CalculateFuelEfficiency(decimal distance, int tripCount)
        {
            if (tripCount == 0 || distance == 0) return 0;
            
            const decimal averageFuelConsumption = 0.08m;
            return distance * averageFuelConsumption / tripCount;
        }

        private decimal CalculateCostPerStudent(decimal distance, int studentCount)
        {
            if (studentCount == 0) return 0;
            
            const decimal costPerKm = 2.5m;
            return distance * costPerKm / studentCount;
        }

        private decimal CalculateOptimizationScore(Route route, List<Trip> trips)
        {
            var distanceScore = Math.Min(100, (50 / Math.Max(1, route.Distance)) * 100);
            var timeScore = Math.Min(100, (60 / Math.Max(1, route.EstimatedDuration)) * 100);
            var performanceScore = CalculateOnTimePerformance(trips);
            
            return (distanceScore + timeScore + performanceScore) / 3;
        }
    }
}

