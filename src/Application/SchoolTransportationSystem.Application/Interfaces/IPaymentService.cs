using SchoolTransportationSystem.Application.DTOs;
using SchoolTransportationSystem.Core.Common;

namespace SchoolTransportationSystem.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<Result<PaymentDto>> GetByIdAsync(int id, string tenantId);
        Task<Result<PagedResult<PaymentDto>>> GetAllAsync(PaymentSearchDto searchDto, string tenantId);
        Task<Result<PaymentDto>> CreateAsync(CreatePaymentDto createDto, string tenantId);
        Task<Result<PaymentDto>> UpdateAsync(int id, UpdatePaymentDto updateDto, string tenantId);
        Task<Result<bool>> DeleteAsync(int id, string tenantId);
        Task<Result<List<PaymentDto>>> GetPaymentsByStudentAsync(int studentId, DateTime startDate, DateTime endDate, string tenantId);
        Task<Result<List<PaymentDto>>> GetOverduePaymentsAsync(string tenantId);
        Task<Result<bool>> ProcessPaymentAsync(int paymentId, string paymentMethod, string tenantId);
    }
}

