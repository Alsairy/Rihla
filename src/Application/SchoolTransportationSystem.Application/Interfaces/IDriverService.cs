using Rihla.Application.DTOs;
using Rihla.Core.Common;

namespace Rihla.Application.Interfaces
{
    public interface IDriverService
    {
        Task<Result<DriverDto>> GetByIdAsync(int id, string tenantId);
        Task<Result<PagedResult<DriverDto>>> GetAllAsync(DriverSearchDto searchDto, string tenantId);
        Task<Result<DriverDto>> CreateAsync(CreateDriverDto createDto, string tenantId);
        Task<Result<DriverDto>> UpdateAsync(int id, UpdateDriverDto updateDto, string tenantId);
        Task<Result<bool>> DeleteAsync(int id, string tenantId);
        Task<Result<DriverDto>> GetByLicenseNumberAsync(string licenseNumber, string tenantId);
        Task<Result<List<DriverDto>>> GetAvailableDriversAsync(DateTime date, string tenantId);
        Task<Result<List<DriverDto>>> GetDriversByStatusAsync(string status, string tenantId);
    }
}

