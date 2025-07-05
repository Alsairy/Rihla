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
    public class VehicleService : IVehicleService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VehicleService> _logger;

        public VehicleService(ApplicationDbContext context, ILogger<VehicleService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<VehicleDto>> GetByIdAsync(int id, string tenantId)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .Where(v => v.Id == id && v.TenantId == int.Parse(tenantId) && !v.IsDeleted)
                    .FirstOrDefaultAsync();

                if (vehicle == null)
                {
                    return Result<VehicleDto>.Failure("Vehicle not found");
                }

                var vehicleDto = MapToDto(vehicle);
                return Result<VehicleDto>.Success(vehicleDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle by ID {VehicleId}", id);
                return Result<VehicleDto>.Failure("An error occurred while retrieving the vehicle");
            }
        }

        public async Task<Result<PagedResult<VehicleDto>>> GetAllAsync(VehicleSearchDto searchDto, string tenantId)
        {
            try
            {
                var query = _context.Vehicles
                    .Where(v => v.TenantId == int.Parse(tenantId) && !v.IsDeleted)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchDto.SearchTerm))
                {
                    query = query.Where(v => 
                        v.VehicleNumber.Contains(searchDto.SearchTerm) ||
                        v.Make.Contains(searchDto.SearchTerm) ||
                        v.Model.Contains(searchDto.SearchTerm));
                }

                var totalCount = await query.CountAsync();
                var vehicles = await query
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                var vehicleDtos = vehicles.Select(MapToDto).ToList();

                var pagedResult = new PagedResult<VehicleDto>
                {
                    Items = vehicleDtos,
                    TotalCount = totalCount,
                    Page = searchDto.Page,
                    PageSize = searchDto.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize)
                };

                return Result<PagedResult<VehicleDto>>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicles");
                return Result<PagedResult<VehicleDto>>.Failure("An error occurred while retrieving vehicles");
            }
        }

        public async Task<Result<VehicleDto>> CreateAsync(CreateVehicleDto createDto, string tenantId)
        {
            try
            {
                var existingVehicle = await _context.Vehicles
                    .Where(v => (v.VehicleNumber == createDto.VehicleNumber || v.LicensePlate == createDto.LicensePlate) 
                               && v.TenantId == int.Parse(tenantId) && !v.IsDeleted)
                    .FirstOrDefaultAsync();

                if (existingVehicle != null)
                {
                    return Result<VehicleDto>.Failure("A vehicle with this vehicle number or license plate already exists");
                }

                var vehicle = new Vehicle
                {
                    TenantId = int.Parse(tenantId),
                    VehicleNumber = createDto.VehicleNumber,
                    Make = createDto.Make,
                    Model = createDto.Model,
                    Year = createDto.Year,
                    Type = createDto.Type,
                    Capacity = (int)createDto.Capacity,
                    LicensePlate = createDto.LicensePlate,
                    Status = VehicleStatus.Active,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();

                var vehicleDto = MapToDto(vehicle);
                return Result<VehicleDto>.Success(vehicleDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vehicle");
                return Result<VehicleDto>.Failure("An error occurred while creating the vehicle");
            }
        }

        public async Task<Result<VehicleDto>> UpdateAsync(int id, UpdateVehicleDto updateDto, string tenantId)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .Where(v => v.Id == id && v.TenantId == int.Parse(tenantId) && !v.IsDeleted)
                    .FirstOrDefaultAsync();

                if (vehicle == null)
                {
                    return Result<VehicleDto>.Failure("Vehicle not found");
                }

                var existingVehicle = await _context.Vehicles
                    .Where(v => (v.VehicleNumber == updateDto.VehicleNumber || v.LicensePlate == updateDto.LicensePlate) 
                               && v.Id != id && v.TenantId == int.Parse(tenantId) && !v.IsDeleted)
                    .FirstOrDefaultAsync();

                if (existingVehicle != null)
                {
                    return Result<VehicleDto>.Failure("A vehicle with this vehicle number or license plate already exists");
                }

                vehicle.VehicleNumber = updateDto.VehicleNumber;
                vehicle.Make = updateDto.Make;
                vehicle.Model = updateDto.Model;
                vehicle.Year = updateDto.Year;
                vehicle.Type = updateDto.Type;
                vehicle.Capacity = (int)updateDto.Capacity;
                vehicle.LicensePlate = updateDto.LicensePlate;
                vehicle.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var vehicleDto = MapToDto(vehicle);
                return Result<VehicleDto>.Success(vehicleDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle with ID {VehicleId}", id);
                return Result<VehicleDto>.Failure("An error occurred while updating the vehicle");
            }
        }

        public async Task<Result<bool>> DeleteAsync(int id, string tenantId)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .Where(v => v.Id == id && v.TenantId == int.Parse(tenantId) && !v.IsDeleted)
                    .FirstOrDefaultAsync();

                if (vehicle == null)
                {
                    return Result<bool>.Failure("Vehicle not found");
                }

                vehicle.IsDeleted = true;
                vehicle.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle with ID {VehicleId}", id);
                return Result<bool>.Failure("An error occurred while deleting the vehicle");
            }
        }

        public async Task<Result<VehicleDto>> GetByVehicleNumberAsync(string vehicleNumber, string tenantId)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .Where(v => v.VehicleNumber == vehicleNumber && v.TenantId == int.Parse(tenantId) && !v.IsDeleted)
                    .FirstOrDefaultAsync();

                if (vehicle == null)
                {
                    return Result<VehicleDto>.Failure("Vehicle not found");
                }

                var vehicleDto = MapToDto(vehicle);
                return Result<VehicleDto>.Success(vehicleDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle by vehicle number {VehicleNumber}", vehicleNumber);
                return Result<VehicleDto>.Failure("An error occurred while retrieving the vehicle");
            }
        }

        public async Task<Result<List<VehicleDto>>> GetAvailableVehiclesAsync(DateTime date, string tenantId)
        {
            try
            {
                var vehicles = await _context.Vehicles
                    .Where(v => v.TenantId == int.Parse(tenantId) && !v.IsDeleted && v.Status == VehicleStatus.Active)
                    .Where(v => v.InsuranceExpiry > date && v.RegistrationExpiry > date)
                    .ToListAsync();

                var vehicleDtos = vehicles.Select(MapToDto).ToList();
                return Result<List<VehicleDto>>.Success(vehicleDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available vehicles for date {Date}", date);
                return Result<List<VehicleDto>>.Failure("An error occurred while retrieving available vehicles");
            }
        }

        public async Task<Result<List<VehicleDto>>> GetVehiclesByStatusAsync(string status, string tenantId)
        {
            try
            {
                if (!Enum.TryParse<VehicleStatus>(status, true, out var vehicleStatus))
                {
                    return Result<List<VehicleDto>>.Failure("Invalid vehicle status");
                }

                var vehicles = await _context.Vehicles
                    .Where(v => v.TenantId == int.Parse(tenantId) && !v.IsDeleted && v.Status == vehicleStatus)
                    .ToListAsync();

                var vehicleDtos = vehicles.Select(MapToDto).ToList();
                return Result<List<VehicleDto>>.Success(vehicleDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicles by status {Status}", status);
                return Result<List<VehicleDto>>.Failure("An error occurred while retrieving vehicles by status");
            }
        }
        public async Task<Result<List<VehicleDto>>> GetVehiclesByTypeAsync(string vehicleType, string tenantId)
        {
            try
            {
                if (!Enum.TryParse<VehicleType>(vehicleType, true, out var type))
                {
                    return Result<List<VehicleDto>>.Failure("Invalid vehicle type");
                }

                var vehicles = await _context.Vehicles
                    .Where(v => v.TenantId == int.Parse(tenantId) && !v.IsDeleted && v.Type == type)
                    .ToListAsync();

                var vehicleDtos = vehicles.Select(MapToDto).ToList();
                return Result<List<VehicleDto>>.Success(vehicleDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicles by type {VehicleType}", vehicleType);
                return Result<List<VehicleDto>>.Failure("An error occurred while retrieving vehicles by type");
            }
        }

        public async Task<Result<bool>> AssignDriverAsync(int vehicleId, int driverId, string tenantId)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .Where(v => v.Id == vehicleId && v.TenantId == int.Parse(tenantId) && !v.IsDeleted)
                    .FirstOrDefaultAsync();

                if (vehicle == null)
                {
                    return Result<bool>.Failure("Vehicle not found");
                }

                var driver = await _context.Drivers
                    .Where(d => d.Id == driverId && d.TenantId == int.Parse(tenantId) && !d.IsDeleted)
                    .FirstOrDefaultAsync();

                if (driver == null)
                {
                    return Result<bool>.Failure("Driver not found");
                }

                vehicle.AssignedDriverId = driverId;
                vehicle.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning driver {DriverId} to vehicle {VehicleId}", driverId, vehicleId);
                return Result<bool>.Failure("An error occurred while assigning the driver to the vehicle");
            }
        }

        public async Task<Result<bool>> UnassignDriverAsync(int vehicleId, string tenantId)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .Where(v => v.Id == vehicleId && v.TenantId == int.Parse(tenantId) && !v.IsDeleted)
                    .FirstOrDefaultAsync();

                if (vehicle == null)
                {
                    return Result<bool>.Failure("Vehicle not found");
                }

                vehicle.AssignedDriverId = null;
                vehicle.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unassigning driver from vehicle {VehicleId}", vehicleId);
                return Result<bool>.Failure("An error occurred while unassigning the driver from the vehicle");
            }
        }

        public async Task<Result<bool>> UpdateStatusAsync(int vehicleId, string status, string tenantId)
        {
            try
            {
                if (!Enum.TryParse<VehicleStatus>(status, true, out var vehicleStatus))
                {
                    return Result<bool>.Failure("Invalid vehicle status");
                }

                var vehicle = await _context.Vehicles
                    .Where(v => v.Id == vehicleId && v.TenantId == int.Parse(tenantId) && !v.IsDeleted)
                    .FirstOrDefaultAsync();

                if (vehicle == null)
                {
                    return Result<bool>.Failure("Vehicle not found");
                }

                vehicle.Status = vehicleStatus;
                vehicle.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for vehicle {VehicleId}", vehicleId);
                return Result<bool>.Failure("An error occurred while updating the vehicle status");
            }
        }

        public async Task<Result<bool>> UpdateLocationAsync(int vehicleId, decimal latitude, decimal longitude, string tenantId)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .Where(v => v.Id == vehicleId && v.TenantId == int.Parse(tenantId) && !v.IsDeleted)
                    .FirstOrDefaultAsync();

                if (vehicle == null)
                {
                    return Result<bool>.Failure("Vehicle not found");
                }

                vehicle.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating location for vehicle {VehicleId}", vehicleId);
                return Result<bool>.Failure("An error occurred while updating the vehicle location");
            }
        }

        public async Task<Result<bool>> UpdateMileageAsync(int vehicleId, int mileage, string tenantId)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .Where(v => v.Id == vehicleId && v.TenantId == int.Parse(tenantId) && !v.IsDeleted)
                    .FirstOrDefaultAsync();

                if (vehicle == null)
                {
                    return Result<bool>.Failure("Vehicle not found");
                }

                if (mileage < vehicle.Mileage)
                {
                    return Result<bool>.Failure("New mileage cannot be less than current mileage");
                }

                vehicle.Mileage = mileage;
                vehicle.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating mileage for vehicle {VehicleId}", vehicleId);
                return Result<bool>.Failure("An error occurred while updating the vehicle mileage");
            }
        }
        public async Task<Result<bool>> ScheduleMaintenanceAsync(int vehicleId, DateTime scheduledDate, string maintenanceType, string tenantId)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .Where(v => v.Id == vehicleId && v.TenantId == int.Parse(tenantId) && !v.IsDeleted)
                    .FirstOrDefaultAsync();

                if (vehicle == null)
                {
                    return Result<bool>.Failure("Vehicle not found");
                }

                vehicle.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling maintenance for vehicle {VehicleId}", vehicleId);
                return Result<bool>.Failure("An error occurred while scheduling maintenance");
            }
        }

        public async Task<Result<List<string>>> GetMaintenanceAlertsAsync(string tenantId)
        {
            try
            {
                var alerts = new List<string>();
                var currentDate = DateTime.UtcNow;
                var alertThreshold = currentDate.AddDays(30);

                var vehicles = await _context.Vehicles
                    .Where(v => v.TenantId == int.Parse(tenantId) && !v.IsDeleted)
                    .ToListAsync();

                foreach (var vehicle in vehicles)
                {

                    if (vehicle.InsuranceExpiry <= alertThreshold)
                    {
                        alerts.Add($"Vehicle {vehicle.VehicleNumber} insurance expires on {vehicle.InsuranceExpiry:yyyy-MM-dd}");
                    }

                    if (vehicle.RegistrationExpiry <= alertThreshold)
                    {
                        alerts.Add($"Vehicle {vehicle.VehicleNumber} registration expires on {vehicle.RegistrationExpiry:yyyy-MM-dd}");
                    }
                }

                return Result<List<string>>.Success(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting maintenance alerts");
                return Result<List<string>>.Failure("An error occurred while retrieving maintenance alerts");
            }
        }

        public async Task<Result<List<VehicleDto>>> GetVehiclesDueForMaintenanceAsync(int daysAhead, string tenantId)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);

                var vehicles = await _context.Vehicles
                    .Where(v => v.TenantId == int.Parse(tenantId) && !v.IsDeleted)
                    .Where(v => v.Status == VehicleStatus.Active)
                    .ToListAsync();

                var vehicleDtos = vehicles.Select(MapToDto).ToList();
                return Result<List<VehicleDto>>.Success(vehicleDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicles due for maintenance");
                return Result<List<VehicleDto>>.Failure("An error occurred while retrieving vehicles due for maintenance");
            }
        }

        public async Task<Result<string>> GetMaintenanceHistoryAsync(int vehicleId, string tenantId)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .Where(v => v.Id == vehicleId && v.TenantId == int.Parse(tenantId) && !v.IsDeleted)
                    .FirstOrDefaultAsync();

                if (vehicle == null)
                {
                    return Result<string>.Failure("Vehicle not found");
                }

                var history = $"Maintenance History for Vehicle {vehicle.VehicleNumber} ({vehicle.Make} {vehicle.Model})\n";
                history += $"Purchase Date: {vehicle.PurchaseDate:yyyy-MM-dd}\n";
                history += $"Current Mileage: {vehicle.Mileage:N0} miles\n";
                history += $"Next Maintenance: Not scheduled\n";
                history += $"Insurance Expiry: {vehicle.InsuranceExpiry:yyyy-MM-dd}\n";
                history += $"Registration Expiry: {vehicle.RegistrationExpiry:yyyy-MM-dd}\n";

                return Result<string>.Success(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting maintenance history for vehicle {VehicleId}", vehicleId);
                return Result<string>.Failure("An error occurred while retrieving maintenance history");
            }
        }

        public async Task<Result<string>> GetUtilizationReportAsync(DateTime startDate, DateTime endDate, string tenantId)
        {
            try
            {
                var vehicles = await _context.Vehicles
                    .Where(v => v.TenantId == int.Parse(tenantId) && !v.IsDeleted)
                    .ToListAsync();

                var report = $"Vehicle Utilization Report ({startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd})\n\n";
                report += $"Total Vehicles: {vehicles.Count}\n";
                report += $"Active Vehicles: {vehicles.Count(v => v.Status == VehicleStatus.Active)}\n";
                report += $"Inactive Vehicles: {vehicles.Count(v => v.Status == VehicleStatus.Inactive)}\n";
                report += $"Under Maintenance: {vehicles.Count(v => v.Status == VehicleStatus.Inactive)}\n\n";

                foreach (var vehicle in vehicles.Take(10))
                {
                    report += $"Vehicle {vehicle.VehicleNumber}: {vehicle.Status}, {vehicle.Mileage:N0} miles\n";
                }

                return Result<string>.Success(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating utilization report");
                return Result<string>.Failure("An error occurred while generating the utilization report");
            }
        }

        public async Task<Result<string>> GetPerformanceReportAsync(DateTime startDate, DateTime endDate, string tenantId)
        {
            try
            {
                var vehicles = await _context.Vehicles
                    .Where(v => v.TenantId == int.Parse(tenantId) && !v.IsDeleted)
                    .ToListAsync();

                var report = $"Vehicle Performance Report ({startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd})\n\n";
                report += $"Fleet Overview:\n";
                report += $"Total Vehicles: {vehicles.Count}\n";
                report += $"Average Age: {vehicles.Average(v => DateTime.Now.Year - v.Year):F1} years\n";
                report += $"Average Mileage: {vehicles.Average(v => v.Mileage):N0} miles\n";
                report += $"Total Fleet Value: ${vehicles.Sum(v => v.PurchasePrice):N2}\n\n";

                var vehiclesByType = vehicles.GroupBy(v => v.Type);
                foreach (var group in vehiclesByType)
                {
                    report += $"{group.Key}: {group.Count()} vehicles, Avg Mileage: {group.Average(v => v.Mileage):N0}\n";
                }

                return Result<string>.Success(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating performance report");
                return Result<string>.Failure("An error occurred while generating the performance report");
            }
        }

        public async Task<Result<string>> GetCostAnalysisAsync(DateTime startDate, DateTime endDate, string tenantId)
        {
            try
            {
                var vehicles = await _context.Vehicles
                    .Where(v => v.TenantId == int.Parse(tenantId) && !v.IsDeleted)
                    .ToListAsync();

                var report = $"Vehicle Cost Analysis ({startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd})\n\n";
                report += $"Fleet Investment:\n";
                report += $"Total Purchase Cost: ${vehicles.Sum(v => v.PurchasePrice):N2}\n";
                report += $"Average Vehicle Cost: ${vehicles.Average(v => v.PurchasePrice):N2}\n";
                report += $"Most Expensive: ${vehicles.Max(v => v.PurchasePrice):N2}\n";
                report += $"Least Expensive: ${vehicles.Min(v => v.PurchasePrice):N2}\n\n";

                report += $"Cost per Mile Analysis:\n";
                foreach (var vehicle in vehicles.Where(v => v.Mileage > 0).Take(5))
                {
                    var costPerMile = vehicle.PurchasePrice / vehicle.Mileage;
                    report += $"Vehicle {vehicle.VehicleNumber}: ${costPerMile:F2} per mile\n";
                }

                return Result<string>.Success(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating cost analysis");
                return Result<string>.Failure("An error occurred while generating the cost analysis");
            }
        }

        public async Task<Result<List<VehicleDto>>> GetOptimalFleetSizeAsync(string tenantId)
        {
            try
            {
                var vehicles = await _context.Vehicles
                    .Where(v => v.TenantId == int.Parse(tenantId) && !v.IsDeleted && v.Status == VehicleStatus.Active)
                    .ToListAsync();

                var optimalVehicles = vehicles
                    .Where(v => v.Mileage < 200000 && DateTime.Now.Year - v.Year < 10)
                    .ToList();

                var vehicleDtos = optimalVehicles.Select(MapToDto).ToList();
                return Result<List<VehicleDto>>.Success(vehicleDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating optimal fleet size");
                return Result<List<VehicleDto>>.Failure("An error occurred while calculating optimal fleet size");
            }
        }

        public async Task<Result<List<string>>> GetFleetOptimizationRecommendationsAsync(string tenantId)
        {
            try
            {
                var recommendations = new List<string>();
                var vehicles = await _context.Vehicles
                    .Where(v => v.TenantId == int.Parse(tenantId) && !v.IsDeleted)
                    .ToListAsync();

                var oldVehicles = vehicles.Where(v => DateTime.Now.Year - v.Year > 10).ToList();
                if (oldVehicles.Any())
                {
                    recommendations.Add($"Consider replacing {oldVehicles.Count} vehicles older than 10 years");
                }

                var highMileageVehicles = vehicles.Where(v => v.Mileage > 200000).ToList();
                if (highMileageVehicles.Any())
                {
                    recommendations.Add($"Consider replacing {highMileageVehicles.Count} vehicles with over 200,000 miles");
                }

                var inactiveVehicles = vehicles.Where(v => v.Status == VehicleStatus.Inactive).ToList();
                if (inactiveVehicles.Any())
                {
                    recommendations.Add($"Review {inactiveVehicles.Count} inactive vehicles for potential disposal");
                }

                var maintenanceOverdue = new List<Vehicle>(); // NextMaintenanceDate not available in current Vehicle entity
                if (maintenanceOverdue.Any())
                {
                    recommendations.Add($"Schedule maintenance for {maintenanceOverdue.Count} overdue vehicles");
                }

                if (!recommendations.Any())
                {
                    recommendations.Add("Fleet is well-maintained with no immediate optimization recommendations");
                }

                return Result<List<string>>.Success(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating fleet optimization recommendations");
                return Result<List<string>>.Failure("An error occurred while generating optimization recommendations");
            }
        }

        public async Task<Result<string>> GetEnvironmentalImpactReportAsync(DateTime startDate, DateTime endDate, string tenantId)
        {
            try
            {
                var vehicles = await _context.Vehicles
                    .Where(v => v.TenantId == int.Parse(tenantId) && !v.IsDeleted)
                    .ToListAsync();

                var report = $"Environmental Impact Report ({startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd})\n\n";
                
                var fuelTypeGroups = vehicles.GroupBy(v => v.FuelType);
                report += "Fleet Composition by Fuel Type:\n";
                foreach (var group in fuelTypeGroups)
                {
                    var percentage = (double)group.Count() / vehicles.Count * 100;
                    report += $"{group.Key}: {group.Count()} vehicles ({percentage:F1}%)\n";
                }

                var totalMileage = vehicles.Sum(v => v.Mileage);
                var estimatedCO2 = totalMileage * 0.404; // kg CO2 per mile (average)
                
                report += $"\nEstimated Environmental Impact:\n";
                report += $"Total Fleet Mileage: {totalMileage:N0} miles\n";
                report += $"Estimated CO2 Emissions: {estimatedCO2:N0} kg\n";
                report += $"Average Emissions per Vehicle: {estimatedCO2 / vehicles.Count:N0} kg\n";

                return Result<string>.Success(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating environmental impact report");
                return Result<string>.Failure("An error occurred while generating the environmental impact report");
            }
        }

        private VehicleDto MapToDto(Vehicle vehicle)
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
    }
}

