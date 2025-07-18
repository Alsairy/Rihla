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
    public class GPSTrackingService : IGPSTrackingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GPSTrackingService> _logger;
        private readonly IUserContext _userContext;

        public GPSTrackingService(ApplicationDbContext context, ILogger<GPSTrackingService> logger, IUserContext userContext)
        {
            _context = context;
            _logger = logger;
            _userContext = userContext;
        }

        public async Task<Result<bool>> StartRealTimeTrackingAsync(int vehicleId, int tripId, string tenantId)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .Where(v => v.Id == vehicleId && v.TenantId == int.Parse(tenantId ?? "0") && !v.IsDeleted)
                    .FirstOrDefaultAsync();

                if (vehicle == null)
                {
                    return Result<bool>.Failure("Vehicle not found");
                }

                var trip = await _context.Trips
                    .Where(t => t.Id == tripId && t.TenantId == int.Parse(tenantId) && !t.IsDeleted)
                    .FirstOrDefaultAsync();

                if (trip == null)
                {
                    return Result<bool>.Failure("Trip not found");
                }

                trip.Status = TripStatus.InProgress;
                trip.ActualStartTime = DateTime.UtcNow;
                trip.UpdatedAt = DateTime.UtcNow;

                var trackingRecord = new VehicleLocation
                {
                    VehicleId = vehicleId,
                    TripId = tripId,
                    Latitude = 0, // Will be updated with first location update
                    Longitude = 0,
                    Speed = 0,
                    Heading = 0,
                    Timestamp = DateTime.UtcNow,
                    IsActive = true,
                    TenantId = tenantId ?? "0",
                    CreatedAt = DateTime.UtcNow
                };

                _context.VehicleLocations.Add(trackingRecord);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Real-time tracking started for vehicle {VehicleId} on trip {TripId}", vehicleId, tripId);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting real-time tracking for vehicle {VehicleId} on trip {TripId}", vehicleId, tripId);
                return Result<bool>.Failure("An error occurred while starting real-time tracking");
            }
        }

        public async Task<Result<bool>> UpdateVehicleLocationAsync(int vehicleId, double latitude, double longitude, DateTime timestamp, string tenantId)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .Where(v => v.Id == vehicleId && v.TenantId == int.Parse(tenantId ?? "0") && !v.IsDeleted)
                    .FirstOrDefaultAsync();

                if (vehicle == null)
                {
                    return Result<bool>.Failure("Vehicle not found");
                }

                var activeTrip = await _context.Trips
                    .Where(t => t.VehicleId == vehicleId && t.Status == TripStatus.InProgress && 
                               t.TenantId == int.Parse(tenantId ?? "0") && !t.IsDeleted)
                    .FirstOrDefaultAsync();

                var locationRecord = new VehicleLocation
                {
                    VehicleId = vehicleId,
                    TripId = activeTrip?.Id,
                    Latitude = (decimal)latitude,
                    Longitude = (decimal)longitude,
                    Speed = (decimal)await CalculateSpeed(vehicleId, (double)latitude, (double)longitude, timestamp),
                    Heading = (decimal)await CalculateHeading(vehicleId, (double)latitude, (double)longitude),
                    Timestamp = timestamp,
                    IsActive = true,
                    TenantId = tenantId ?? "0",
                    CreatedAt = DateTime.UtcNow
                };

                _context.VehicleLocations.Add(locationRecord);

                vehicle.CurrentLatitude = (decimal?)latitude;
                vehicle.CurrentLongitude = (decimal?)longitude;
                vehicle.LastLocationUpdate = timestamp;
                vehicle.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogDebug("Location updated for vehicle {VehicleId}: {Latitude}, {Longitude}", vehicleId, latitude, longitude);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating location for vehicle {VehicleId}", vehicleId);
                return Result<bool>.Failure("An error occurred while updating vehicle location");
            }
        }

        public async Task<Result<List<GeofenceViolationDto>>> CheckGeofenceViolationsAsync(int vehicleId, double latitude, double longitude, string tenantId)
        {
            try
            {
                var violations = new List<GeofenceViolationDto>();

                var activeTrip = await _context.Trips
                    .Include(t => t.Route)
                        .ThenInclude(r => r.RouteStops)
                    .Where(t => t.VehicleId == vehicleId && t.Status == TripStatus.InProgress && 
                               t.TenantId == int.Parse(tenantId ?? "0") && !t.IsDeleted)
                    .FirstOrDefaultAsync();

                if (activeTrip == null)
                {
                    return Result<List<GeofenceViolationDto>>.Success(violations);
                }

                const double allowedDeviationKm = 2.0; // 2km allowed deviation from route
                const double speedLimitKmh = 60.0; // Speed limit in km/h

                var nearestStop = activeTrip.Route.RouteStops
                    .OrderBy(s => CalculateDistance(latitude, longitude, (double)s.Latitude, (double)s.Longitude))
                    .FirstOrDefault();

                if (nearestStop != null)
                {
                    var distanceFromRoute = CalculateDistance(latitude, longitude, (double)nearestStop.Latitude, (double)nearestStop.Longitude);
                    
                    if (distanceFromRoute > allowedDeviationKm)
                    {
                        violations.Add(new GeofenceViolationDto
                        {
                            VehicleId = vehicleId,
                            TripId = activeTrip.Id,
                            ViolationType = "Route Deviation",
                            Description = $"Vehicle is {distanceFromRoute:F2}km away from the designated route",
                            Latitude = (decimal)latitude,
                            Longitude = (decimal)longitude,
                            Timestamp = DateTime.UtcNow,
                            Severity = distanceFromRoute > 5.0 ? "High" : "Medium",
                            ActionRequired = distanceFromRoute > 5.0 ? "Immediate contact with driver required" : "Monitor closely"
                        });
                    }
                }

                var currentSpeed = await GetCurrentSpeedAsync(vehicleId);
                if (currentSpeed > speedLimitKmh)
                {
                    violations.Add(new GeofenceViolationDto
                    {
                        VehicleId = vehicleId,
                        TripId = activeTrip.Id,
                        ViolationType = "Speed Violation",
                        Description = $"Vehicle speed ({currentSpeed:F1} km/h) exceeds limit ({speedLimitKmh} km/h)",
                        Latitude = (decimal)latitude,
                        Longitude = (decimal)longitude,
                        Timestamp = DateTime.UtcNow,
                        Severity = currentSpeed > speedLimitKmh * 1.2 ? "High" : "Medium",
                        ActionRequired = "Contact driver to reduce speed"
                    });
                }

                var restrictedAreas = await GetRestrictedAreasAsync(tenantId);
                foreach (var area in restrictedAreas)
                {
                    var distanceFromArea = CalculateDistance(latitude, longitude, (double)area.Latitude, (double)area.Longitude);
                    if (distanceFromArea <= (double)area.RadiusKm && IsWithinRestrictedHours(area))
                    {
                        violations.Add(new GeofenceViolationDto
                        {
                            VehicleId = vehicleId,
                            TripId = activeTrip.Id,
                            ViolationType = "Restricted Area",
                            Description = $"Vehicle entered restricted area: {area.Name}",
                            Latitude = (decimal)latitude,
                            Longitude = (decimal)longitude,
                            Timestamp = DateTime.UtcNow,
                            Severity = "High",
                            ActionRequired = "Immediate evacuation from restricted area"
                        });
                    }
                }

                if (violations.Any())
                {
                    _logger.LogWarning("Geofence violations detected for vehicle {VehicleId}: {ViolationCount} violations", vehicleId, violations.Count);
                }

                return Result<List<GeofenceViolationDto>>.Success(violations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking geofence violations for vehicle {VehicleId}", vehicleId);
                return Result<List<GeofenceViolationDto>>.Failure("An error occurred while checking geofence violations");
            }
        }

        public async Task<Result<EstimatedArrivalDto>> GetEstimatedArrivalTimeAsync(int tripId, int stopId, string tenantId)
        {
            try
            {
                var trip = await _context.Trips
                    .Include(t => t.Route)
                        .ThenInclude(r => r.RouteStops)
                    .Include(t => t.Vehicle)
                    .Where(t => t.Id == tripId && t.TenantId == int.Parse(tenantId) && !t.IsDeleted)
                    .FirstOrDefaultAsync();

                if (trip == null)
                {
                    return Result<EstimatedArrivalDto>.Failure("Trip not found");
                }

                var targetStop = trip.Route.RouteStops.FirstOrDefault(s => s.Id == stopId);
                if (targetStop == null)
                {
                    return Result<EstimatedArrivalDto>.Failure("Stop not found in route");
                }

                var currentLocation = await _context.VehicleLocations
                    .Where(l => l.VehicleId == trip.VehicleId && l.IsActive)
                    .OrderByDescending(l => l.Timestamp)
                    .FirstOrDefaultAsync();

                if (currentLocation == null)
                {
                    return Result<EstimatedArrivalDto>.Failure("Current vehicle location not available");
                }

                var distanceKm = CalculateDistance((double)currentLocation.Latitude, (double)currentLocation.Longitude, 
                                                 (double)targetStop.Latitude, (double)targetStop.Longitude);

                var averageSpeed = await GetAverageSpeedAsync(trip.VehicleId, TimeSpan.FromMinutes(10));
                if (averageSpeed <= 0)
                {
                    averageSpeed = 30.0; // Default speed assumption in km/h
                }

                var estimatedTravelTimeHours = distanceKm / averageSpeed;
                var estimatedArrival = DateTime.UtcNow.AddHours(estimatedTravelTimeHours);

                var bufferMinutes = Math.Min(distanceKm * 2, 15); // 2 minutes per km, max 15 minutes
                estimatedArrival = estimatedArrival.AddMinutes(bufferMinutes);

                var result = new EstimatedArrivalDto
                {
                    TripId = tripId,
                    StopId = stopId,
                    StopName = targetStop.Name,
                    EstimatedArrivalTime = estimatedArrival,
                    DistanceKm = (decimal)distanceKm,
                    AverageSpeedKmh = (decimal)averageSpeed,
                    ConfidenceLevel = (decimal)CalculateConfidenceLevel(distanceKm, averageSpeed),
                    LastUpdated = DateTime.UtcNow,
                    DelayMinutes = CalculateDelay(trip, targetStop, estimatedArrival)
                };

                _logger.LogDebug("ETA calculated for trip {TripId} to stop {StopId}: {ETA}", tripId, stopId, estimatedArrival);
                return Result<EstimatedArrivalDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating ETA for trip {TripId} to stop {StopId}", tripId, stopId);
                return Result<EstimatedArrivalDto>.Failure("An error occurred while calculating estimated arrival time");
            }
        }

        public async Task<Result<List<VehicleLocationDto>>> GetVehicleLocationHistoryAsync(int vehicleId, DateTime startTime, DateTime endTime, string tenantId)
        {
            try
            {
                var locations = await _context.VehicleLocations
                    .Where(l => l.VehicleId == vehicleId && l.TenantId == (tenantId ?? "0") &&
                               l.Timestamp >= startTime && l.Timestamp <= endTime)
                    .OrderBy(l => l.Timestamp)
                    .ToListAsync();

                var locationDtos = locations.Select(l => new VehicleLocationDto
                {
                    Id = l.Id,
                    VehicleId = l.VehicleId,
                    TripId = l.TripId,
                    Latitude = l.Latitude,
                    Longitude = l.Longitude,
                    Speed = l.Speed,
                    Heading = l.Heading,
                    Timestamp = l.Timestamp,
                    IsActive = l.IsActive
                }).ToList();

                return Result<List<VehicleLocationDto>>.Success(locationDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting location history for vehicle {VehicleId}", vehicleId);
                return Result<List<VehicleLocationDto>>.Failure("An error occurred while retrieving location history");
            }
        }

        public async Task<Result<bool>> StopRealTimeTrackingAsync(int vehicleId, string tenantId)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .Where(v => v.Id == vehicleId && v.TenantId == int.Parse(tenantId ?? "0") && !v.IsDeleted)
                    .FirstOrDefaultAsync();

                if (vehicle == null)
                {
                    return Result<bool>.Failure("Vehicle not found");
                }

                var activeLocations = await _context.VehicleLocations
                    .Where(l => l.VehicleId == vehicleId && l.IsActive)
                    .ToListAsync();

                foreach (var location in activeLocations)
                {
                    location.IsActive = false;
                    location.UpdatedAt = DateTime.UtcNow;
                }

                var activeTrips = await _context.Trips
                    .Where(t => t.VehicleId == vehicleId && t.Status == TripStatus.InProgress && 
                               t.TenantId == int.Parse(tenantId ?? "0") && !t.IsDeleted)
                    .ToListAsync();

                foreach (var trip in activeTrips)
                {
                    trip.Status = TripStatus.Completed;
                    trip.ActualEndTime = DateTime.UtcNow;
                    trip.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Real-time tracking stopped for vehicle {VehicleId}", vehicleId);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping real-time tracking for vehicle {VehicleId}", vehicleId);
                return Result<bool>.Failure("An error occurred while stopping real-time tracking");
            }
        }

        public async Task<Result<List<VehicleLocationDto>>> GetActiveVehicleLocationsAsync(string tenantId)
        {
            try
            {
                var activeLocations = await _context.VehicleLocations
                    .Include(l => l.Vehicle)
                    .Where(l => l.IsActive && l.TenantId == (tenantId ?? "0"))
                    .GroupBy(l => l.VehicleId)
                    .Select(g => g.OrderByDescending(l => l.Timestamp).First())
                    .ToListAsync();

                var locationDtos = activeLocations.Select(l => new VehicleLocationDto
                {
                    Id = l.Id,
                    VehicleId = l.VehicleId,
                    TripId = l.TripId,
                    Latitude = l.Latitude,
                    Longitude = l.Longitude,
                    Speed = l.Speed,
                    Heading = l.Heading,
                    Timestamp = l.Timestamp,
                    IsActive = l.IsActive,
                    VehicleName = l.Vehicle?.LicensePlate ?? "Unknown"
                }).ToList();

                return Result<List<VehicleLocationDto>>.Success(locationDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active vehicle locations");
                return Result<List<VehicleLocationDto>>.Failure("An error occurred while retrieving active vehicle locations");
            }
        }


        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadiusKm = 6371.0;
            
            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);
            
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            
            return earthRadiusKm * c;
        }

        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        private async Task<double> CalculateSpeed(int vehicleId, double latitude, double longitude, DateTime timestamp)
        {
            var previousLocation = await _context.VehicleLocations
                .Where(l => l.VehicleId == vehicleId && l.Timestamp < timestamp)
                .OrderByDescending(l => l.Timestamp)
                .FirstOrDefaultAsync();

            if (previousLocation == null)
                return 0;

            var distance = CalculateDistance((double)previousLocation.Latitude, (double)previousLocation.Longitude, latitude, longitude);
            var timeHours = (timestamp - previousLocation.Timestamp).TotalHours;

            return timeHours > 0 ? distance / timeHours : 0;
        }

        private async Task<double> CalculateHeading(int vehicleId, double latitude, double longitude)
        {
            var previousLocation = await _context.VehicleLocations
                .Where(l => l.VehicleId == vehicleId)
                .OrderByDescending(l => l.Timestamp)
                .FirstOrDefaultAsync();

            if (previousLocation == null)
                return 0;

            var dLon = DegreesToRadians(longitude - (double)previousLocation.Longitude);
            var lat1 = DegreesToRadians((double)previousLocation.Latitude);
            var lat2 = DegreesToRadians(latitude);

            var y = Math.Sin(dLon) * Math.Cos(lat2);
            var x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);

            var heading = Math.Atan2(y, x);
            return (heading * 180 / Math.PI + 360) % 360;
        }

        private async Task<double> GetCurrentSpeedAsync(int vehicleId)
        {
            var recentLocation = await _context.VehicleLocations
                .Where(l => l.VehicleId == vehicleId && l.IsActive)
                .OrderByDescending(l => l.Timestamp)
                .FirstOrDefaultAsync();

            return (double)(recentLocation?.Speed ?? 0);
        }

        private async Task<double> GetAverageSpeedAsync(int vehicleId, TimeSpan timeWindow)
        {
            var cutoffTime = DateTime.UtcNow.Subtract(timeWindow);
            var recentLocations = await _context.VehicleLocations
                .Where(l => l.VehicleId == vehicleId && l.Timestamp >= cutoffTime && l.Speed > 0)
                .ToListAsync();

            return recentLocations.Any() ? (double)recentLocations.Average(l => l.Speed) : 0;
        }

        private async Task<List<RestrictedAreaDto>> GetRestrictedAreasAsync(string tenantId)
        {
            return new List<RestrictedAreaDto>
            {
                new RestrictedAreaDto
                {
                    Name = "Hospital Zone",
                    Latitude = 24.7136M, // Example coordinates
                    Longitude = 46.6753M,
                    RadiusKm = 0.5M,
                    RestrictedHours = new List<string> { "22:00-06:00" }
                }
            };
        }

        private bool IsWithinRestrictedHours(RestrictedAreaDto area)
        {
            if (area.RestrictedHours == null || !area.RestrictedHours.Any())
                return false;

            var currentTime = DateTime.Now.TimeOfDay;
            var timeRange = area.RestrictedHours.FirstOrDefault();
            if (string.IsNullOrEmpty(timeRange))
                return false;
                
            var parts = timeRange.Split('-');
            
            if (parts.Length != 2)
                return false;

            if (TimeSpan.TryParse(parts[0], out var startTime) && TimeSpan.TryParse(parts[1], out var endTime))
            {
                if (startTime <= endTime)
                {
                    return currentTime >= startTime && currentTime <= endTime;
                }
                else
                {
                    return currentTime >= startTime || currentTime <= endTime;
                }
            }

            return false;
        }

        private double CalculateConfidenceLevel(double distanceKm, double averageSpeed)
        {
            if (distanceKm < 5 && averageSpeed > 0)
                return 0.9;
            else if (distanceKm < 15 && averageSpeed > 0)
                return 0.7;
            else
                return 0.5;
        }

        private int CalculateDelay(Trip trip, RouteStop targetStop, DateTime estimatedArrival)
        {
            return 0;
        }
    }
}
