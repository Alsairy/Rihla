using Rihla.Application.DTOs;
using Rihla.Core.Common;

namespace Rihla.Application.Interfaces
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
    }
}

