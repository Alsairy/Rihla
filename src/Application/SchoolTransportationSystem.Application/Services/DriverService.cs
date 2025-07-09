using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rihla.Application.DTOs;
using Rihla.Application.Interfaces;
using Rihla.Core.Common;
using Rihla.Core.Entities;
using Rihla.Core.Enums;
using Rihla.Core.ValueObjects;
using Rihla.Core.Interfaces;

namespace Rihla.Application.Services
{
    public class DriverService : IDriverService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DriverService> _logger;

        public DriverService(IUnitOfWork unitOfWork, ILogger<DriverService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<DriverDto>> GetByIdAsync(int id, string tenantId)
        {
            try
            {
                var driver = await _unitOfWork.Drivers
                    .GetByIdAsync(id, tenantId);

                if (driver == null)
                {
                    return Result<DriverDto>.Failure("Driver not found");
                }

                var driverDto = MapToDto(driver);
                return Result<DriverDto>.Success(driverDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting driver by ID {DriverId}", id);
                return Result<DriverDto>.Failure("An error occurred while retrieving the driver");
            }
        }

        public async Task<Result<PagedResult<DriverDto>>> GetAllAsync(DriverSearchDto searchDto, string tenantId)
        {
            try
            {
                var query = _unitOfWork.Drivers
                    .Query(tenantId);

                if (!string.IsNullOrEmpty(searchDto.SearchTerm))
                {
                    query = query.Where(d => 
                        d.FullName.FirstName.Contains(searchDto.SearchTerm) ||
                        d.FullName.LastName.Contains(searchDto.SearchTerm) ||
                        d.LicenseNumber.Contains(searchDto.SearchTerm));
                }

                var totalCount = await query.CountAsync();
                var drivers = await query
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                var driverDtos = drivers.Select(MapToDto).ToList();

                var pagedResult = new PagedResult<DriverDto>
                {
                    Items = driverDtos,
                    TotalCount = totalCount,
                    Page = searchDto.Page,
                    PageSize = searchDto.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize)
                };

                return Result<PagedResult<DriverDto>>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drivers");
                return Result<PagedResult<DriverDto>>.Failure("An error occurred while retrieving drivers");
            }
        }

        public async Task<Result<DriverDto>> CreateAsync(CreateDriverDto createDto, string tenantId)
        {
            try
            {
                var existingDriver = await _unitOfWork.Drivers
                    .Query(tenantId)
                    .FirstOrDefaultAsync(d => d.LicenseNumber == createDto.LicenseNumber);

                if (existingDriver != null)
                {
                    return Result<DriverDto>.Failure("A driver with this license number already exists");
                }

                var driver = new Driver
                {
                    TenantId = int.Parse(tenantId),
                    EmployeeNumber = createDto.EmployeeNumber,
                    FullName = new FullName(createDto.FirstName, createDto.LastName, createDto.MiddleName),
                    LicenseNumber = createDto.LicenseNumber,
                    LicenseExpiry = createDto.LicenseExpiry,
                    Phone = createDto.Phone,
                    Email = createDto.Email,
                    Address = new Address(createDto.Street, createDto.City, createDto.State, createDto.ZipCode, createDto.Country),
                    HireDate = createDto.HireDate,
                    DateOfBirth = createDto.DateOfBirth,
                    Status = createDto.Status,
                    EmergencyContact = createDto.EmergencyContact,
                    EmergencyPhone = createDto.EmergencyPhone,
                    MedicalCertExpiry = createDto.MedicalCertExpiry,
                    BackgroundCheckDate = createDto.BackgroundCheckDate,
                    LastTrainingDate = createDto.LastTrainingDate,
                    Notes = createDto.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Drivers.AddAsync(driver);
                await _unitOfWork.SaveChangesAsync();

                var driverDto = MapToDto(driver);
                return Result<DriverDto>.Success(driverDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating driver");
                return Result<DriverDto>.Failure("An error occurred while creating the driver");
            }
        }

        public async Task<Result<DriverDto>> UpdateAsync(int id, UpdateDriverDto updateDto, string tenantId)
        {
            try
            {
                var driver = await _unitOfWork.Drivers
                    .GetByIdAsync(id, tenantId);

                if (driver == null)
                {
                    return Result<DriverDto>.Failure("Driver not found");
                }

                var existingDriver = await _unitOfWork.Drivers
                    .Query(tenantId)
                    .FirstOrDefaultAsync(d => d.LicenseNumber == updateDto.LicenseNumber && d.Id != id);

                if (existingDriver != null)
                {
                    return Result<DriverDto>.Failure("A driver with this license number already exists");
                }

                driver.FullName = new FullName(updateDto.FirstName, updateDto.LastName, updateDto.MiddleName);
                driver.LicenseNumber = updateDto.LicenseNumber;
                driver.LicenseExpiry = updateDto.LicenseExpiry;
                driver.Phone = updateDto.Phone;
                driver.Email = updateDto.Email;
                driver.Address = new Address(updateDto.Street, updateDto.City, updateDto.State, updateDto.ZipCode, updateDto.Country);
                driver.DateOfBirth = updateDto.DateOfBirth;
                driver.Status = updateDto.Status;
                driver.EmergencyContact = updateDto.EmergencyContact;
                driver.EmergencyPhone = updateDto.EmergencyPhone;
                driver.MedicalCertExpiry = updateDto.MedicalCertExpiry;
                driver.BackgroundCheckDate = updateDto.BackgroundCheckDate;
                driver.LastTrainingDate = updateDto.LastTrainingDate;
                driver.Notes = updateDto.Notes;
                driver.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                var driverDto = MapToDto(driver);
                return Result<DriverDto>.Success(driverDto);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency conflict when updating driver with ID {DriverId}", id);
                return Result<DriverDto>.Failure("The driver was modified by another user. Please refresh and try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating driver with ID {DriverId}", id);
                return Result<DriverDto>.Failure("An error occurred while updating the driver");
            }
        }

        public async Task<Result<bool>> DeleteAsync(int id, string tenantId)
        {
            try
            {
                var driver = await _unitOfWork.Drivers
                    .GetByIdAsync(id, tenantId);

                if (driver == null)
                {
                    return Result<bool>.Failure("Driver not found");
                }

                driver.IsDeleted = true;
                driver.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting driver with ID {DriverId}", id);
                return Result<bool>.Failure("An error occurred while deleting the driver");
            }
        }

        public async Task<Result<DriverDto>> GetByLicenseNumberAsync(string licenseNumber, string tenantId)
        {
            try
            {
                var driver = await _unitOfWork.Drivers
                    .Query(tenantId)
                    .FirstOrDefaultAsync(d => d.LicenseNumber == licenseNumber);

                if (driver == null)
                {
                    return Result<DriverDto>.Failure("Driver not found");
                }

                var driverDto = MapToDto(driver);
                return Result<DriverDto>.Success(driverDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting driver by license number {LicenseNumber}", licenseNumber);
                return Result<DriverDto>.Failure("An error occurred while retrieving the driver");
            }
        }

        public async Task<Result<List<DriverDto>>> GetAvailableDriversAsync(DateTime date, string tenantId)
        {
            try
            {
                var drivers = await _unitOfWork.Drivers
                    .Query(tenantId)
                    .Where(d => d.Status == DriverStatus.Active && d.LicenseExpiry > date)
                    .ToListAsync();

                var driverDtos = drivers.Select(MapToDto).ToList();
                return Result<List<DriverDto>>.Success(driverDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available drivers for date {Date}", date);
                return Result<List<DriverDto>>.Failure("An error occurred while retrieving available drivers");
            }
        }

        public async Task<Result<List<DriverDto>>> GetDriversByStatusAsync(string status, string tenantId)
        {
            try
            {
                if (!Enum.TryParse<DriverStatus>(status, true, out var driverStatus))
                {
                    return Result<List<DriverDto>>.Failure("Invalid driver status");
                }

                var drivers = await _unitOfWork.Drivers
                    .Query(tenantId)
                    .Where(d => d.Status == driverStatus)
                    .ToListAsync();

                var driverDtos = drivers.Select(MapToDto).ToList();
                return Result<List<DriverDto>>.Success(driverDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drivers by status {Status}", status);
                return Result<List<DriverDto>>.Failure("An error occurred while retrieving drivers by status");
            }
        }

        private DriverDto MapToDto(Driver driver)
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
    }
}

