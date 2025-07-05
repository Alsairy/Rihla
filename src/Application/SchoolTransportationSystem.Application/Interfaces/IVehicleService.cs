using Rihla.Application.DTOs;
using Rihla.Core.Common;

namespace Rihla.Application.Interfaces
{
    public interface IVehicleService
    {
        Task<Result<VehicleDto>> GetByIdAsync(int id, string tenantId);
        Task<Result<PagedResult<VehicleDto>>> GetAllAsync(VehicleSearchDto searchDto, string tenantId);
        Task<Result<VehicleDto>> CreateAsync(CreateVehicleDto createDto, string tenantId);
        Task<Result<VehicleDto>> UpdateAsync(int id, UpdateVehicleDto updateDto, string tenantId);
        Task<Result<bool>> DeleteAsync(int id, string tenantId);
        Task<Result<VehicleDto>> GetByVehicleNumberAsync(string vehicleNumber, string tenantId);
        Task<Result<List<VehicleDto>>> GetAvailableVehiclesAsync(DateTime date, string tenantId);
        Task<Result<List<VehicleDto>>> GetVehiclesByStatusAsync(string status, string tenantId);
    }
}

