using Rihla.Application.DTOs;
using Rihla.Core.Common;

namespace Rihla.Application.Interfaces
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
    }
}

