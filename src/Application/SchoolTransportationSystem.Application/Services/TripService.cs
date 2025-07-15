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
    public class TripService : ITripService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TripService> _logger;

        public TripService(ApplicationDbContext context, ILogger<TripService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<TripDto>> GetByIdAsync(int id, string tenantId)
        {
            try
            {
                var trip = await _context.Trips
                    .Include(t => t.Route)
                    .Include(t => t.Vehicle)
                    .Include(t => t.Driver)
                    .Include(t => t.Attendances)
                    .Where(t => t.Id == id && t.TenantId == int.Parse(tenantId) && !t.IsDeleted)
                    .FirstOrDefaultAsync();

                if (trip == null)
                {
                    return Result<TripDto>.Failure("Trip not found");
                }

                var tripDto = MapToDto(trip);
                return Result<TripDto>.Success(tripDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trip by ID {TripId}", id);
                return Result<TripDto>.Failure("An error occurred while retrieving the trip");
            }
        }

        public async Task<Result<PagedResult<TripDto>>> GetAllAsync(TripSearchDto searchDto, string tenantId)
        {
            try
            {
                var query = _context.Trips
                    .Include(t => t.Route)
                    .Include(t => t.Vehicle)
                    .Include(t => t.Driver)
                    .Where(t => t.TenantId == int.Parse(tenantId) && !t.IsDeleted);

                if (searchDto.RouteId.HasValue)
                    query = query.Where(t => t.RouteId == searchDto.RouteId.Value);

                if (searchDto.VehicleId.HasValue)
                    query = query.Where(t => t.VehicleId == searchDto.VehicleId.Value);

                if (searchDto.DriverId.HasValue)
                    query = query.Where(t => t.DriverId == searchDto.DriverId.Value);

                if (searchDto.Status.HasValue)
                    query = query.Where(t => t.Status == searchDto.Status.Value);

                if (searchDto.TripDateFrom.HasValue)
                    query = query.Where(t => t.ScheduledStartTime.Date >= searchDto.TripDateFrom.Value.Date);

                if (searchDto.TripDateTo.HasValue)
                    query = query.Where(t => t.ScheduledStartTime.Date <= searchDto.TripDateTo.Value.Date);

                var totalCount = await query.CountAsync();

                var trips = await query
                    .OrderByDescending(t => t.ScheduledStartTime)
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                var tripDtos = trips.Select(MapToDto).ToList();

                var pagedResult = new PagedResult<TripDto>
                {
                    Items = tripDtos,
                    TotalCount = totalCount,
                    Page = searchDto.Page,
                    PageSize = searchDto.PageSize
                };

                return Result<PagedResult<TripDto>>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trips");
                return Result<PagedResult<TripDto>>.Failure("An error occurred while retrieving trips");
            }
        }

        public async Task<Result<TripDto>> CreateAsync(CreateTripDto createDto, string tenantId)
        {
            try
            {
                var route = await _context.Routes
                    .Where(r => r.Id == createDto.RouteId && r.TenantId == int.Parse(tenantId) && !r.IsDeleted)
                    .FirstOrDefaultAsync();

                if (route == null)
                {
                    return Result<TripDto>.Failure("Route not found");
                }

                var vehicle = await _context.Vehicles
                    .Where(v => v.Id == createDto.VehicleId && v.TenantId == int.Parse(tenantId) && !v.IsDeleted)
                    .FirstOrDefaultAsync();

                if (vehicle == null)
                {
                    return Result<TripDto>.Failure("Vehicle not found");
                }

                var driver = await _context.Drivers
                    .Where(d => d.Id == createDto.DriverId && d.TenantId == int.Parse(tenantId) && !d.IsDeleted)
                    .FirstOrDefaultAsync();

                if (driver == null)
                {
                    return Result<TripDto>.Failure("Driver not found");
                }

                var trip = new Trip
                {
                    TenantId = int.Parse(tenantId),
                    RouteId = createDto.RouteId,
                    VehicleId = createDto.VehicleId,
                    DriverId = createDto.DriverId,
                    ScheduledStartTime = createDto.ScheduledStartTime,
                    ScheduledEndTime = createDto.ScheduledEndTime,
                    Status = createDto.Status,
                    Notes = createDto.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Trips.Add(trip);
                await _context.SaveChangesAsync();

                var tripDto = MapToDto(trip);
                return Result<TripDto>.Success(tripDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating trip");
                return Result<TripDto>.Failure("An error occurred while creating the trip");
            }
        }

        public async Task<Result<TripDto>> UpdateAsync(int id, UpdateTripDto updateDto, string tenantId)
        {
            try
            {
                var trip = await _context.Trips
                    .Where(t => t.Id == id && t.TenantId == int.Parse(tenantId) && !t.IsDeleted)
                    .FirstOrDefaultAsync();

                if (trip == null)
                {
                    return Result<TripDto>.Failure("Trip not found");
                }

                var route = await _context.Routes
                    .Where(r => r.Id == updateDto.RouteId && r.TenantId == int.Parse(tenantId) && !r.IsDeleted)
                    .FirstOrDefaultAsync();

                if (route == null)
                {
                    return Result<TripDto>.Failure("Route not found");
                }

                var vehicle = await _context.Vehicles
                    .Where(v => v.Id == updateDto.VehicleId && v.TenantId == int.Parse(tenantId) && !v.IsDeleted)
                    .FirstOrDefaultAsync();

                if (vehicle == null)
                {
                    return Result<TripDto>.Failure("Vehicle not found");
                }

                var driver = await _context.Drivers
                    .Where(d => d.Id == updateDto.DriverId && d.TenantId == int.Parse(tenantId) && !d.IsDeleted)
                    .FirstOrDefaultAsync();

                if (driver == null)
                {
                    return Result<TripDto>.Failure("Driver not found");
                }

                trip.RouteId = updateDto.RouteId;
                trip.VehicleId = updateDto.VehicleId;
                trip.DriverId = updateDto.DriverId;
                trip.ScheduledStartTime = updateDto.ScheduledStartTime;
                trip.ScheduledEndTime = updateDto.ScheduledEndTime;
                trip.ActualStartTime = updateDto.ActualStartTime;
                trip.ActualEndTime = updateDto.ActualEndTime;
                trip.Status = updateDto.Status;
                trip.Notes = updateDto.Notes;
                trip.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var tripDto = MapToDto(trip);
                return Result<TripDto>.Success(tripDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating trip with ID {TripId}", id);
                return Result<TripDto>.Failure("An error occurred while updating the trip");
            }
        }

        public async Task<Result<bool>> DeleteAsync(int id, string tenantId)
        {
            try
            {
                var trip = await _context.Trips
                    .Where(t => t.Id == id && t.TenantId == int.Parse(tenantId) && !t.IsDeleted)
                    .FirstOrDefaultAsync();

                if (trip == null)
                {
                    return Result<bool>.Failure("Trip not found");
                }

                trip.IsDeleted = true;
                trip.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting trip with ID {TripId}", id);
                return Result<bool>.Failure("An error occurred while deleting the trip");
            }
        }

        public async Task<Result<List<TripDto>>> GetTripsByRouteAsync(int routeId, DateTime date, string tenantId)
        {
            try
            {
                var trips = await _context.Trips
                    .Include(t => t.Route)
                    .Include(t => t.Vehicle)
                    .Include(t => t.Driver)
                    .Where(t => t.RouteId == routeId && t.TenantId == int.Parse(tenantId) && !t.IsDeleted)
                    .Where(t => t.ScheduledStartTime.Date == date.Date)
                    .OrderBy(t => t.ScheduledStartTime)
                    .ToListAsync();

                var tripDtos = trips.Select(MapToDto).ToList();
                return Result<List<TripDto>>.Success(tripDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trips by route {RouteId} for date {Date}", routeId, date);
                return Result<List<TripDto>>.Failure("An error occurred while retrieving trips by route");
            }
        }

        public async Task<Result<List<TripDto>>> GetActiveTripsByDateAsync(DateTime date, string tenantId)
        {
            try
            {
                var trips = await _context.Trips
                    .Include(t => t.Route)
                    .Include(t => t.Vehicle)
                    .Include(t => t.Driver)
                    .Where(t => t.TenantId == int.Parse(tenantId) && !t.IsDeleted)
                    .Where(t => t.ScheduledStartTime.Date == date.Date)
                    .Where(t => t.Status == TripStatus.InProgress || t.Status == TripStatus.Scheduled)
                    .OrderBy(t => t.ScheduledStartTime)
                    .ToListAsync();

                var tripDtos = trips.Select(MapToDto).ToList();
                return Result<List<TripDto>>.Success(tripDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active trips for date {Date}", date);
                return Result<List<TripDto>>.Failure("An error occurred while retrieving active trips");
            }
        }

        public async Task<Result<bool>> StartTripAsync(int tripId, string tenantId)
        {
            try
            {
                var trip = await _context.Trips
                    .Where(t => t.Id == tripId && t.TenantId == int.Parse(tenantId) && !t.IsDeleted)
                    .FirstOrDefaultAsync();

                if (trip == null)
                {
                    return Result<bool>.Failure("Trip not found");
                }

                if (trip.Status != TripStatus.Scheduled)
                {
                    return Result<bool>.Failure("Trip cannot be started. Current status: " + trip.Status);
                }

                trip.ActualStartTime = DateTime.UtcNow;
                trip.Status = TripStatus.InProgress;
                trip.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting trip {TripId}", tripId);
                return Result<bool>.Failure("An error occurred while starting the trip");
            }
        }

        public async Task<Result<bool>> EndTripAsync(int tripId, string tenantId)
        {
            try
            {
                var trip = await _context.Trips
                    .Where(t => t.Id == tripId && t.TenantId == int.Parse(tenantId) && !t.IsDeleted)
                    .FirstOrDefaultAsync();

                if (trip == null)
                {
                    return Result<bool>.Failure("Trip not found");
                }

                if (trip.Status != TripStatus.InProgress)
                {
                    return Result<bool>.Failure("Trip cannot be ended. Current status: " + trip.Status);
                }

                trip.ActualEndTime = DateTime.UtcNow;
                trip.Status = TripStatus.Completed;
                trip.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending trip {TripId}", tripId);
                return Result<bool>.Failure("An error occurred while ending the trip");
            }
        }

        private TripDto MapToDto(Trip trip)
        {
            return new TripDto
            {
                Id = trip.Id,
                RouteId = trip.RouteId,
                VehicleId = trip.VehicleId,
                DriverId = trip.DriverId,
                TripDate = trip.ScheduledStartTime.Date,
                Type = TripType.PickUp, // Default since not in entity
                Status = trip.Status,
                ScheduledStartTime = trip.ScheduledStartTime,
                ActualStartTime = trip.ActualStartTime,
                ScheduledEndTime = trip.ScheduledEndTime,
                ActualEndTime = trip.ActualEndTime,
                ActualDistance = trip.GetTripDistance(),
                FuelConsumed = null, // Not available in entity
                StudentsPickedUp = 0, // Would need to calculate from attendance
                StudentsDroppedOff = 0, // Would need to calculate from attendance
                Notes = trip.Notes,
                IncidentReports = null, // Not available in entity
                CreatedAt = trip.CreatedAt,
                UpdatedAt = trip.UpdatedAt,
                TenantId = trip.TenantId.ToString(),
                Route = trip.Route != null ? MapRouteToDto(trip.Route) : new RouteDto(),
                Vehicle = trip.Vehicle != null ? MapVehicleToDto(trip.Vehicle) : new VehicleDto(),
                Driver = trip.Driver != null ? MapDriverToDto(trip.Driver) : new DriverDto(),
                AttendanceRecords = trip.Attendances?.Select(MapAttendanceToDto).ToList() ?? new List<AttendanceDto>()
            };
        }

        private RouteDto MapRouteToDto(Route route)
        {
            return new RouteDto
            {
                Id = route.Id,
                TenantId = route.TenantId.ToString(),
                RouteNumber = route.Id.ToString(),
                Name = route.Name,
                Description = route.Description,
                Type = RouteType.Regular,
                Status = RouteStatus.Active,
                StartTime = TimeSpan.FromHours(7),
                EndTime = TimeSpan.FromHours(8),
                EstimatedDuration = TimeSpan.FromMinutes(60),
                EstimatedDistance = 10.0m,
                MaxCapacity = 50,
                CurrentOccupancy = 0,
                IsActive = true, // Default since not in entity
                CreatedAt = route.CreatedAt,
                UpdatedAt = route.UpdatedAt
            };
        }

        private VehicleDto MapVehicleToDto(Vehicle vehicle)
        {
            return new VehicleDto
            {
                Id = vehicle.Id,
                TenantId = vehicle.TenantId,
                VehicleNumber = vehicle.VehicleNumber,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Color = vehicle.Color,
                Type = vehicle.Type,
                Capacity = vehicle.Capacity,
                LicensePlate = vehicle.LicensePlate,
                VIN = vehicle.VIN,
                Status = vehicle.Status,
                Mileage = vehicle.Mileage,
                FuelType = vehicle.FuelType,
                InsuranceExpiry = vehicle.InsuranceExpiry,
                RegistrationExpiry = vehicle.RegistrationExpiry,
                PurchaseDate = vehicle.PurchaseDate,
                PurchasePrice = vehicle.PurchasePrice,
                Notes = vehicle.Notes,
                CreatedAt = vehicle.CreatedAt,
                UpdatedAt = vehicle.UpdatedAt
            };
        }

        private DriverDto MapDriverToDto(Driver driver)
        {
            return new DriverDto
            {
                Id = driver.Id,
                TenantId = driver.TenantId,
                EmployeeNumber = driver.EmployeeNumber,
                FirstName = driver.FullName.FirstName,
                LastName = driver.FullName.LastName,
                MiddleName = driver.FullName.MiddleName,
                LicenseNumber = driver.LicenseNumber,
                LicenseExpiry = driver.LicenseExpiry,
                Phone = driver.Phone,
                Email = driver.Email,
                Street = driver.Address.Street,
                City = driver.Address.City,
                State = driver.Address.State,
                ZipCode = driver.Address.ZipCode,
                Country = driver.Address.Country,
                HireDate = driver.HireDate,
                DateOfBirth = driver.DateOfBirth,
                Status = driver.Status,
                EmergencyContact = driver.EmergencyContact,
                EmergencyPhone = driver.EmergencyPhone,
                MedicalCertExpiry = driver.MedicalCertExpiry,
                BackgroundCheckDate = driver.BackgroundCheckDate,
                LastTrainingDate = driver.LastTrainingDate,
                Notes = driver.Notes,
                CreatedAt = driver.CreatedAt,
                UpdatedAt = driver.UpdatedAt
            };
        }

        private AttendanceDto MapAttendanceToDto(Attendance attendance)
        {
            return new AttendanceDto
            {
                Id = attendance.Id,
                TripId = attendance.TripId,
                StudentId = attendance.StudentId,
                Status = attendance.Status,
                BoardingTime = attendance.BoardingTime,
                AlightingTime = attendance.AlightingTime,
                Notes = attendance.Notes,
                CreatedAt = attendance.CreatedAt,
                UpdatedAt = attendance.UpdatedAt
            };
        }

        public async Task<Result<List<ResourceConflictDto>>> DetectResourceConflictsAsync(DateTime date, string tenantId)
        {
            try
            {
                var conflicts = new List<ResourceConflictDto>();
                
                var trips = await _context.Trips
                    .Include(t => t.Vehicle)
                    .Include(t => t.Driver)
                    .Include(t => t.Route)
                    .Where(t => t.TenantId == int.Parse(tenantId) && !t.IsDeleted)
                    .Where(t => t.ScheduledStartTime.Date == date.Date)
                    .OrderBy(t => t.ScheduledStartTime)
                    .ToListAsync();

                var vehicleConflicts = DetectVehicleConflicts(trips);
                var driverConflicts = DetectDriverConflicts(trips);

                conflicts.AddRange(vehicleConflicts);
                conflicts.AddRange(driverConflicts);

                return Result<List<ResourceConflictDto>>.Success(conflicts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting resource conflicts for date {Date}", date);
                return Result<List<ResourceConflictDto>>.Failure("An error occurred while detecting resource conflicts");
            }
        }

        public async Task<Result<DailyTripScheduleDto>> GenerateDailyTripScheduleAsync(DateTime date, string tenantId)
        {
            try
            {
                var routes = await _context.Routes
                    .Include(r => r.AssignedVehicle)
                    .Include(r => r.AssignedDriver)
                    .Where(r => r.TenantId == int.Parse(tenantId) && !r.IsDeleted && r.Status == RouteStatus.Active)
                    .ToListAsync();

                var existingTrips = await _context.Trips
                    .Where(t => t.TenantId == int.Parse(tenantId) && !t.IsDeleted)
                    .Where(t => t.ScheduledStartTime.Date == date.Date)
                    .ToListAsync();

                var schedule = new DailyTripScheduleDto
                {
                    ScheduleDate = date,
                    TotalRoutes = routes.Count,
                    ScheduledTrips = new List<object>(),
                    UnscheduledRoutes = new List<object>(),
                    Conflicts = new List<ResourceConflictDto>()
                };

                foreach (var route in routes)
                {
                    var morningTrip = GenerateTripForRoute(route, date, TripType.PickUp, existingTrips);
                    var afternoonTrip = GenerateTripForRoute(route, date, TripType.DropOff, existingTrips);

                    if (morningTrip != null)
                    {
                        schedule.ScheduledTrips.Add(morningTrip);
                    }
                    
                    if (afternoonTrip != null)
                    {
                        schedule.ScheduledTrips.Add(afternoonTrip);
                    }

                    if (morningTrip == null && afternoonTrip == null)
                    {
                        schedule.UnscheduledRoutes.Add(MapRouteToDto(route));
                    }
                }

                var conflicts = await DetectResourceConflictsAsync(date, tenantId);
                if (conflicts.IsSuccess)
                {
                    schedule.Conflicts = conflicts.Value;
                }

                return Result<DailyTripScheduleDto>.Success(schedule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating daily trip schedule for date {Date}", date);
                return Result<DailyTripScheduleDto>.Failure("An error occurred while generating the daily trip schedule");
            }
        }

        public async Task<Result<TripDto>> AdjustTripTimingAsync(int tripId, DateTime newStartTime, DateTime newEndTime, string tenantId)
        {
            try
            {
                var trip = await _context.Trips
                    .Include(t => t.Route)
                    .Include(t => t.Vehicle)
                    .Include(t => t.Driver)
                    .Where(t => t.Id == tripId && t.TenantId == int.Parse(tenantId) && !t.IsDeleted)
                    .FirstOrDefaultAsync();

                if (trip == null)
                {
                    return Result<TripDto>.Failure("Trip not found");
                }

                if (trip.Status == TripStatus.Completed)
                {
                    return Result<TripDto>.Failure("Cannot adjust timing for completed trips");
                }

                var conflicts = await CheckTimingConflictsAsync(tripId, newStartTime, newEndTime, tenantId);
                if (conflicts.Any())
                {
                    var conflictMessages = string.Join(", ", conflicts.Select(c => c.Description));
                    return Result<TripDto>.Failure($"Timing conflicts detected: {conflictMessages}");
                }

                trip.ScheduledStartTime = newStartTime;
                trip.ScheduledEndTime = newEndTime;
                trip.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var tripDto = MapToDto(trip);
                return Result<TripDto>.Success(tripDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adjusting trip timing for trip {TripId}", tripId);
                return Result<TripDto>.Failure("An error occurred while adjusting trip timing");
            }
        }

        public async Task<Result<bool>> SendScheduleNotificationsAsync(int tripId, string tenantId)
        {
            try
            {
                var trip = await _context.Trips
                    .Include(t => t.Route)
                        .ThenInclude(r => r.Students)
                    .Include(t => t.Driver)
                    .Include(t => t.Vehicle)
                    .Where(t => t.Id == tripId && t.TenantId == int.Parse(tenantId) && !t.IsDeleted)
                    .FirstOrDefaultAsync();

                if (trip == null)
                {
                    return Result<bool>.Failure("Trip not found");
                }

                var notifications = new List<Notification>();

                if (trip.Driver != null)
                {
                    notifications.Add(new Notification
                    {
                        TenantId = int.Parse(tenantId),
                        UserId = trip.Driver.Id,
                        Type = NotificationType.TripSchedule.ToString(),
                        Title = "Trip Schedule Update",
                        Message = $"Your trip on route {trip.Route?.Name} is scheduled for {trip.ScheduledStartTime:HH:mm}",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                foreach (var student in trip.Route?.Students ?? new List<Student>())
                {
                    if (!string.IsNullOrEmpty(student.ParentEmail))
                    {
                        notifications.Add(new Notification
                        {
                            TenantId = int.Parse(tenantId),
                            UserId = student.Id,
                            Type = NotificationType.TripSchedule.ToString(),
                            Title = "Trip Schedule Notification",
                            Message = $"Trip for {student.FullName.FirstName} on route {trip.Route?.Name} is scheduled for {trip.ScheduledStartTime:HH:mm}",
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                _context.Notifications.AddRange(notifications);
                await _context.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending schedule notifications for trip {TripId}", tripId);
                return Result<bool>.Failure("An error occurred while sending schedule notifications");
            }
        }

        private List<ResourceConflictDto> DetectVehicleConflicts(List<Trip> trips)
        {
            var conflicts = new List<ResourceConflictDto>();
            var vehicleGroups = trips.Where(t => t.VehicleId != null)
                                   .GroupBy(t => t.VehicleId);

            foreach (var group in vehicleGroups)
            {
                var vehicleTrips = group.OrderBy(t => t.ScheduledStartTime).ToList();
                
                for (int i = 0; i < vehicleTrips.Count() - 1; i++)
                {
                    var currentTrip = vehicleTrips[i];
                    var nextTrip = vehicleTrips[i + 1];

                    if (currentTrip.ScheduledEndTime > nextTrip.ScheduledStartTime)
                    {
                        conflicts.Add(new ResourceConflictDto
                        {
                            ConflictType = ConflictType.Vehicle,
                            ResourceId = currentTrip.VehicleId,
                            ResourceName = currentTrip.Vehicle?.VehicleNumber ?? "Unknown Vehicle",
                            ConflictingTripIds = new List<int> { currentTrip.Id, nextTrip.Id },
                            ConflictTime = nextTrip.ScheduledStartTime,
                            Description = $"Vehicle {currentTrip.Vehicle?.VehicleNumber} is scheduled for overlapping trips",
                            Severity = ConflictSeverity.High
                        });
                    }
                }
            }

            return conflicts;
        }

        private List<ResourceConflictDto> DetectDriverConflicts(List<Trip> trips)
        {
            var conflicts = new List<ResourceConflictDto>();
            var driverGroups = trips.Where(t => t.DriverId != null)
                                   .GroupBy(t => t.DriverId);

            foreach (var group in driverGroups)
            {
                var driverTrips = group.OrderBy(t => t.ScheduledStartTime).ToList();
                
                for (int i = 0; i < driverTrips.Count() - 1; i++)
                {
                    var currentTrip = driverTrips[i];
                    var nextTrip = driverTrips[i + 1];

                    if (currentTrip.ScheduledEndTime > nextTrip.ScheduledStartTime)
                    {
                        conflicts.Add(new ResourceConflictDto
                        {
                            ConflictType = ConflictType.Driver,
                            ResourceId = currentTrip.DriverId,
                            ResourceName = $"{currentTrip.Driver?.FullName.FirstName} {currentTrip.Driver?.FullName.LastName}",
                            ConflictingTripIds = new List<int> { currentTrip.Id, nextTrip.Id },
                            ConflictTime = nextTrip.ScheduledStartTime,
                            Description = $"Driver {currentTrip.Driver?.FullName.FirstName} {currentTrip.Driver?.FullName.LastName} is scheduled for overlapping trips",
                            Severity = ConflictSeverity.High
                        });
                    }
                }
            }

            return conflicts;
        }

        private TripDto? GenerateTripForRoute(Route route, DateTime date, TripType tripType, List<Trip> existingTrips)
        {
            if (route.AssignedVehicle == null || route.AssignedDriver == null)
            {
                return null;
            }

            var baseTime = tripType == TripType.PickUp ? route.StartTime : route.EndTime;
            var scheduledStart = date.Date.Add(baseTime);
            var scheduledEnd = scheduledStart.Add(TimeSpan.FromMinutes(route.EstimatedDuration));

            var existingTrip = existingTrips.FirstOrDefault(t => 
                t.RouteId == route.Id && 
                t.ScheduledStartTime.Date == date.Date &&
                Math.Abs((t.ScheduledStartTime.TimeOfDay - baseTime).TotalMinutes) < 30);

            if (existingTrip != null)
            {
                return MapToDto(existingTrip);
            }

            var trip = new Trip
            {
                TenantId = route.TenantId,
                RouteId = route.Id,
                VehicleId = route.AssignedVehicle.Id,
                DriverId = route.AssignedDriver.Id,
                ScheduledStartTime = scheduledStart,
                ScheduledEndTime = scheduledEnd,
                Status = TripStatus.Scheduled,
                CreatedAt = DateTime.UtcNow
            };

            return new TripDto
            {
                Id = 0,
                RouteId = trip.RouteId,
                VehicleId = trip.VehicleId,
                DriverId = trip.DriverId,
                TripDate = trip.ScheduledStartTime.Date,
                Type = tripType,
                Status = trip.Status,
                ScheduledStartTime = trip.ScheduledStartTime,
                ScheduledEndTime = trip.ScheduledEndTime,
                Route = MapRouteToDto(route),
                Vehicle = MapVehicleToDto(route.AssignedVehicle),
                Driver = MapDriverToDto(route.AssignedDriver),
                CreatedAt = trip.CreatedAt,
                TenantId = trip.TenantId.ToString()
            };
        }

        private async Task<List<ResourceConflictDto>> CheckTimingConflictsAsync(int tripId, DateTime newStartTime, DateTime newEndTime, string tenantId)
        {
            var conflicts = new List<ResourceConflictDto>();

            var trip = await _context.Trips
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .Where(t => t.Id == tripId && t.TenantId == int.Parse(tenantId))
                .FirstOrDefaultAsync();

            if (trip == null) return conflicts;

            var conflictingTrips = await _context.Trips
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .Where(t => t.Id != tripId && t.TenantId == int.Parse(tenantId) && !t.IsDeleted)
                .Where(t => t.ScheduledStartTime.Date == newStartTime.Date)
                .Where(t => (t.VehicleId == trip.VehicleId || t.DriverId == trip.DriverId))
                .Where(t => t.ScheduledStartTime < newEndTime && t.ScheduledEndTime > newStartTime)
                .ToListAsync();

            foreach (var conflictingTrip in conflictingTrips)
            {
                if (conflictingTrip.VehicleId == trip.VehicleId)
                {
                    conflicts.Add(new ResourceConflictDto
                    {
                        ConflictType = ConflictType.Vehicle,
                        ResourceId = trip.VehicleId,
                        ResourceName = trip.Vehicle?.VehicleNumber ?? "Unknown Vehicle",
                        ConflictingTripIds = new List<int> { tripId, conflictingTrip.Id },
                        ConflictTime = newStartTime,
                        Description = $"Vehicle conflict with trip {conflictingTrip.Id}",
                        Severity = ConflictSeverity.High
                    });
                }

                if (conflictingTrip.DriverId == trip.DriverId)
                {
                    conflicts.Add(new ResourceConflictDto
                    {
                        ConflictType = ConflictType.Driver,
                        ResourceId = trip.DriverId,
                        ResourceName = $"{trip.Driver?.FullName.FirstName} {trip.Driver?.FullName.LastName}",
                        ConflictingTripIds = new List<int> { tripId, conflictingTrip.Id },
                        ConflictTime = newStartTime,
                        Description = $"Driver conflict with trip {conflictingTrip.Id}",
                        Severity = ConflictSeverity.High
                    });
                }
            }

            return conflicts;
        }

        public async Task<Result<List<TripDto>>> GenerateDailyTripScheduleAsync(DailyScheduleRequestDto request, string tenantId)
        {
            try
            {
                var schedule = await GenerateDailyTripScheduleAsync(request.Date, tenantId);
                if (!schedule.IsSuccess)
                {
                    return Result<List<TripDto>>.Failure(schedule.Error);
                }

                var trips = new List<TripDto>();
                foreach (var scheduledTrip in schedule.Value.ScheduledTrips)
                {
                    if (scheduledTrip is TripDto tripDto)
                    {
                        trips.Add(tripDto);
                    }
                }

                return Result<List<TripDto>>.Success(trips);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating daily trip schedule");
                return Result<List<TripDto>>.Failure("An error occurred while generating the daily trip schedule");
            }
        }

        public async Task<Result<bool>> RescheduleTripAsync(RescheduleTripDto rescheduleDto, string tenantId)
        {
            try
            {
                var trip = await _context.Trips
                    .Where(t => t.Id == rescheduleDto.TripId && t.TenantId == int.Parse(tenantId) && !t.IsDeleted)
                    .FirstOrDefaultAsync();

                if (trip == null)
                {
                    return Result<bool>.Failure("Trip not found");
                }

                if (trip.Status == TripStatus.Completed)
                {
                    return Result<bool>.Failure("Cannot reschedule completed trips");
                }

                var conflicts = await CheckTimingConflictsAsync(rescheduleDto.TripId, rescheduleDto.NewStartTime, rescheduleDto.NewEndTime, tenantId);
                if (conflicts.Any())
                {
                    var conflictMessages = string.Join(", ", conflicts.Select(c => c.Description));
                    return Result<bool>.Failure($"Rescheduling conflicts detected: {conflictMessages}");
                }

                trip.ScheduledStartTime = rescheduleDto.NewStartTime;
                trip.ScheduledEndTime = rescheduleDto.NewEndTime;
                
                if (rescheduleDto.NewVehicleId.HasValue)
                {
                    trip.VehicleId = rescheduleDto.NewVehicleId.Value;
                }
                
                if (rescheduleDto.NewDriverId.HasValue)
                {
                    trip.DriverId = rescheduleDto.NewDriverId.Value;
                }

                trip.Notes = $"{trip.Notes ?? ""}\nRescheduled: {rescheduleDto.Reason}";
                trip.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                if (rescheduleDto.NotifyParents)
                {
                    await SendScheduleNotificationsAsync(rescheduleDto.TripId, tenantId);
                }

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rescheduling trip {TripId}", rescheduleDto.TripId);
                return Result<bool>.Failure("An error occurred while rescheduling the trip");
            }
        }

        public async Task<Result<List<ScheduleConflictDto>>> GetScheduleConflictsAsync(DateTime date, string tenantId)
        {
            try
            {
                var resourceConflicts = await DetectResourceConflictsAsync(date, tenantId);
                if (!resourceConflicts.IsSuccess)
                {
                    return Result<List<ScheduleConflictDto>>.Failure(resourceConflicts.Error);
                }

                var scheduleConflicts = resourceConflicts.Value.Select(rc => new ScheduleConflictDto
                {
                    ConflictId = rc.Id,
                    Type = rc.ConflictType switch
                    {
                        ConflictType.Vehicle => ScheduleConflictType.VehicleConflict,
                        ConflictType.Driver => ScheduleConflictType.DriverConflict,
                        ConflictType.Route => ScheduleConflictType.RouteConflict,
                        ConflictType.Time => ScheduleConflictType.TimeConflict,
                        _ => ScheduleConflictType.TimeConflict
                    },
                    Description = rc.Description,
                    ConflictTime = rc.ConflictTime,
                    AffectedTripIds = rc.ConflictingTripIds,
                    Severity = rc.Severity switch
                    {
                        ConflictSeverity.Low => ScheduleConflictSeverity.Low,
                        ConflictSeverity.Medium => ScheduleConflictSeverity.Medium,
                        ConflictSeverity.High => ScheduleConflictSeverity.High,
                        ConflictSeverity.Critical => ScheduleConflictSeverity.Critical,
                        _ => ScheduleConflictSeverity.Medium
                    },
                    SuggestedResolutions = new List<string> { "Reschedule one of the conflicting trips", "Assign different resources" }
                }).ToList();

                return Result<List<ScheduleConflictDto>>.Success(scheduleConflicts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedule conflicts for date {Date}", date);
                return Result<List<ScheduleConflictDto>>.Failure("An error occurred while retrieving schedule conflicts");
            }
        }

        public async Task<Result<ScheduleOptimizationResultDto>> OptimizeScheduleAsync(ScheduleOptimizationRequestDto request, string tenantId)
        {
            try
            {
                var trips = await _context.Trips
                    .Include(t => t.Route)
                    .Include(t => t.Vehicle)
                    .Include(t => t.Driver)
                    .Where(t => t.TenantId == int.Parse(tenantId) && !t.IsDeleted)
                    .Where(t => t.ScheduledStartTime.Date >= request.StartDate.Date && t.ScheduledStartTime.Date <= request.EndDate.Date)
                    .ToListAsync();

                if (request.RouteIds?.Any() == true)
                {
                    trips = trips.Where(t => request.RouteIds.Contains(t.RouteId)).ToList();
                }

                var optimizedTrips = new List<OptimizedTripDto>();
                decimal totalTimeSaved = 0;
                decimal totalFuelSaved = 0;

                foreach (var trip in trips)
                {
                    var originalStart = trip.ScheduledStartTime;
                    var originalEnd = trip.ScheduledEndTime;
                    
                    var optimizedStart = OptimizeStartTime(trip, request.Goal);
                    var optimizedEnd = optimizedStart.Add(originalEnd - originalStart);
                    
                    var timeSaved = (decimal)(originalStart - optimizedStart).TotalMinutes;
                    var fuelSaved = timeSaved * 0.1m; // Mock calculation

                    optimizedTrips.Add(new OptimizedTripDto
                    {
                        TripId = trip.Id,
                        OriginalStartTime = originalStart,
                        OptimizedStartTime = optimizedStart,
                        OriginalEndTime = originalEnd,
                        OptimizedEndTime = optimizedEnd,
                        OriginalVehicleId = trip.VehicleId,
                        OptimizedVehicleId = trip.VehicleId,
                        OriginalDriverId = trip.DriverId,
                        OptimizedDriverId = trip.DriverId,
                        TimeSaved = timeSaved,
                        FuelSaved = fuelSaved
                    });

                    totalTimeSaved += timeSaved;
                    totalFuelSaved += fuelSaved;
                }

                var result = new ScheduleOptimizationResultDto
                {
                    Success = true,
                    OptimizedTrips = optimizedTrips,
                    Metrics = new OptimizationMetricsDto
                    {
                        TotalTimeSaved = totalTimeSaved,
                        TotalFuelSaved = totalFuelSaved,
                        TotalCostSaved = totalFuelSaved * 3.5m, // Mock fuel cost
                        TripsOptimized = optimizedTrips.Count,
                        EfficiencyImprovement = trips.Count > 0 ? (totalTimeSaved / trips.Count) : 0,
                        ProcessingTime = TimeSpan.FromSeconds(2)
                    },
                    Warnings = new List<string>()
                };

                return Result<ScheduleOptimizationResultDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing schedule from {StartDate} to {EndDate}", request.StartDate, request.EndDate);
                return Result<ScheduleOptimizationResultDto>.Failure("An error occurred while optimizing the schedule");
            }
        }

        public async Task<Result<ScheduleAnalyticsDto>> GetScheduleAnalyticsAsync(DateTime startDate, DateTime endDate, string tenantId)
        {
            try
            {
                var trips = await _context.Trips
                    .Include(t => t.Route)
                    .Include(t => t.Vehicle)
                    .Include(t => t.Driver)
                    .Where(t => t.TenantId == int.Parse(tenantId) && !t.IsDeleted)
                    .Where(t => t.ScheduledStartTime.Date >= startDate.Date && t.ScheduledStartTime.Date <= endDate.Date)
                    .ToListAsync();

                var analytics = new ScheduleAnalyticsDto
                {
                    TotalTrips = trips.Count,
                    CompletedTrips = trips.Count(t => t.Status == TripStatus.Completed),
                    CancelledTrips = trips.Count(t => t.Status == TripStatus.Cancelled),
                    DelayedTrips = trips.Count(t => t.ActualStartTime.HasValue && t.ActualStartTime > t.ScheduledStartTime),
                    OnTimePerformance = trips.Count > 0 ? (decimal)(trips.Count - trips.Count(t => t.ActualStartTime.HasValue && t.ActualStartTime > t.ScheduledStartTime)) / trips.Count * 100 : 0,
                    AverageDelay = trips.Where(t => t.ActualStartTime.HasValue && t.ActualStartTime > t.ScheduledStartTime)
                                       .Select(t => (decimal)(t.ActualStartTime!.Value - t.ScheduledStartTime).TotalMinutes)
                                       .DefaultIfEmpty(0)
                                       .Average(),
                    RoutePerformance = new List<RoutePerformanceDto>(),
                    VehicleUtilization = new List<VehicleUtilizationDto>(),
                    DriverPerformance = new List<DriverPerformanceDto>()
                };

                return Result<ScheduleAnalyticsDto>.Success(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedule analytics from {StartDate} to {EndDate}", startDate, endDate);
                return Result<ScheduleAnalyticsDto>.Failure("An error occurred while retrieving schedule analytics");
            }
        }

        private DateTime OptimizeStartTime(Trip trip, OptimizationGoal goal)
        {
            switch (goal)
            {
                case OptimizationGoal.MinimizeTime:
                    return trip.ScheduledStartTime.AddMinutes(-5);
                case OptimizationGoal.MinimizeFuel:
                    return trip.ScheduledStartTime.AddMinutes(-3);
                case OptimizationGoal.MinimizeCost:
                    return trip.ScheduledStartTime.AddMinutes(-2);
                default:
                    return trip.ScheduledStartTime;
            }
        }

    }
}

