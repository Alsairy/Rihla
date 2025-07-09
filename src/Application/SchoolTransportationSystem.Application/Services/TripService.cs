using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rihla.Application.DTOs;
using Rihla.Application.Interfaces;
using Rihla.Core.Common;
using Rihla.Core.Entities;
using Rihla.Core.Enums;
using Rihla.Core.Interfaces;

namespace Rihla.Application.Services
{
    public class TripService : ITripService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TripService> _logger;

        public TripService(IUnitOfWork unitOfWork, ILogger<TripService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<TripDto>> GetByIdAsync(int id, string tenantId)
        {
            try
            {
                var trip = await _unitOfWork.Trips
                    .QueryWithIncludes(tenantId, t => t.Route, t => t.Vehicle, t => t.Driver, t => t.Attendances)
                    .FirstOrDefaultAsync(t => t.Id == id);

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
                var query = _unitOfWork.Trips
                    .QueryWithIncludes(tenantId, t => t.Route, t => t.Vehicle, t => t.Driver);

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
                var route = await _unitOfWork.Routes
                    .GetByIdAsync(createDto.RouteId, tenantId);

                if (route == null)
                {
                    return Result<TripDto>.Failure("Route not found");
                }

                var vehicle = await _unitOfWork.Vehicles
                    .GetByIdAsync(createDto.VehicleId, tenantId);

                if (vehicle == null)
                {
                    return Result<TripDto>.Failure("Vehicle not found");
                }

                var driver = await _unitOfWork.Drivers
                    .GetByIdAsync(createDto.DriverId, tenantId);

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

                await _unitOfWork.Trips.AddAsync(trip);
                await _unitOfWork.SaveChangesAsync();

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
                var trip = await _unitOfWork.Trips
                    .GetByIdAsync(id, tenantId);

                if (trip == null)
                {
                    return Result<TripDto>.Failure("Trip not found");
                }

                var route = await _unitOfWork.Routes
                    .GetByIdAsync(updateDto.RouteId, tenantId);

                if (route == null)
                {
                    return Result<TripDto>.Failure("Route not found");
                }

                var vehicle = await _unitOfWork.Vehicles
                    .GetByIdAsync(updateDto.VehicleId, tenantId);

                if (vehicle == null)
                {
                    return Result<TripDto>.Failure("Vehicle not found");
                }

                var driver = await _unitOfWork.Drivers
                    .GetByIdAsync(updateDto.DriverId, tenantId);

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

                await _unitOfWork.SaveChangesAsync();

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
                var trip = await _unitOfWork.Trips
                    .GetByIdAsync(id, tenantId);

                if (trip == null)
                {
                    return Result<bool>.Failure("Trip not found");
                }

                trip.IsDeleted = true;
                trip.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();
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
                var trips = await _unitOfWork.Trips
                    .QueryWithIncludes(tenantId, t => t.Route, t => t.Vehicle, t => t.Driver)
                    .Where(t => t.RouteId == routeId)
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
                var trips = await _unitOfWork.Trips
                    .QueryWithIncludes(tenantId, t => t.Route, t => t.Vehicle, t => t.Driver)
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
                var trip = await _unitOfWork.Trips
                    .GetByIdAsync(tripId, tenantId);

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

                await _unitOfWork.SaveChangesAsync();
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
                var trip = await _unitOfWork.Trips
                    .GetByIdAsync(tripId, tenantId);

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

                await _unitOfWork.SaveChangesAsync();
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
    }
}

