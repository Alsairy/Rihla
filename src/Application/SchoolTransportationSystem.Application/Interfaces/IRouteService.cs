using SchoolTransportationSystem.Application.DTOs;
using SchoolTransportationSystem.Core.Common;

namespace SchoolTransportationSystem.Application.Interfaces
{
    public interface IRouteService
    {
        Task<Result<RouteDto>> GetByIdAsync(int id, string tenantId);
        Task<Result<PagedResult<RouteDto>>> GetAllAsync(RouteSearchDto searchDto, string tenantId);
        Task<Result<RouteDto>> CreateAsync(CreateRouteDto createDto, string tenantId);
        Task<Result<RouteDto>> UpdateAsync(int id, UpdateRouteDto updateDto, string tenantId);
        Task<Result<bool>> DeleteAsync(int id, string tenantId);
        Task<Result<RouteDto>> GetByRouteNumberAsync(string routeNumber, string tenantId);
        Task<Result<List<RouteDto>>> GetActiveRoutesAsync(string tenantId);
        Task<Result<List<StudentDto>>> GetStudentsOnRouteAsync(int routeId, string tenantId);
        Task<Result<RouteDto>> GenerateOptimalRouteAsync(List<int> studentIds, int vehicleId, string tenantId);
        Task<Result<RouteDto>> OptimizeExistingRouteAsync(RouteOptimizationRequestDto request, string tenantId);
        Task<Result<RoutePerformanceDto>> CalculateRouteEfficiencyMetricsAsync(int routeId, DateTime startDate, DateTime endDate, string tenantId);
        Task<Result<List<RouteOptimizationDto>>> GetOptimizationHistoryAsync(int routeId, string tenantId);
    }
}

