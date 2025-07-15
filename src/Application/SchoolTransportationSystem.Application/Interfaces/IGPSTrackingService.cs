using SchoolTransportationSystem.Application.DTOs;
using SchoolTransportationSystem.Core.Common;

namespace SchoolTransportationSystem.Application.Interfaces
{
    public interface IGPSTrackingService
    {
        Task<Result<bool>> StartRealTimeTrackingAsync(int vehicleId, int tripId, string tenantId);
        Task<Result<bool>> UpdateVehicleLocationAsync(int vehicleId, double latitude, double longitude, DateTime timestamp, string tenantId);
        Task<Result<List<GeofenceViolationDto>>> CheckGeofenceViolationsAsync(int vehicleId, double latitude, double longitude, string tenantId);
        Task<Result<EstimatedArrivalDto>> GetEstimatedArrivalTimeAsync(int tripId, int stopId, string tenantId);
        Task<Result<List<VehicleLocationDto>>> GetVehicleLocationHistoryAsync(int vehicleId, DateTime startTime, DateTime endTime, string tenantId);
        Task<Result<bool>> StopRealTimeTrackingAsync(int vehicleId, string tenantId);
        Task<Result<List<VehicleLocationDto>>> GetActiveVehicleLocationsAsync(string tenantId);
    }
}
