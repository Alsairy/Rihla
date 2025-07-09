using SchoolTransportationSystem.Application.DTOs;
using SchoolTransportationSystem.Core.Common;

namespace SchoolTransportationSystem.Application.Interfaces
{
    public interface IStudentService
    {
        Task<Result<StudentDto>> GetByIdAsync(int id, string tenantId);
        Task<Result<PagedResult<StudentDto>>> GetAllAsync(StudentSearchDto searchDto, string tenantId);
        Task<Result<StudentDto>> CreateAsync(CreateStudentDto createDto, string tenantId);
        Task<Result<StudentDto>> UpdateAsync(int id, UpdateStudentDto updateDto, string tenantId);
        Task<Result<bool>> DeleteAsync(int id, string tenantId);
        Task<Result<StudentDto>> GetByStudentNumberAsync(string studentNumber, string tenantId);
        Task<Result<List<StudentDto>>> GetStudentsBySchoolAsync(string school, string tenantId);
        Task<Result<List<StudentDto>>> GetStudentsByRouteAsync(int routeId, string tenantId);
    }
}

