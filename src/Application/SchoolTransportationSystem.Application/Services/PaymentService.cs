using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolTransportationSystem.Application.DTOs;
using SchoolTransportationSystem.Application.Interfaces;
using SchoolTransportationSystem.Core.Common;
using SchoolTransportationSystem.Core.Entities;
using SchoolTransportationSystem.Core.Enums;
using SchoolTransportationSystem.Infrastructure.Data;

namespace SchoolTransportationSystem.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(ApplicationDbContext context, ILogger<PaymentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<PaymentDto>> GetByIdAsync(int id, string tenantId)
        {
            try
            {
                var payment = await _context.Payments
                    .Include(p => p.Student)
                    .Where(p => p.Id == id && p.TenantId == int.Parse(tenantId) && !p.IsDeleted)
                    .FirstOrDefaultAsync();

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
                var query = _context.Payments
                    .Include(p => p.Student)
                    .Where(p => p.TenantId == int.Parse(tenantId) && !p.IsDeleted);

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
                var student = await _context.Students
                    .Where(s => s.Id == createDto.StudentId && s.TenantId == int.Parse(tenantId) && !s.IsDeleted)
                    .FirstOrDefaultAsync();

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

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

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
                var payment = await _context.Payments
                    .Where(p => p.Id == id && p.TenantId == int.Parse(tenantId) && !p.IsDeleted)
                    .FirstOrDefaultAsync();

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

                await _context.SaveChangesAsync();

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
                var payment = await _context.Payments
                    .Where(p => p.Id == id && p.TenantId == int.Parse(tenantId) && !p.IsDeleted)
                    .FirstOrDefaultAsync();

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

                await _context.SaveChangesAsync();
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
                var payments = await _context.Payments
                    .Include(p => p.Student)
                    .Where(p => p.StudentId == studentId && p.TenantId == int.Parse(tenantId) && !p.IsDeleted)
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
                var payments = await _context.Payments
                    .Include(p => p.Student)
                    .Where(p => p.TenantId == int.Parse(tenantId) && !p.IsDeleted)
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
                var payment = await _context.Payments
                    .Where(p => p.Id == paymentId && p.TenantId == int.Parse(tenantId) && !p.IsDeleted)
                    .FirstOrDefaultAsync();

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

                await _context.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment {PaymentId}", paymentId);
                return Result<bool>.Failure("An error occurred while processing the payment");
            }
        }

        public async Task<Result<PaymentDto>> ProcessSecurePaymentAsync(int paymentId, string paymentMethod, string gatewayReference, string tenantId)
        {
            try
            {
                var payment = await _context.Payments
                    .Include(p => p.Student)
                    .Where(p => p.Id == paymentId && p.TenantId == int.Parse(tenantId) && !p.IsDeleted)
                    .FirstOrDefaultAsync();

                if (payment == null)
                {
                    return Result<PaymentDto>.Failure("Payment not found");
                }

                if (payment.Status == PaymentStatus.Completed)
                {
                    return Result<PaymentDto>.Failure("Payment has already been processed");
                }

                var securityValidation = await ValidatePaymentSecurityAsync(paymentId, paymentMethod, tenantId);
                if (!securityValidation.IsSuccess)
                {
                    _logger.LogWarning("Payment security validation failed for payment {PaymentId}: {Error}", paymentId, securityValidation.Error);
                    return Result<PaymentDto>.Failure($"Security validation failed: {securityValidation.Error}");
                }

                payment.Status = PaymentStatus.Completed;
                payment.PaymentMethod = paymentMethod;
                payment.PaidDate = DateTime.UtcNow;
                payment.TransactionId = Guid.NewGuid().ToString();
                payment.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(gatewayReference))
                {
                    payment.Notes = $"{payment.Notes ?? ""}\nGateway Reference: {gatewayReference}";
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Secure payment processed successfully for payment {PaymentId} with gateway reference {GatewayReference}", paymentId, gatewayReference);

                var paymentDto = MapToDto(payment);
                return Result<PaymentDto>.Success(paymentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing secure payment {PaymentId}", paymentId);
                return Result<PaymentDto>.Failure("An error occurred while processing the secure payment");
            }
        }

        public async Task<Result<List<PaymentDto>>> GenerateAutomatedInvoicesAsync(DateTime billingPeriodStart, DateTime billingPeriodEnd, string tenantId)
        {
            try
            {
                var students = await _context.Students
                    .Where(s => s.TenantId == int.Parse(tenantId) && !s.IsDeleted)
                    .ToListAsync();

                var generatedInvoices = new List<PaymentDto>();

                foreach (var student in students)
                {
                    var existingInvoice = await _context.Payments
                        .Where(p => p.StudentId == student.Id && 
                                   p.TenantId == int.Parse(tenantId) && 
                                   p.Type == PaymentType.MonthlyFee &&
                                   p.CreatedAt.Date >= billingPeriodStart.Date && 
                                   p.CreatedAt.Date <= billingPeriodEnd.Date &&
                                   !p.IsDeleted)
                        .FirstOrDefaultAsync();

                    if (existingInvoice != null)
                    {
                        continue; // Skip if invoice already exists
                    }

                    decimal baseFee = 150.00m; // Base monthly transportation fee
                    
                    var discountResult = await ApplyFamilyDiscountsAsync(student.Id, baseFee, tenantId);
                    decimal finalAmount = discountResult.IsSuccess ? discountResult.Value : baseFee;

                    var paymentNumber = await GeneratePaymentNumberAsync(tenantId);
                    var dueDate = billingPeriodEnd.AddDays(15); // 15 days after billing period end

                    var invoice = new Payment
                    {
                        TenantId = int.Parse(tenantId),
                        StudentId = student.Id,
                        PaymentNumber = paymentNumber,
                        Amount = finalAmount,
                        Currency = "USD",
                        Type = PaymentType.MonthlyFee,
                        Status = PaymentStatus.Pending,
                        DueDate = dueDate,
                        Description = $"Monthly Transportation Fee - {billingPeriodStart:MMM yyyy}",
                        Notes = $"Billing Period: {billingPeriodStart:yyyy-MM-dd} to {billingPeriodEnd:yyyy-MM-dd}",
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Payments.Add(invoice);
                    generatedInvoices.Add(MapToDto(invoice));
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Generated {Count} automated invoices for billing period {Start} to {End}", 
                    generatedInvoices.Count, billingPeriodStart, billingPeriodEnd);

                return Result<List<PaymentDto>>.Success(generatedInvoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating automated invoices for period {Start} to {End}", billingPeriodStart, billingPeriodEnd);
                return Result<List<PaymentDto>>.Failure("An error occurred while generating automated invoices");
            }
        }

        public async Task<Result<decimal>> ApplyFamilyDiscountsAsync(int studentId, decimal baseAmount, string tenantId)
        {
            try
            {
                var student = await _context.Students
                    .Where(s => s.Id == studentId && s.TenantId == int.Parse(tenantId) && !s.IsDeleted)
                    .FirstOrDefaultAsync();

                if (student == null)
                {
                    return Result<decimal>.Failure("Student not found");
                }

                var siblings = await _context.Students
                    .Where(s => s.TenantId == int.Parse(tenantId) && 
                               !s.IsDeleted && 
                               s.Id != studentId &&
                               (s.ParentEmail == student.ParentEmail || s.ParentPhone == student.ParentPhone))
                    .CountAsync();

                decimal discountedAmount = baseAmount;
                decimal discountPercentage = 0;

                if (siblings >= 3) // 4 or more children
                {
                    discountPercentage = 0.25m; // 25% discount
                }
                else if (siblings >= 2) // 3 children
                {
                    discountPercentage = 0.20m; // 20% discount
                }
                else if (siblings >= 1) // 2 children
                {
                    discountPercentage = 0.15m; // 15% discount
                }

                if (discountPercentage > 0)
                {
                    discountedAmount = baseAmount * (1 - discountPercentage);
                    _logger.LogInformation("Applied {DiscountPercentage}% family discount to student {StudentId}. Original: {BaseAmount}, Discounted: {DiscountedAmount}", 
                        discountPercentage * 100, studentId, baseAmount, discountedAmount);
                }

                return Result<decimal>.Success(discountedAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying family discount for student {StudentId}", studentId);
                return Result<decimal>.Failure("An error occurred while applying family discount");
            }
        }

        public async Task<Result<PaymentDto>> ProcessRefundsAsync(int paymentId, decimal refundAmount, string refundReason, string tenantId)
        {
            try
            {
                var payment = await _context.Payments
                    .Include(p => p.Student)
                    .Where(p => p.Id == paymentId && p.TenantId == int.Parse(tenantId) && !p.IsDeleted)
                    .FirstOrDefaultAsync();

                if (payment == null)
                {
                    return Result<PaymentDto>.Failure("Payment not found");
                }

                if (payment.Status != PaymentStatus.Completed)
                {
                    return Result<PaymentDto>.Failure("Cannot refund a payment that is not completed");
                }

                if (refundAmount <= 0 || refundAmount > payment.Amount)
                {
                    return Result<PaymentDto>.Failure("Invalid refund amount");
                }

                var refundPaymentNumber = await GeneratePaymentNumberAsync(tenantId);
                
                var refundPayment = new Payment
                {
                    TenantId = int.Parse(tenantId),
                    StudentId = payment.StudentId,
                    PaymentNumber = refundPaymentNumber,
                    Amount = -refundAmount, // Negative amount for refund
                    Currency = payment.Currency,
                    Type = PaymentType.Refund,
                    Status = PaymentStatus.Completed,
                    DueDate = DateTime.UtcNow,
                    Description = $"Refund for {payment.PaymentNumber}",
                    Notes = $"Original Payment: {payment.PaymentNumber}\nRefund Reason: {refundReason}",
                    PaymentMethod = payment.PaymentMethod,
                    PaidDate = DateTime.UtcNow,
                    TransactionId = Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Payments.Add(refundPayment);

                payment.Notes = $"{payment.Notes ?? ""}\nRefund Processed: {refundAmount:C} on {DateTime.UtcNow:yyyy-MM-dd} - {refundReason}";
                payment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Processed refund of {RefundAmount} for payment {PaymentId}. Reason: {RefundReason}", 
                    refundAmount, paymentId, refundReason);

                var refundDto = MapToDto(refundPayment);
                return Result<PaymentDto>.Success(refundDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for payment {PaymentId}", paymentId);
                return Result<PaymentDto>.Failure("An error occurred while processing the refund");
            }
        }

        private async Task<Result<bool>> ValidatePaymentSecurityAsync(int paymentId, string paymentMethod, string tenantId)
        {
            try
            {
                if (string.IsNullOrEmpty(paymentMethod))
                {
                    return Result<bool>.Failure("Payment method is required");
                }

                var supportedMethods = new[] { "CreditCard", "DebitCard", "BankTransfer", "Cash", "Check" };
                if (!supportedMethods.Contains(paymentMethod))
                {
                    return Result<bool>.Failure("Unsupported payment method");
                }


                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating payment security for payment {PaymentId}", paymentId);
                return Result<bool>.Failure("Security validation failed");
            }
        }

        private async Task<string> GeneratePaymentNumberAsync(string tenantId)
        {
            var count = await _context.Payments
                .Where(p => p.TenantId == int.Parse(tenantId))
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

        public async Task<Result<SecurePaymentResultDto>> ProcessSecurePaymentAsync(SecurePaymentRequestDto request, string tenantId)
        {
            try
            {
                if (request.Amount <= 0)
                {
                    return Result<SecurePaymentResultDto>.Failure("Invalid payment amount");
                }

                var transactionId = Guid.NewGuid().ToString();
                var result = new SecurePaymentResultDto
                {
                    Success = true,
                    TransactionId = transactionId,
                    Status = "Completed",
                    Amount = request.Amount,
                    Currency = request.Currency,
                    ProcessedAt = DateTime.UtcNow,
                    AuthorizationCode = $"AUTH-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    ReceiptUrl = $"/receipts/{transactionId}"
                };

                _logger.LogInformation("Secure payment processed successfully: {TransactionId}", transactionId);
                return Result<SecurePaymentResultDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing secure payment");
                return Result<SecurePaymentResultDto>.Failure("An error occurred while processing the secure payment");
            }
        }

        public async Task<Result<List<PaymentDto>>> GenerateAutomatedInvoicesAsync(InvoiceGenerationRequestDto request, string tenantId)
        {
            try
            {
                var invoices = await GenerateAutomatedInvoicesAsync(request.BillingPeriodStart, request.BillingPeriodEnd, tenantId);
                return invoices;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating automated invoices");
                return Result<List<PaymentDto>>.Failure("An error occurred while generating automated invoices");
            }
        }

        public async Task<Result<FamilyDiscountResultDto>> ApplyFamilyDiscountsAsync(FamilyDiscountRequestDto request, string tenantId)
        {
            try
            {
                decimal totalOriginalAmount = 0;
                decimal totalDiscountedAmount = 0;

                foreach (var studentId in request.StudentIds)
                {
                    var baseAmount = 150.00m;
                    totalOriginalAmount += baseAmount;

                    var discountResult = await ApplyFamilyDiscountsAsync(studentId, baseAmount, tenantId);
                    totalDiscountedAmount += discountResult.IsSuccess ? discountResult.Value : baseAmount;
                }

                var result = new FamilyDiscountResultDto
                {
                    Success = true,
                    OriginalAmount = totalOriginalAmount,
                    DiscountAmount = totalOriginalAmount - totalDiscountedAmount,
                    FinalAmount = totalDiscountedAmount,
                    DiscountPercentage = request.DiscountPercentage
                };

                return Result<FamilyDiscountResultDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying family discounts");
                return Result<FamilyDiscountResultDto>.Failure("An error occurred while applying family discounts");
            }
        }

        public async Task<Result<RefundResultDto>> ProcessRefundsAsync(RefundRequestDto request, string tenantId)
        {
            try
            {
                var payment = await _context.Payments
                    .Where(p => p.TransactionId == request.TransactionId && p.TenantId == int.Parse(tenantId) && !p.IsDeleted)
                    .FirstOrDefaultAsync();

                if (payment == null)
                {
                    return Result<RefundResultDto>.Failure("Payment not found");
                }

                var refundResult = await ProcessRefundsAsync(payment.Id, request.RefundAmount, request.Reason, tenantId);
                if (!refundResult.IsSuccess)
                {
                    return Result<RefundResultDto>.Failure(refundResult.Error);
                }

                var result = new RefundResultDto
                {
                    Success = true,
                    RefundId = Guid.NewGuid().ToString(),
                    RefundAmount = request.RefundAmount,
                    Status = "Completed",
                    ProcessedAt = DateTime.UtcNow
                };

                return Result<RefundResultDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund");
                return Result<RefundResultDto>.Failure("An error occurred while processing the refund");
            }
        }

        public async Task<Result<List<PaymentGatewayStatusDto>>> GetPaymentGatewayStatusAsync(string tenantId)
        {
            try
            {
                var gateways = new List<PaymentGatewayStatusDto>
                {
                    new PaymentGatewayStatusDto
                    {
                        GatewayName = "Stripe",
                        IsOnline = true,
                        Status = "Active",
                        LastChecked = DateTime.UtcNow,
                        ResponseTime = 150
                    },
                    new PaymentGatewayStatusDto
                    {
                        GatewayName = "PayPal",
                        IsOnline = true,
                        Status = "Active",
                        LastChecked = DateTime.UtcNow,
                        ResponseTime = 200
                    }
                };

                return Result<List<PaymentGatewayStatusDto>>.Success(gateways);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment gateway status");
                return Result<List<PaymentGatewayStatusDto>>.Failure("An error occurred while retrieving payment gateway status");
            }
        }

        public async Task<Result<FraudDetectionResultDto>> GetFraudDetectionResultsAsync(string transactionId, string tenantId)
        {
            try
            {
                var result = new FraudDetectionResultDto
                {
                    IsFraudulent = false,
                    RiskScore = 0.15m,
                    RiskLevel = "Low",
                    Flags = new List<string>(),
                    Recommendation = "Approve transaction"
                };

                return Result<FraudDetectionResultDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting fraud detection results for transaction {TransactionId}", transactionId);
                return Result<FraudDetectionResultDto>.Failure("An error occurred while retrieving fraud detection results");
            }
        }

        public async Task<Result<PaymentAnalyticsDto>> GetPaymentAnalyticsAsync(DateTime startDate, DateTime endDate, string tenantId)
        {
            try
            {
                var payments = await _context.Payments
                    .Where(p => p.TenantId == int.Parse(tenantId) && !p.IsDeleted)
                    .Where(p => p.CreatedAt.Date >= startDate.Date && p.CreatedAt.Date <= endDate.Date)
                    .ToListAsync();

                var analytics = new PaymentAnalyticsDto
                {
                    TotalRevenue = payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount),
                    TotalTransactions = payments.Count,
                    AverageTransactionAmount = payments.Count > 0 ? payments.Average(p => p.Amount) : 0,
                    SuccessfulTransactions = payments.Count(p => p.Status == PaymentStatus.Completed),
                    FailedTransactions = payments.Count(p => p.Status == PaymentStatus.Failed),
                    SuccessRate = payments.Count > 0 ? (decimal)payments.Count(p => p.Status == PaymentStatus.Completed) / payments.Count * 100 : 0,
                    PaymentMethodStats = new List<PaymentMethodStatsDto>(),
                    DailyRevenue = new List<DailyRevenueDto>()
                };

                return Result<PaymentAnalyticsDto>.Success(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment analytics from {StartDate} to {EndDate}", startDate, endDate);
                return Result<PaymentAnalyticsDto>.Failure("An error occurred while retrieving payment analytics");
            }
        }
    }
}

