using SchoolTransportationSystem.Application.DTOs;
using SchoolTransportationSystem.Core.Common;

namespace SchoolTransportationSystem.Application.Interfaces
{
    public interface IAttendanceService
    {
        Task<Result<AttendanceDto>> GetByIdAsync(int id, string tenantId);
        Task<Result<PagedResult<AttendanceDto>>> GetAllAsync(AttendanceSearchDto searchDto, string tenantId);
        Task<Result<AttendanceDto>> CreateAsync(CreateAttendanceDto createDto, string tenantId);
        Task<Result<AttendanceDto>> UpdateAsync(int id, UpdateAttendanceDto updateDto, string tenantId);
        Task<Result<bool>> DeleteAsync(int id, string tenantId);
        Task<Result<List<AttendanceDto>>> GetAttendanceByStudentAsync(int studentId, DateTime startDate, DateTime endDate, string tenantId);
        Task<Result<List<AttendanceDto>>> GetAttendanceByTripAsync(int tripId, string tenantId);
        Task<Result<bool>> RecordBoardingAsync(int studentId, int tripId, int stopId, DateTime boardingTime, string tenantId);
        Task<Result<bool>> RecordAlightingAsync(int studentId, int tripId, int stopId, DateTime alightingTime, string tenantId);
        
        Task<Result<AttendanceDto>> RecordRFIDAttendanceAsync(string rfidTag, int tripId, int stopId, DateTime timestamp, string tenantId);
        Task<Result<AttendanceDto>> RecordPhotoAttendanceAsync(int studentId, int tripId, int stopId, string photoBase64, DateTime timestamp, string tenantId);
        Task<Result<AttendanceDto>> RecordBiometricAttendanceAsync(int studentId, int tripId, int stopId, string biometricData, string biometricType, DateTime timestamp, string tenantId);
        Task<Result<OfflineAttendanceSyncResultDto>> SyncOfflineAttendanceAsync(List<OfflineAttendanceDto> offlineRecords, string tenantId);
        Task<Result<List<AttendanceMethodDto>>> GetAttendanceMethodsAsync(string tenantId);
        Task<Result<AttendanceAnalyticsDto>> GetAttendanceAnalyticsAsync(DateTime startDate, DateTime endDate, string tenantId);
        Task<Result<List<GeofenceAlertDto>>> GenerateGeofenceAlertsAsync(int tripId, double latitude, double longitude, string tenantId);
    }
}

