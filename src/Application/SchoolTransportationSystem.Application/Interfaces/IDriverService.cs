using SchoolTransportationSystem.Application.DTOs;
using SchoolTransportationSystem.Core.Common;

namespace SchoolTransportationSystem.Application.Interfaces
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

