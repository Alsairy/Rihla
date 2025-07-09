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
    public class MaintenanceService : IMaintenanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MaintenanceService> _logger;

        public MaintenanceService(IUnitOfWork unitOfWork, ILogger<MaintenanceService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<MaintenanceRecordDto>> GetByIdAsync(int id, string tenantId)
        {
            try
            {
                var maintenanceRecord = await _unitOfWork.MaintenanceRecords
                    .QueryWithIncludes(tenantId, m => m.Vehicle)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (maintenanceRecord == null)
                {
                    return Result<MaintenanceRecordDto>.Failure("Maintenance record not found");
                }

                var maintenanceDto = MapToDto(maintenanceRecord);
                return Result<MaintenanceRecordDto>.Success(maintenanceDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting maintenance record by ID {MaintenanceId}", id);
                return Result<MaintenanceRecordDto>.Failure("An error occurred while retrieving the maintenance record");
            }
        }

        public async Task<Result<PagedResult<MaintenanceRecordDto>>> GetAllAsync(MaintenanceSearchDto searchDto, string tenantId)
        {
            try
            {
                var query = _unitOfWork.MaintenanceRecords
                    .QueryWithIncludes(tenantId, m => m.Vehicle);

                if (searchDto.VehicleId.HasValue)
                    query = query.Where(m => m.VehicleId == searchDto.VehicleId.Value);

                if (!string.IsNullOrEmpty(searchDto.VehicleNumber))
                    query = query.Where(m => m.Vehicle.VehicleNumber.Contains(searchDto.VehicleNumber));

                if (searchDto.Type.HasValue)
                    query = query.Where(m => m.MaintenanceType.Contains(searchDto.Type.Value.ToString()));

                if (searchDto.ScheduledDateFrom.HasValue)
                    query = query.Where(m => m.ScheduledDate.Date >= searchDto.ScheduledDateFrom.Value.Date);

                if (searchDto.ScheduledDateTo.HasValue)
                    query = query.Where(m => m.ScheduledDate.Date <= searchDto.ScheduledDateTo.Value.Date);

                if (searchDto.CompletedDateFrom.HasValue)
                    query = query.Where(m => m.CompletedDate.HasValue && m.CompletedDate.Value.Date >= searchDto.CompletedDateFrom.Value.Date);

                if (searchDto.CompletedDateTo.HasValue)
                    query = query.Where(m => m.CompletedDate.HasValue && m.CompletedDate.Value.Date <= searchDto.CompletedDateTo.Value.Date);

                if (searchDto.CostFrom.HasValue)
                    query = query.Where(m => m.Cost >= searchDto.CostFrom.Value);

                if (searchDto.CostTo.HasValue)
                    query = query.Where(m => m.Cost <= searchDto.CostTo.Value);

                if (!string.IsNullOrEmpty(searchDto.ServiceProvider))
                    query = query.Where(m => m.ServiceProvider != null && m.ServiceProvider.Contains(searchDto.ServiceProvider));

                if (searchDto.IsOverdue.HasValue && searchDto.IsOverdue.Value)
                    query = query.Where(m => !m.IsCompleted && m.ScheduledDate < DateTime.Today);

                var totalCount = await query.CountAsync();

                var maintenanceRecords = await query
                    .OrderByDescending(m => m.CreatedAt)
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                var maintenanceDtos = maintenanceRecords.Select(MapToDto).ToList();

                var pagedResult = new PagedResult<MaintenanceRecordDto>
                {
                    Items = maintenanceDtos,
                    TotalCount = totalCount,
                    Page = searchDto.Page,
                    PageSize = searchDto.PageSize
                };

                return Result<PagedResult<MaintenanceRecordDto>>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting maintenance records");
                return Result<PagedResult<MaintenanceRecordDto>>.Failure("An error occurred while retrieving maintenance records");
            }
        }

        public async Task<Result<MaintenanceRecordDto>> CreateAsync(CreateMaintenanceRecordDto createDto, string tenantId)
        {
            try
            {
                var vehicle = await _unitOfWork.Vehicles
                    .GetByIdAsync(createDto.VehicleId, tenantId);

                if (vehicle == null)
                {
                    return Result<MaintenanceRecordDto>.Failure("Vehicle not found");
                }

                var maintenanceRecord = new MaintenanceRecord
                {
                    TenantId = int.Parse(tenantId),
                    VehicleId = createDto.VehicleId,
                    MaintenanceType = createDto.Type.ToString(),
                    Description = createDto.Description,
                    ScheduledDate = createDto.ScheduledDate,
                    Cost = createDto.Cost,
                    Currency = "USD",
                    ServiceProvider = createDto.ServiceProvider,
                    InvoiceNumber = createDto.InvoiceNumber,
                    MileageAtService = (int?)createDto.Mileage,
                    PartsReplaced = createDto.PartsReplaced,
                    Notes = createDto.Notes,
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.MaintenanceRecords.AddAsync(maintenanceRecord);
                await _unitOfWork.SaveChangesAsync();

                var maintenanceDto = MapToDto(maintenanceRecord);
                return Result<MaintenanceRecordDto>.Success(maintenanceDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating maintenance record");
                return Result<MaintenanceRecordDto>.Failure("An error occurred while creating the maintenance record");
            }
        }

        public async Task<Result<MaintenanceRecordDto>> UpdateAsync(int id, UpdateMaintenanceRecordDto updateDto, string tenantId)
        {
            try
            {
                var maintenanceRecord = await _unitOfWork.MaintenanceRecords
                    .GetByIdAsync(id, tenantId);

                if (maintenanceRecord == null)
                {
                    return Result<MaintenanceRecordDto>.Failure("Maintenance record not found");
                }

                maintenanceRecord.MaintenanceType = updateDto.Type.ToString();
                maintenanceRecord.Description = updateDto.Description;
                maintenanceRecord.ScheduledDate = updateDto.ScheduledDate;
                maintenanceRecord.CompletedDate = updateDto.CompletedDate;
                maintenanceRecord.Cost = updateDto.Cost;
                maintenanceRecord.ServiceProvider = updateDto.ServiceProvider;
                maintenanceRecord.InvoiceNumber = updateDto.InvoiceNumber;
                maintenanceRecord.MileageAtService = (int?)updateDto.Mileage;
                maintenanceRecord.PartsReplaced = updateDto.PartsReplaced;
                maintenanceRecord.Notes = updateDto.Notes;
                maintenanceRecord.IsCompleted = updateDto.Status == MaintenanceStatus.Completed;
                maintenanceRecord.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                var maintenanceDto = MapToDto(maintenanceRecord);
                return Result<MaintenanceRecordDto>.Success(maintenanceDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating maintenance record with ID {MaintenanceId}", id);
                return Result<MaintenanceRecordDto>.Failure("An error occurred while updating the maintenance record");
            }
        }

        public async Task<Result<bool>> DeleteAsync(int id, string tenantId)
        {
            try
            {
                var maintenanceRecord = await _unitOfWork.MaintenanceRecords
                    .GetByIdAsync(id, tenantId);

                if (maintenanceRecord == null)
                {
                    return Result<bool>.Failure("Maintenance record not found");
                }

                maintenanceRecord.IsDeleted = true;
                maintenanceRecord.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting maintenance record with ID {MaintenanceId}", id);
                return Result<bool>.Failure("An error occurred while deleting the maintenance record");
            }
        }

        public async Task<Result<List<MaintenanceRecordDto>>> GetMaintenanceByVehicleAsync(int vehicleId, DateTime startDate, DateTime endDate, string tenantId)
        {
            try
            {
                var maintenanceRecords = await _unitOfWork.MaintenanceRecords
                    .QueryWithIncludes(tenantId, m => m.Vehicle)
                    .Where(m => m.VehicleId == vehicleId)
                    .Where(m => m.ScheduledDate.Date >= startDate.Date && m.ScheduledDate.Date <= endDate.Date)
                    .OrderByDescending(m => m.ScheduledDate)
                    .ToListAsync();

                var maintenanceDtos = maintenanceRecords.Select(MapToDto).ToList();
                return Result<List<MaintenanceRecordDto>>.Success(maintenanceDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting maintenance records by vehicle {VehicleId} from {StartDate} to {EndDate}", vehicleId, startDate, endDate);
                return Result<List<MaintenanceRecordDto>>.Failure("An error occurred while retrieving vehicle maintenance records");
            }
        }

        public async Task<Result<List<MaintenanceRecordDto>>> GetOverdueMaintenanceAsync(string tenantId)
        {
            try
            {
                var today = DateTime.Today;
                var maintenanceRecords = await _unitOfWork.MaintenanceRecords
                    .QueryWithIncludes(tenantId, m => m.Vehicle)
                    .Where(m => !m.IsCompleted && m.ScheduledDate < today)
                    .OrderBy(m => m.ScheduledDate)
                    .ToListAsync();

                var maintenanceDtos = maintenanceRecords.Select(MapToDto).ToList();
                return Result<List<MaintenanceRecordDto>>.Success(maintenanceDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overdue maintenance records");
                return Result<List<MaintenanceRecordDto>>.Failure("An error occurred while retrieving overdue maintenance records");
            }
        }

        public async Task<Result<bool>> ScheduleMaintenanceAsync(int vehicleId, string maintenanceType, DateTime scheduledDate, string tenantId)
        {
            try
            {
                var vehicle = await _unitOfWork.Vehicles
                    .GetByIdAsync(vehicleId, tenantId);

                if (vehicle == null)
                {
                    return Result<bool>.Failure("Vehicle not found");
                }

                var maintenanceRecord = new MaintenanceRecord
                {
                    TenantId = int.Parse(tenantId),
                    VehicleId = vehicleId,
                    MaintenanceType = maintenanceType,
                    Description = $"Scheduled {maintenanceType} maintenance",
                    ScheduledDate = scheduledDate,
                    Cost = 0,
                    Currency = "USD",
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.MaintenanceRecords.AddAsync(maintenanceRecord);
                await _unitOfWork.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling maintenance for vehicle {VehicleId}", vehicleId);
                return Result<bool>.Failure("An error occurred while scheduling maintenance");
            }
        }

        private MaintenanceRecordDto MapToDto(MaintenanceRecord maintenanceRecord)
        {
            return new MaintenanceRecordDto
            {
                Id = maintenanceRecord.Id,
                VehicleId = maintenanceRecord.VehicleId,
                Type = Enum.TryParse<MaintenanceType>(maintenanceRecord.MaintenanceType, out var type) ? type : MaintenanceType.Preventive,
                Status = maintenanceRecord.IsCompleted ? MaintenanceStatus.Completed : MaintenanceStatus.Scheduled,
                Description = maintenanceRecord.Description,
                ScheduledDate = maintenanceRecord.ScheduledDate,
                CompletedDate = maintenanceRecord.CompletedDate,
                Mileage = maintenanceRecord.MileageAtService ?? 0,
                Cost = maintenanceRecord.Cost,
                ServiceProvider = maintenanceRecord.ServiceProvider,
                TechnicianName = null, // Not available in entity
                PartsReplaced = maintenanceRecord.PartsReplaced,
                WorkPerformed = null, // Not available in entity
                Notes = maintenanceRecord.Notes,
                NextServiceDate = null, // Not available in entity
                NextServiceMileage = null, // Not available in entity
                WarrantyInfo = null, // Not available in entity
                InvoiceNumber = maintenanceRecord.InvoiceNumber,
                IsWarrantyWork = false, // Not available in entity
                CreatedAt = maintenanceRecord.CreatedAt,
                UpdatedAt = maintenanceRecord.UpdatedAt,
                TenantId = maintenanceRecord.TenantId.ToString(),
                Vehicle = maintenanceRecord.Vehicle != null ? MapVehicleToDto(maintenanceRecord.Vehicle) : new VehicleDto()
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
                LicensePlate = vehicle.LicensePlate,
                VIN = vehicle.VIN,
                Capacity = vehicle.Capacity,
                FuelType = vehicle.FuelType,
                Status = vehicle.Status,
                PurchaseDate = vehicle.PurchaseDate,
                PurchasePrice = vehicle.PurchasePrice,
                Mileage = vehicle.Mileage,
                InsuranceExpiry = vehicle.InsuranceExpiry,
                RegistrationExpiry = vehicle.RegistrationExpiry,
                Notes = vehicle.Notes,
                CreatedAt = vehicle.CreatedAt,
                UpdatedAt = vehicle.UpdatedAt
            };
        }
    }
}

