using SchoolTransportationSystem.Application.DTOs;
using SchoolTransportationSystem.Core.Common;

namespace SchoolTransportationSystem.Application.Interfaces
{
    public interface IPaymentGatewayService
    {
        Task<Result<PaymentGatewayDto>> GetByIdAsync(int id);
        Task<Result<IEnumerable<PaymentGatewayDto>>> GetAllAsync();
        Task<Result<PaymentGatewayDto>> CreateAsync(CreatePaymentGatewayDto dto);
        Task<Result<PaymentGatewayDto>> UpdateAsync(UpdatePaymentGatewayDto dto);
        Task<Result<bool>> DeleteAsync(int id);
        Task<Result<CreditCardPaymentDto>> ProcessCreditCardPaymentAsync(CreditCardPaymentRequestDto request);
        Task<Result<PaymentSecurityValidationDto>> ValidatePaymentSecurityAsync(PaymentSecurityRequestDto request);
        Task<Result<SecurityCheckDto>> PerformFraudCheckAsync(PaymentSecurityRequestDto request);
        Task<Result<SecurityCheckDto>> PerformAVSCheckAsync(PaymentSecurityRequestDto request);
        Task<Result<SecurityCheckDto>> PerformCVVCheckAsync(PaymentSecurityRequestDto request);
        Task<Result<SecurityCheckDto>> Perform3DSecureCheckAsync(PaymentSecurityRequestDto request);
        Task<Result<SecurityCheckDto>> PerformVelocityCheckAsync(PaymentSecurityRequestDto request);
        Task<Result<PaymentReceiptDataDto>> GenerateReceiptAsync(CreditCardPaymentDto payment);
    }
}
