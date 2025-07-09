using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rihla.Application.DTOs;
using Rihla.Application.Interfaces;
using Rihla.Core.Common;
using Rihla.Core.Entities;
using Rihla.Core.Enums;
using Rihla.Core.Interfaces;

namespace Rihla.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(IUnitOfWork unitOfWork, ILogger<PaymentService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<PaymentDto>> GetByIdAsync(int id, string tenantId)
        {
            try
            {
                var payment = await _unitOfWork.Payments
                    .QueryWithIncludes(tenantId, p => p.Student)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (payment == null)
                {
                    return Result<PaymentDto>.Failure("Payment not found");
                }

                var paymentDto = MapToDto(payment);
                return Result<PaymentDto>.Success(paymentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment by ID {PaymentId}", id);
                return Result<PaymentDto>.Failure("An error occurred while retrieving the payment");
            }
        }

        public async Task<Result<PagedResult<PaymentDto>>> GetAllAsync(PaymentSearchDto searchDto, string tenantId)
        {
            try
            {
                var query = _unitOfWork.Payments
                    .QueryWithIncludes(tenantId, p => p.Student);

                if (searchDto.StudentId.HasValue)
                    query = query.Where(p => p.StudentId == searchDto.StudentId.Value);

                if (!string.IsNullOrEmpty(searchDto.PaymentReference))
                    query = query.Where(p => p.PaymentNumber.Contains(searchDto.PaymentReference));

                if (searchDto.Type.HasValue)
                    query = query.Where(p => p.Type == searchDto.Type.Value);

                if (searchDto.Status.HasValue)
                    query = query.Where(p => p.Status == searchDto.Status.Value);

                if (searchDto.AmountFrom.HasValue)
                    query = query.Where(p => p.Amount >= searchDto.AmountFrom.Value);

                if (searchDto.AmountTo.HasValue)
                    query = query.Where(p => p.Amount <= searchDto.AmountTo.Value);

                if (searchDto.DueDateFrom.HasValue)
                    query = query.Where(p => p.DueDate.Date >= searchDto.DueDateFrom.Value.Date);

                if (searchDto.DueDateTo.HasValue)
                    query = query.Where(p => p.DueDate.Date <= searchDto.DueDateTo.Value.Date);

                if (searchDto.IsOverdue.HasValue && searchDto.IsOverdue.Value)
                    query = query.Where(p => p.DueDate < DateTime.Today && p.Status != PaymentStatus.Completed);

                var totalCount = await query.CountAsync();

                var payments = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                var paymentDtos = payments.Select(MapToDto).ToList();

                var pagedResult = new PagedResult<PaymentDto>
                {
                    Items = paymentDtos,
                    TotalCount = totalCount,
                    Page = searchDto.Page,
                    PageSize = searchDto.PageSize
                };

                return Result<PagedResult<PaymentDto>>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments");
                return Result<PagedResult<PaymentDto>>.Failure("An error occurred while retrieving payments");
            }
        }

        public async Task<Result<PaymentDto>> CreateAsync(CreatePaymentDto createDto, string tenantId)
        {
            try
            {
                var student = await _unitOfWork.Students
                    .GetByIdAsync(createDto.StudentId, tenantId);

                if (student == null)
                {
                    return Result<PaymentDto>.Failure("Student not found");
                }

                var paymentNumber = await GeneratePaymentNumberAsync(tenantId);

                var payment = new Payment
                {
                    TenantId = int.Parse(tenantId),
                    StudentId = createDto.StudentId,
                    PaymentNumber = paymentNumber,
                    Amount = createDto.Amount,
                    Currency = createDto.Currency,
                    Type = createDto.Type,
                    Status = PaymentStatus.Pending,
                    DueDate = createDto.DueDate,
                    Description = createDto.Description,
                    Notes = createDto.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Payments.AddAsync(payment);
                await _unitOfWork.SaveChangesAsync();

                var paymentDto = MapToDto(payment);
                return Result<PaymentDto>.Success(paymentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment");
                return Result<PaymentDto>.Failure("An error occurred while creating the payment");
            }
        }

        public async Task<Result<PaymentDto>> UpdateAsync(int id, UpdatePaymentDto updateDto, string tenantId)
        {
            try
            {
                var payment = await _unitOfWork.Payments
                    .GetByIdAsync(id, tenantId);

                if (payment == null)
                {
                    return Result<PaymentDto>.Failure("Payment not found");
                }

                payment.Type = updateDto.Type;
                payment.Amount = updateDto.Amount;
                payment.Currency = updateDto.Currency;
                payment.DueDate = updateDto.DueDate;
                payment.Description = updateDto.Description;
                payment.Notes = updateDto.Notes;
                payment.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                var paymentDto = MapToDto(payment);
                return Result<PaymentDto>.Success(paymentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment with ID {PaymentId}", id);
                return Result<PaymentDto>.Failure("An error occurred while updating the payment");
            }
        }

        public async Task<Result<bool>> DeleteAsync(int id, string tenantId)
        {
            try
            {
                var payment = await _unitOfWork.Payments
                    .GetByIdAsync(id, tenantId);

                if (payment == null)
                {
                    return Result<bool>.Failure("Payment not found");
                }

                if (payment.Status == PaymentStatus.Completed)
                {
                    return Result<bool>.Failure("Cannot delete a completed payment");
                }

                payment.IsDeleted = true;
                payment.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment with ID {PaymentId}", id);
                return Result<bool>.Failure("An error occurred while deleting the payment");
            }
        }

        public async Task<Result<List<PaymentDto>>> GetPaymentsByStudentAsync(int studentId, DateTime startDate, DateTime endDate, string tenantId)
        {
            try
            {
                var payments = await _unitOfWork.Payments
                    .QueryWithIncludes(tenantId, p => p.Student)
                    .Where(p => p.StudentId == studentId)
                    .Where(p => p.CreatedAt.Date >= startDate.Date && p.CreatedAt.Date <= endDate.Date)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                var paymentDtos = payments.Select(MapToDto).ToList();
                return Result<List<PaymentDto>>.Success(paymentDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments by student {StudentId} from {StartDate} to {EndDate}", studentId, startDate, endDate);
                return Result<List<PaymentDto>>.Failure("An error occurred while retrieving student payments");
            }
        }

        public async Task<Result<List<PaymentDto>>> GetOverduePaymentsAsync(string tenantId)
        {
            try
            {
                var today = DateTime.Today;
                var payments = await _unitOfWork.Payments
                    .QueryWithIncludes(tenantId, p => p.Student)
                    .Where(p => p.DueDate < today && p.Status != PaymentStatus.Completed)
                    .OrderBy(p => p.DueDate)
                    .ToListAsync();

                var paymentDtos = payments.Select(MapToDto).ToList();
                return Result<List<PaymentDto>>.Success(paymentDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overdue payments");
                return Result<List<PaymentDto>>.Failure("An error occurred while retrieving overdue payments");
            }
        }

        public async Task<Result<bool>> ProcessPaymentAsync(int paymentId, string paymentMethod, string tenantId)
        {
            try
            {
                var payment = await _unitOfWork.Payments
                    .GetByIdAsync(paymentId, tenantId);

                if (payment == null)
                {
                    return Result<bool>.Failure("Payment not found");
                }

                if (payment.Status == PaymentStatus.Completed)
                {
                    return Result<bool>.Failure("Payment has already been processed");
                }

                payment.Status = PaymentStatus.Completed;
                payment.PaymentMethod = paymentMethod;
                payment.PaidDate = DateTime.UtcNow;
                payment.TransactionId = Guid.NewGuid().ToString();
                payment.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment {PaymentId}", paymentId);
                return Result<bool>.Failure("An error occurred while processing the payment");
            }
        }

        private async Task<string> GeneratePaymentNumberAsync(string tenantId)
        {
            var count = await _unitOfWork.Payments
                .Query(tenantId)
                .CountAsync();

            return $"PAY-{DateTime.UtcNow:yyyyMM}-{(count + 1):D4}";
        }

        private PaymentDto MapToDto(Payment payment)
        {
            return new PaymentDto
            {
                Id = payment.Id,
                StudentId = payment.StudentId,
                PaymentReference = payment.PaymentNumber,
                Type = payment.Type,
                Method = PaymentMethod.Cash, // Default since not in entity
                Status = payment.Status,
                Amount = payment.Amount,
                Currency = payment.Currency,
                PaymentDate = payment.PaidDate ?? payment.CreatedAt,
                DueDate = payment.DueDate,
                Description = payment.Description,
                TransactionId = payment.TransactionId,
                GatewayReference = null, // Not available in entity
                ReceiptNumber = null, // Not available in entity
                ProcessedDate = payment.PaidDate,
                ProcessedBy = null, // Not available in entity
                Notes = payment.Notes,
                RefundAmount = null, // Not available in entity
                RefundDate = null, // Not available in entity
                RefundReason = null, // Not available in entity
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt,
                TenantId = payment.TenantId.ToString(),
                Student = payment.Student != null ? MapStudentToDto(payment.Student) : new StudentDto()
            };
        }

        private StudentDto MapStudentToDto(Student student)
        {
            return new StudentDto
            {
                Id = student.Id,
                TenantId = student.TenantId,
                StudentNumber = student.StudentNumber,
                FirstName = student.FullName.FirstName,
                LastName = student.FullName.LastName,
                MiddleName = student.FullName.MiddleName,
                DateOfBirth = student.DateOfBirth,
                Grade = student.Grade,
                Phone = student.Phone,
                Email = student.Email,
                Street = student.Address.Street,
                City = student.Address.City,
                State = student.Address.State,
                ZipCode = student.Address.ZipCode,
                Country = student.Address.Country,
                ParentName = student.ParentName,
                ParentPhone = student.ParentPhone,
                ParentEmail = student.ParentEmail,
                EmergencyContact = student.EmergencyContact,
                EmergencyPhone = student.EmergencyPhone,
                SpecialNeeds = student.SpecialNeeds,
                Notes = student.Notes,
                CreatedAt = student.CreatedAt,
                UpdatedAt = student.UpdatedAt
            };
        }
    }
}

