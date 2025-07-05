using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rihla.Application.DTOs;
using Rihla.Application.Interfaces;
using Rihla.Core.Common;
using Rihla.Core.Entities;
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

        // Placeholder implementations for interface compliance
        public Task<Result<VehicleDto>> CreateAsync(CreateVehicleDto createDto, string tenantId) => throw new NotImplementedException();
        public Task<Result<VehicleDto>> UpdateAsync(int id, UpdateVehicleDto updateDto, string tenantId) => throw new NotImplementedException();
        public Task<Result<bool>> DeleteAsync(int id, string tenantId) => throw new NotImplementedException();
        public Task<Result<VehicleDto>> GetByVehicleNumberAsync(string vehicleNumber, string tenantId) => throw new NotImplementedException();
        public Task<Result<List<VehicleDto>>> GetAvailableVehiclesAsync(DateTime date, string tenantId) => throw new NotImplementedException();
        public Task<Result<List<VehicleDto>>> GetVehiclesByStatusAsync(string status, string tenantId) => throw new NotImplementedException();
        public Task<Result<List<VehicleDto>>> GetVehiclesByTypeAsync(string vehicleType, string tenantId) => throw new NotImplementedException();
        public Task<Result<bool>> AssignDriverAsync(int vehicleId, int driverId, string tenantId) => throw new NotImplementedException();
        public Task<Result<bool>> UnassignDriverAsync(int vehicleId, string tenantId) => throw new NotImplementedException();
        public Task<Result<bool>> UpdateStatusAsync(int vehicleId, string status, string tenantId) => throw new NotImplementedException();
        public Task<Result<bool>> UpdateLocationAsync(int vehicleId, decimal latitude, decimal longitude, string tenantId) => throw new NotImplementedException();
        public Task<Result<bool>> UpdateMileageAsync(int vehicleId, int mileage, string tenantId) => throw new NotImplementedException();
        public Task<Result<bool>> ScheduleMaintenanceAsync(int vehicleId, DateTime scheduledDate, string maintenanceType, string tenantId) => throw new NotImplementedException();
        public Task<Result<List<string>>> GetMaintenanceAlertsAsync(string tenantId) => throw new NotImplementedException();
        public Task<Result<List<VehicleDto>>> GetVehiclesDueForMaintenanceAsync(int daysAhead, string tenantId) => throw new NotImplementedException();
        public Task<Result<string>> GetMaintenanceHistoryAsync(int vehicleId, string tenantId) => throw new NotImplementedException();
        public Task<Result<string>> GetUtilizationReportAsync(DateTime startDate, DateTime endDate, string tenantId) => throw new NotImplementedException();
        public Task<Result<string>> GetPerformanceReportAsync(DateTime startDate, DateTime endDate, string tenantId) => throw new NotImplementedException();
        public Task<Result<string>> GetCostAnalysisAsync(DateTime startDate, DateTime endDate, string tenantId) => throw new NotImplementedException();
        public Task<Result<List<VehicleDto>>> GetOptimalFleetSizeAsync(string tenantId) => throw new NotImplementedException();
        public Task<Result<List<string>>> GetFleetOptimizationRecommendationsAsync(string tenantId) => throw new NotImplementedException();
        public Task<Result<string>> GetEnvironmentalImpactReportAsync(DateTime startDate, DateTime endDate, string tenantId) => throw new NotImplementedException();

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

