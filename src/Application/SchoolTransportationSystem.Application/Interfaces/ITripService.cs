using SchoolTransportationSystem.Application.DTOs;
using SchoolTransportationSystem.Core.Common;

namespace SchoolTransportationSystem.Application.Interfaces
{
    public interface ITripService
    {
        Task<Result<TripDto>> GetByIdAsync(int id, string tenantId);
        Task<Result<PagedResult<TripDto>>> GetAllAsync(TripSearchDto searchDto, string tenantId);
        Task<Result<TripDto>> CreateAsync(CreateTripDto createDto, string tenantId);
        Task<Result<TripDto>> UpdateAsync(int id, UpdateTripDto updateDto, string tenantId);
        Task<Result<bool>> DeleteAsync(int id, string tenantId);
        Task<Result<List<TripDto>>> GetTripsByRouteAsync(int routeId, DateTime date, string tenantId);
        Task<Result<List<TripDto>>> GetActiveTripsByDateAsync(DateTime date, string tenantId);
        Task<Result<bool>> StartTripAsync(int tripId, string tenantId);
        Task<Result<bool>> EndTripAsync(int tripId, string tenantId);
    }
}

