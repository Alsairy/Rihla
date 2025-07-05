using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rihla.Application.DTOs;
using Rihla.Application.Interfaces;
using Rihla.Core.Common;
using Rihla.Core.Entities;
using Rihla.Infrastructure.Data;

namespace Rihla.Application.Services
{
    public class DriverService : IDriverService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DriverService> _logger;

        public DriverService(ApplicationDbContext context, ILogger<DriverService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<DriverDto>> GetByIdAsync(int id, string tenantId)
        {
            try
            {
                var driver = await _context.Drivers
                    .Where(d => d.Id == id && d.TenantId == int.Parse(tenantId) && !d.IsDeleted)
                    .FirstOrDefaultAsync();

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
                var query = _context.Drivers
                    .Where(d => d.TenantId == int.Parse(tenantId) && !d.IsDeleted)
                    .AsQueryable();

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

        // Placeholder implementations for interface compliance
        public Task<Result<DriverDto>> CreateAsync(CreateDriverDto createDto, string tenantId) => throw new NotImplementedException();
        public Task<Result<DriverDto>> UpdateAsync(int id, UpdateDriverDto updateDto, string tenantId) => throw new NotImplementedException();
        public Task<Result<bool>> DeleteAsync(int id, string tenantId) => throw new NotImplementedException();
        public Task<Result<DriverDto>> GetByLicenseNumberAsync(string licenseNumber, string tenantId) => throw new NotImplementedException();
        public Task<Result<List<DriverDto>>> GetAvailableDriversAsync(DateTime date, string tenantId) => throw new NotImplementedException();
        public Task<Result<List<DriverDto>>> GetDriversByStatusAsync(string status, string tenantId) => throw new NotImplementedException();

        private DriverDto MapToDto(Driver driver)
        {
            return new DriverDto
            {
                Id = driver.Id,
                TenantId = driver.TenantId,
                FirstName = driver.FullName.FirstName,
                LastName = driver.FullName.LastName,
                Email = driver.Email,
                Phone = driver.Phone,
                LicenseNumber = driver.LicenseNumber,
                LicenseExpiry = driver.LicenseExpiry,
                HireDate = driver.HireDate,
                Status = driver.Status,
                EmergencyContact = driver.EmergencyContact,
                EmergencyPhone = driver.EmergencyPhone,
                Notes = driver.Notes,
                CreatedAt = driver.CreatedAt,
                UpdatedAt = driver.UpdatedAt
            };
        }
    }
}

