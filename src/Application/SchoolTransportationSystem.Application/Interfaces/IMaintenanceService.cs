using Rihla.Application.DTOs;
using Rihla.Core.Common;

namespace Rihla.Application.Interfaces
{
    public interface IMaintenanceService
    {
        Task<Result<MaintenanceRecordDto>> GetByIdAsync(int id, string tenantId);
        Task<Result<PagedResult<MaintenanceRecordDto>>> GetAllAsync(MaintenanceSearchDto searchDto, string tenantId);
        Task<Result<MaintenanceRecordDto>> CreateAsync(CreateMaintenanceRecordDto createDto, string tenantId);
        Task<Result<MaintenanceRecordDto>> UpdateAsync(int id, UpdateMaintenanceRecordDto updateDto, string tenantId);
        Task<Result<bool>> DeleteAsync(int id, string tenantId);
        Task<Result<List<MaintenanceRecordDto>>> GetMaintenanceByVehicleAsync(int vehicleId, DateTime startDate, DateTime endDate, string tenantId);
        Task<Result<List<MaintenanceRecordDto>>> GetOverdueMaintenanceAsync(string tenantId);
        Task<Result<bool>> ScheduleMaintenanceAsync(int vehicleId, string maintenanceType, DateTime scheduledDate, string tenantId);
    }
}

