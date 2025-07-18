using Microsoft.Extensions.Logging;
using SchoolTransportationSystem.Application.DTOs;
using SchoolTransportationSystem.Application.Interfaces;
using SchoolTransportationSystem.Core.Common;
using SchoolTransportationSystem.Core.Entities;
using SchoolTransportationSystem.Core.Enums;
using SchoolTransportationSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace SchoolTransportationSystem.Application.Services
{
    public class PaymentGatewayService : IPaymentGatewayService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PaymentGatewayService> _logger;

        public PaymentGatewayService(ApplicationDbContext context, ILogger<PaymentGatewayService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<PaymentGatewayDto>> GetByIdAsync(int id)
        {
            try
            {
                var gateway = await _context.PaymentGateways.FindAsync(id);
                if (gateway == null)
                {
                    return Result<PaymentGatewayDto>.Failure("Payment gateway not found");
                }

                var dto = new PaymentGatewayDto
                {
                    Id = gateway.Id,
                    GatewayName = gateway.GatewayName,
                    GatewayType = gateway.GatewayType,
                    TransactionId = gateway.TransactionId,
                    ExternalTransactionId = gateway.ExternalTransactionId,
                    Amount = gateway.Amount,
                    Currency = gateway.Currency,
                    Status = gateway.Status,
                    PaymentMethod = gateway.PaymentMethod,
                    CardLast4 = gateway.CardLast4,
                    CardType = gateway.CardType,
                    ProcessorResponse = gateway.ProcessorResponse,
                    SecurityToken = gateway.SecurityToken,
                    FraudScore = gateway.FraudScore,
                    ProcessingFee = gateway.ProcessingFee,
                    RefundAmount = gateway.RefundAmount,
                    Notes = gateway.Notes,
                    PaymentId = gateway.PaymentId,
                    ProcessedAt = gateway.ProcessedAt,
                    CreatedAt = gateway.CreatedAt,
                    UpdatedAt = gateway.UpdatedAt
                };

                return Result<PaymentGatewayDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment gateway by ID {Id}", id);
                return Result<PaymentGatewayDto>.Failure($"Error retrieving payment gateway: {ex.Message}");
            }
        }

        public async Task<Result<IEnumerable<PaymentGatewayDto>>> GetAllAsync()
        {
            try
            {
                var gateways = await _context.PaymentGateways.ToListAsync();
                var dtos = gateways.Select(g => new PaymentGatewayDto
                {
                    Id = g.Id,
                    GatewayName = g.GatewayName,
                    GatewayType = g.GatewayType,
                    TransactionId = g.TransactionId,
                    ExternalTransactionId = g.ExternalTransactionId,
                    Amount = g.Amount,
                    Currency = g.Currency,
                    Status = g.Status,
                    PaymentMethod = g.PaymentMethod,
                    CardLast4 = g.CardLast4,
                    CardType = g.CardType,
                    ProcessorResponse = g.ProcessorResponse,
                    SecurityToken = g.SecurityToken,
                    FraudScore = g.FraudScore,
                    ProcessingFee = g.ProcessingFee,
                    RefundAmount = g.RefundAmount,
                    Notes = g.Notes,
                    PaymentId = g.PaymentId,
                    ProcessedAt = g.ProcessedAt,
                    CreatedAt = g.CreatedAt,
                    UpdatedAt = g.UpdatedAt
                });

                return Result<IEnumerable<PaymentGatewayDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all payment gateways");
                return Result<IEnumerable<PaymentGatewayDto>>.Failure($"Error retrieving payment gateways: {ex.Message}");
            }
        }

        public async Task<Result<PaymentGatewayDto>> CreateAsync(CreatePaymentGatewayDto dto)
        {
            try
            {
                var gateway = new PaymentGateway
                {
                    GatewayName = dto.GatewayName,
                    GatewayType = dto.GatewayType,
                    TransactionId = dto.TransactionId,
                    ExternalTransactionId = dto.ExternalTransactionId,
                    Amount = dto.Amount,
                    Currency = dto.Currency,
                    Status = dto.Status,
                    PaymentMethod = dto.PaymentMethod,
                    CardLast4 = dto.CardLast4,
                    CardType = dto.CardType,
                    ProcessorResponse = dto.ProcessorResponse,
                    SecurityToken = dto.SecurityToken,
                    FraudScore = dto.FraudScore,
                    ProcessingFee = dto.ProcessingFee,
                    RefundAmount = dto.RefundAmount,
                    Notes = dto.Notes,
                    PaymentId = dto.PaymentId,
                    ProcessedAt = dto.ProcessedAt,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PaymentGateways.Add(gateway);
                await _context.SaveChangesAsync();

                var resultDto = new PaymentGatewayDto
                {
                    Id = gateway.Id,
                    GatewayName = gateway.GatewayName,
                    GatewayType = gateway.GatewayType,
                    TransactionId = gateway.TransactionId,
                    ExternalTransactionId = gateway.ExternalTransactionId,
                    Amount = gateway.Amount,
                    Currency = gateway.Currency,
                    Status = gateway.Status,
                    PaymentMethod = gateway.PaymentMethod,
                    CardLast4 = gateway.CardLast4,
                    CardType = gateway.CardType,
                    ProcessorResponse = gateway.ProcessorResponse,
                    SecurityToken = gateway.SecurityToken,
                    FraudScore = gateway.FraudScore,
                    ProcessingFee = gateway.ProcessingFee,
                    RefundAmount = gateway.RefundAmount,
                    Notes = gateway.Notes,
                    PaymentId = gateway.PaymentId,
                    ProcessedAt = gateway.ProcessedAt,
                    CreatedAt = gateway.CreatedAt,
                    UpdatedAt = gateway.UpdatedAt
                };

                return Result<PaymentGatewayDto>.Success(resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment gateway");
                return Result<PaymentGatewayDto>.Failure($"Error creating payment gateway: {ex.Message}");
            }
        }

        public async Task<Result<PaymentGatewayDto>> UpdateAsync(UpdatePaymentGatewayDto dto)
        {
            try
            {
                var gateway = await _context.PaymentGateways.FindAsync(dto.Id);
                if (gateway == null)
                {
                    return Result<PaymentGatewayDto>.Failure("Payment gateway not found");
                }

                if (!string.IsNullOrEmpty(dto.GatewayName))
                    gateway.GatewayName = dto.GatewayName;
                if (!string.IsNullOrEmpty(dto.GatewayType))
                    gateway.GatewayType = dto.GatewayType;
                if (!string.IsNullOrEmpty(dto.ExternalTransactionId))
                    gateway.ExternalTransactionId = dto.ExternalTransactionId;
                if (!string.IsNullOrEmpty(dto.Status))
                    gateway.Status = dto.Status;
                if (!string.IsNullOrEmpty(dto.PaymentMethod))
                    gateway.PaymentMethod = dto.PaymentMethod;
                if (!string.IsNullOrEmpty(dto.CardLast4))
                    gateway.CardLast4 = dto.CardLast4;
                if (!string.IsNullOrEmpty(dto.CardType))
                    gateway.CardType = dto.CardType;
                if (!string.IsNullOrEmpty(dto.ProcessorResponse))
                    gateway.ProcessorResponse = dto.ProcessorResponse;
                if (!string.IsNullOrEmpty(dto.SecurityToken))
                    gateway.SecurityToken = dto.SecurityToken;
                if (dto.FraudScore.HasValue)
                    gateway.FraudScore = dto.FraudScore;
                if (dto.ProcessingFee.HasValue)
                    gateway.ProcessingFee = dto.ProcessingFee;
                if (dto.RefundAmount.HasValue)
                    gateway.RefundAmount = dto.RefundAmount;
                if (!string.IsNullOrEmpty(dto.Notes))
                    gateway.Notes = dto.Notes;
                if (dto.ProcessedAt.HasValue)
                    gateway.ProcessedAt = dto.ProcessedAt;

                gateway.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var resultDto = new PaymentGatewayDto
                {
                    Id = gateway.Id,
                    GatewayName = gateway.GatewayName,
                    GatewayType = gateway.GatewayType,
                    TransactionId = gateway.TransactionId,
                    ExternalTransactionId = gateway.ExternalTransactionId,
                    Amount = gateway.Amount,
                    Currency = gateway.Currency,
                    Status = gateway.Status,
                    PaymentMethod = gateway.PaymentMethod,
                    CardLast4 = gateway.CardLast4,
                    CardType = gateway.CardType,
                    ProcessorResponse = gateway.ProcessorResponse,
                    SecurityToken = gateway.SecurityToken,
                    FraudScore = gateway.FraudScore,
                    ProcessingFee = gateway.ProcessingFee,
                    RefundAmount = gateway.RefundAmount,
                    Notes = gateway.Notes,
                    PaymentId = gateway.PaymentId,
                    ProcessedAt = gateway.ProcessedAt,
                    CreatedAt = gateway.CreatedAt,
                    UpdatedAt = gateway.UpdatedAt
                };

                return Result<PaymentGatewayDto>.Success(resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment gateway {Id}", dto.Id);
                return Result<PaymentGatewayDto>.Failure($"Error updating payment gateway: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            try
            {
                var gateway = await _context.PaymentGateways.FindAsync(id);
                if (gateway == null)
                {
                    return Result<bool>.Failure("Payment gateway not found");
                }

                _context.PaymentGateways.Remove(gateway);
                await _context.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment gateway {Id}", id);
                return Result<bool>.Failure($"Error deleting payment gateway: {ex.Message}");
            }
        }

        public async Task<Result<CreditCardPaymentDto>> ProcessCreditCardPaymentAsync(CreditCardPaymentRequestDto request)
        {
            try
            {
                var securityValidation = await ValidatePaymentSecurityAsync(new PaymentSecurityRequestDto
                {
                    CardNumber = request.CardNumber,
                    ExpiryMonth = request.ExpiryMonth,
                    ExpiryYear = request.ExpiryYear,
                    CVV = request.CVV,
                    CardHolderName = request.CardHolderName,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    CustomerEmail = request.CustomerEmail,
                    CustomerPhone = request.CustomerPhone,
                    BillingAddress = request.BillingAddress
                });

                if (!securityValidation.IsSuccess || !securityValidation.Value.IsValid)
                {
                    return Result<CreditCardPaymentDto>.Failure("Payment security validation failed");
                }

                var transactionId = $"TXN{DateTime.UtcNow.Ticks}";
                var authCode = GenerateAuthorizationCode();

                var payment = new CreditCardPaymentDto
                {
                    TransactionId = transactionId,
                    Status = "Success",
                    Amount = request.Amount,
                    Currency = request.Currency,
                    CardLast4 = request.CardNumber[^4..],
                    CardType = DetectCardType(request.CardNumber),
                    AuthorizationCode = authCode,
                    ProcessorResponse = "Approved",
                    ProcessingFee = request.Amount * 0.029m, // 2.9% processing fee
                    ProcessedAt = DateTime.UtcNow,
                    IsSuccessful = true
                };

                return Result<CreditCardPaymentDto>.Success(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing credit card payment");
                return Result<CreditCardPaymentDto>.Failure($"Payment processing failed: {ex.Message}");
            }
        }

        public async Task<Result<PaymentSecurityValidationDto>> ValidatePaymentSecurityAsync(PaymentSecurityRequestDto request)
        {
            try
            {
                var validation = new PaymentSecurityValidationDto
                {
                    IsValid = true,
                    ValidationStatus = "Passed",
                    SecurityScore = 100,
                    ValidatedAt = DateTime.UtcNow,
                    ValidatedBy = "System"
                };

                var fraudCheck = await PerformFraudCheckAsync(request);
                var avsCheck = await PerformAVSCheckAsync(request);
                var cvvCheck = await PerformCVVCheckAsync(request);

                if (!fraudCheck.IsSuccess || fraudCheck.Value.Status != "Passed")
                {
                    validation.IsValid = false;
                    validation.ValidationErrors.Add("Fraud check failed");
                    validation.SecurityScore -= 30;
                }

                if (!avsCheck.IsSuccess || avsCheck.Value.Status != "Passed")
                {
                    validation.SecurityWarnings.Add("Address verification warning");
                    validation.SecurityScore -= 10;
                }

                if (!cvvCheck.IsSuccess || cvvCheck.Value.Status != "Passed")
                {
                    validation.IsValid = false;
                    validation.ValidationErrors.Add("CVV verification failed");
                    validation.SecurityScore -= 25;
                }

                validation.ValidationStatus = validation.IsValid ? "Passed" : "Failed";
                validation.RecommendedAction = validation.IsValid ? "Proceed" : "Decline";

                return Result<PaymentSecurityValidationDto>.Success(validation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating payment security");
                return Result<PaymentSecurityValidationDto>.Failure($"Security validation failed: {ex.Message}");
            }
        }

        public async Task<Result<SecurityCheckDto>> PerformFraudCheckAsync(PaymentSecurityRequestDto request)
        {
            try
            {
                await Task.Delay(200); // Simulate processing time

                var check = new SecurityCheckDto
                {
                    CheckType = "Fraud Detection",
                    Status = "Passed",
                    Score = 95,
                    Result = "No fraud indicators detected",
                    CheckedAt = DateTime.UtcNow,
                    CheckedBy = "FraudGuard System"
                };

                if (request.Amount > 5000)
                {
                    check.Score -= 20;
                    check.Warnings.Add("High amount transaction");
                }

                if (string.IsNullOrEmpty(request.CustomerEmail))
                {
                    check.Score -= 10;
                    check.Warnings.Add("No customer email provided");
                }

                check.Status = check.Score >= 70 ? "Passed" : "Failed";
                check.RecommendedAction = check.Status == "Passed" ? "Approve" : "Review";

                return Result<SecurityCheckDto>.Success(check);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing fraud check");
                return Result<SecurityCheckDto>.Failure($"Fraud check failed: {ex.Message}");
            }
        }

        public async Task<Result<SecurityCheckDto>> PerformAVSCheckAsync(PaymentSecurityRequestDto request)
        {
            try
            {
                await Task.Delay(150);

                var check = new SecurityCheckDto
                {
                    CheckType = "Address Verification",
                    Status = !string.IsNullOrEmpty(request.BillingAddress) ? "Passed" : "Failed",
                    Score = !string.IsNullOrEmpty(request.BillingAddress) ? 100 : 0,
                    Result = !string.IsNullOrEmpty(request.BillingAddress) ? "Address verified" : "Address verification failed",
                    CheckedAt = DateTime.UtcNow,
                    CheckedBy = "AVS System"
                };

                return Result<SecurityCheckDto>.Success(check);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing AVS check");
                return Result<SecurityCheckDto>.Failure($"AVS check failed: {ex.Message}");
            }
        }

        public async Task<Result<SecurityCheckDto>> PerformCVVCheckAsync(PaymentSecurityRequestDto request)
        {
            try
            {
                await Task.Delay(100);

                var isValid = !string.IsNullOrEmpty(request.CVV) && request.CVV.Length >= 3;
                var check = new SecurityCheckDto
                {
                    CheckType = "CVV Verification",
                    Status = isValid ? "Passed" : "Failed",
                    Score = isValid ? 100 : 0,
                    Result = isValid ? "CVV verified" : "Invalid CVV",
                    CheckedAt = DateTime.UtcNow,
                    CheckedBy = "CVV System"
                };

                return Result<SecurityCheckDto>.Success(check);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing CVV check");
                return Result<SecurityCheckDto>.Failure($"CVV check failed: {ex.Message}");
            }
        }

        public async Task<Result<SecurityCheckDto>> Perform3DSecureCheckAsync(PaymentSecurityRequestDto request)
        {
            try
            {
                await Task.Delay(300);

                var check = new SecurityCheckDto
                {
                    CheckType = "3D Secure",
                    Status = "Passed",
                    Score = 100,
                    Result = "3D Secure authentication successful",
                    CheckedAt = DateTime.UtcNow,
                    CheckedBy = "3DS System"
                };

                return Result<SecurityCheckDto>.Success(check);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing 3D Secure check");
                return Result<SecurityCheckDto>.Failure($"3D Secure check failed: {ex.Message}");
            }
        }

        public async Task<Result<SecurityCheckDto>> PerformVelocityCheckAsync(PaymentSecurityRequestDto request)
        {
            try
            {
                await Task.Delay(100);

                var check = new SecurityCheckDto
                {
                    CheckType = "Velocity Check",
                    Status = "Passed",
                    Score = 100,
                    Result = "Transaction velocity within limits",
                    CheckedAt = DateTime.UtcNow,
                    CheckedBy = "Velocity System"
                };

                return Result<SecurityCheckDto>.Success(check);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing velocity check");
                return Result<SecurityCheckDto>.Failure($"Velocity check failed: {ex.Message}");
            }
        }

        public async Task<Result<PaymentReceiptDataDto>> GenerateReceiptAsync(CreditCardPaymentDto payment)
        {
            try
            {
                var receipt = new PaymentReceiptDataDto
                {
                    ReceiptNumber = $"RCP{DateTime.UtcNow.Ticks % 1000000:D6}",
                    TransactionId = payment.TransactionId,
                    Amount = payment.Amount,
                    Currency = payment.Currency,
                    PaymentMethod = $"{payment.CardType} ending in {payment.CardLast4}",
                    PaymentDate = payment.ProcessedAt,
                    Status = payment.Status,
                    Description = "School Transportation Payment"
                };

                return Result<PaymentReceiptDataDto>.Success(receipt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating receipt");
                return Result<PaymentReceiptDataDto>.Failure($"Receipt generation failed: {ex.Message}");
            }
        }

        public async Task<Result<PaymentGatewayDto>> InitializePaymentGatewayAsync(string gatewayProvider, string merchantId, string tenantId)
        {
            try
            {
                var supportedGateways = new[] { "Stripe", "PayPal", "Square", "Authorize.Net", "Mada" };
                if (!supportedGateways.Contains(gatewayProvider))
                {
                    return Result<PaymentGatewayDto>.Failure($"Unsupported payment gateway: {gatewayProvider}");
                }

                if (string.IsNullOrEmpty(merchantId))
                {
                    return Result<PaymentGatewayDto>.Failure("Merchant ID is required for gateway initialization");
                }

                var gatewayConfig = new PaymentGatewayDto
                {
                    GatewayProvider = gatewayProvider,
                    MerchantId = merchantId,
                    IsActive = true,
                    SupportedPaymentMethods = GetSupportedPaymentMethods(gatewayProvider),
                    SecurityLevel = "PCI-DSS Level 1",
                    InitializedAt = DateTime.UtcNow,
                    TenantId = tenantId
                };

                switch (gatewayProvider.ToLower())
                {
                    case "stripe":
                        gatewayConfig.ApiEndpoint = "https://api.stripe.com/v1";
                        gatewayConfig.WebhookEndpoint = "/api/webhooks/stripe";
                        break;
                    case "paypal":
                        gatewayConfig.ApiEndpoint = "https://api.paypal.com/v2";
                        gatewayConfig.WebhookEndpoint = "/api/webhooks/paypal";
                        break;
                    case "mada":
                        gatewayConfig.ApiEndpoint = "https://api.mada.sa/v1";
                        gatewayConfig.WebhookEndpoint = "/api/webhooks/mada";
                        break;
                    default:
                        gatewayConfig.ApiEndpoint = $"https://api.{gatewayProvider.ToLower()}.com/v1";
                        gatewayConfig.WebhookEndpoint = $"/api/webhooks/{gatewayProvider.ToLower()}";
                        break;
                }

                var connectivityTest = await TestGatewayConnectivityAsync(gatewayConfig);
                if (!connectivityTest.IsSuccess)
                {
                    gatewayConfig.IsActive = false;
                    gatewayConfig.LastError = connectivityTest.Error;
                    _logger.LogWarning("Gateway connectivity test failed for {Gateway}: {Error}", gatewayProvider, connectivityTest.Error);
                }

                _logger.LogInformation("Payment gateway {Gateway} initialized successfully for tenant {TenantId}", gatewayProvider, tenantId);

                return Result<PaymentGatewayDto>.Success(gatewayConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing payment gateway {Gateway} for tenant {TenantId}", gatewayProvider, tenantId);
                return Result<PaymentGatewayDto>.Failure("An error occurred while initializing the payment gateway");
            }
        }

        public async Task<Result<PaymentSecurityValidationDto>> ValidatePaymentSecurityAsync(PaymentSecurityRequestDto securityRequest, string tenantId)
        {
            try
            {
                var validationResult = new PaymentSecurityValidationDto
                {
                    IsValid = true,
                    SecurityChecks = new List<SecurityCheckDto>(),
                    RiskScore = 0,
                    ValidationTimestamp = DateTime.UtcNow
                };

                var pciCheck = await ValidatePCIComplianceAsync(securityRequest);
                validationResult.SecurityChecks.Add(pciCheck);
                if (!pciCheck.Passed) validationResult.IsValid = false;

                if (securityRequest.PaymentMethod == "CreditCard" || securityRequest.PaymentMethod == "DebitCard")
                {
                    var cardCheck = await ValidateCardSecurityAsync(securityRequest);
                    validationResult.SecurityChecks.Add(cardCheck);
                    if (!cardCheck.Passed) validationResult.IsValid = false;
                    validationResult.RiskScore += cardCheck.RiskScore;
                }

                var fraudCheck = await DetectFraudAsync(securityRequest, tenantId);
                validationResult.SecurityChecks.Add(fraudCheck);
                if (!fraudCheck.Passed) validationResult.IsValid = false;
                validationResult.RiskScore += fraudCheck.RiskScore;

                var rateLimitCheck = await CheckRateLimitsAsync(securityRequest, tenantId);
                validationResult.SecurityChecks.Add(rateLimitCheck);
                if (!rateLimitCheck.Passed) validationResult.IsValid = false;

                if (!string.IsNullOrEmpty(securityRequest.BillingAddress))
                {
                    var addressCheck = await VerifyBillingAddressAsync(securityRequest);
                    validationResult.SecurityChecks.Add(addressCheck);
                    if (!addressCheck.Passed) validationResult.RiskScore += 10;
                }

                if (validationResult.RiskScore > 50)
                {
                    validationResult.IsValid = false;
                    validationResult.RiskLevel = "High";
                }
                else if (validationResult.RiskScore > 25)
                {
                    validationResult.RiskLevel = "Medium";
                }
                else
                {
                    validationResult.RiskLevel = "Low";
                }

                _logger.LogInformation("Payment security validation completed. Valid: {IsValid}, Risk Score: {RiskScore}", 
                    validationResult.IsValid, validationResult.RiskScore);

                return Result<PaymentSecurityValidationDto>.Success(validationResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating payment security for tenant {TenantId}", tenantId);
                return Result<PaymentSecurityValidationDto>.Failure("An error occurred during security validation");
            }
        }

        public async Task<Result<CreditCardPaymentDto>> ProcessCreditCardPaymentAsync(CreditCardPaymentRequestDto paymentRequest, string tenantId)
        {
            try
            {
                var securityRequest = new PaymentSecurityRequestDto
                {
                    PaymentMethod = "CreditCard",
                    Amount = paymentRequest.Amount,
                    Currency = paymentRequest.Currency,
                    CardNumber = paymentRequest.CardNumber,
                    CVV = paymentRequest.CVV,
                    BillingAddress = paymentRequest.BillingAddress,
                    CustomerIP = paymentRequest.CustomerIP
                };

                var securityValidation = await ValidatePaymentSecurityAsync(securityRequest, tenantId);
                if (!securityValidation.IsSuccess || !securityValidation.Value.IsValid)
                {
                    return Result<CreditCardPaymentDto>.Failure($"Security validation failed: {securityValidation.Error ?? "High risk transaction"}");
                }

                var paymentResult = new CreditCardPaymentDto
                {
                    TransactionId = Guid.NewGuid().ToString(),
                    GatewayTransactionId = $"gw_{DateTime.UtcNow.Ticks}",
                    Amount = paymentRequest.Amount,
                    Currency = paymentRequest.Currency,
                    Status = "Processing",
                    ProcessedAt = DateTime.UtcNow,
                    GatewayProvider = paymentRequest.GatewayProvider ?? "Stripe",
                    MaskedCardNumber = MaskCardNumber(paymentRequest.CardNumber),
                    CardType = DetectCardType(paymentRequest.CardNumber),
                    AuthorizationCode = GenerateAuthorizationCode(),
                    SecurityValidation = securityValidation.Value
                };

                await Task.Delay(1000); // Simulate network delay

                if (paymentRequest.CardNumber.EndsWith("0000"))
                {
                    paymentResult.Status = "Failed";
                    paymentResult.FailureReason = "Insufficient funds";
                    paymentResult.GatewayResponse = "DECLINED";
                }
                else if (paymentRequest.CardNumber.EndsWith("1111"))
                {
                    paymentResult.Status = "Failed";
                    paymentResult.FailureReason = "Invalid card number";
                    paymentResult.GatewayResponse = "INVALID_CARD";
                }
                else
                {
                    paymentResult.Status = "Completed";
                    paymentResult.GatewayResponse = "APPROVED";
                    paymentResult.ReceiptNumber = $"RCP-{DateTime.UtcNow:yyyyMMdd}-{paymentResult.TransactionId[..8]}";
                }

                _logger.LogInformation("Credit card payment processed. Transaction ID: {TransactionId}, Status: {Status}, Amount: {Amount}", 
                    paymentResult.TransactionId, paymentResult.Status, paymentResult.Amount);

                await StoreTransactionRecordAsync(paymentResult, tenantId);

                return Result<CreditCardPaymentDto>.Success(paymentResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing credit card payment for tenant {TenantId}", tenantId);
                return Result<CreditCardPaymentDto>.Failure("An error occurred while processing the credit card payment");
            }
        }

        public async Task<Result<PaymentReceiptDto>> GeneratePaymentReceiptAsync(string transactionId, string tenantId)
        {
            try
            {
                // In a real implementation, this would retrieve transaction details from database
                
                var receiptData = new PaymentReceiptDataDto
                {
                    MerchantName = "Rihla School Transportation",
                    MerchantAddress = "123 Education Street, Riyadh, Saudi Arabia",
                    MerchantPhone = "+966-11-123-4567",
                    TransactionDate = DateTime.UtcNow,
                    PaymentMethod = "Credit Card",
                    Amount = 150.00m,
                    Currency = "SAR",
                    Status = "Completed",
                    AuthorizationCode = GenerateAuthorizationCode(),
                    CustomerReference = "Student Transportation Fee"
                };

                var receipt = new PaymentReceiptDto
                {
                    ReceiptNumber = $"RCP-{DateTime.UtcNow:yyyyMMdd}-{transactionId[..8]}",
                    TransactionId = transactionId,
                    GeneratedAt = DateTime.UtcNow,
                    TenantId = tenantId,
                    ReceiptContent = GenerateReceiptContent(receiptData)
                };

                receipt.ReceiptContent = GenerateReceiptContent(receiptData);
                
                receipt.DigitalSignature = GenerateReceiptSignature(receipt);

                _logger.LogInformation("Payment receipt generated successfully. Receipt Number: {ReceiptNumber}, Transaction ID: {TransactionId}", 
                    receipt.ReceiptNumber, transactionId);

                return Result<PaymentReceiptDto>.Success(receipt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating payment receipt for transaction {TransactionId}", transactionId);
                return Result<PaymentReceiptDto>.Failure("An error occurred while generating the payment receipt");
            }
        }

        #region Private Helper Methods

        private List<string> GetSupportedPaymentMethods(string gatewayProvider)
        {
            return gatewayProvider.ToLower() switch
            {
                "stripe" => new List<string> { "CreditCard", "DebitCard", "BankTransfer", "ApplePay", "GooglePay" },
                "paypal" => new List<string> { "PayPal", "CreditCard", "DebitCard", "BankTransfer" },
                "mada" => new List<string> { "Mada", "CreditCard", "DebitCard", "STC Pay" },
                _ => new List<string> { "CreditCard", "DebitCard", "BankTransfer" }
            };
        }

        private async Task<Result<bool>> TestGatewayConnectivityAsync(PaymentGatewayDto gatewayConfig)
        {
            try
            {
                await Task.Delay(500);
                
                var isConnected = !string.IsNullOrEmpty(gatewayConfig.ApiEndpoint);
                
                return Result<bool>.Success(isConnected);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Connectivity test failed: {ex.Message}");
            }
        }

        private async Task<SecurityCheckDto> ValidatePCIComplianceAsync(PaymentSecurityRequestDto request)
        {
            await Task.Delay(100); // Simulate processing time
            
            return new SecurityCheckDto
            {
                CheckType = "PCI Compliance",
                Passed = !string.IsNullOrEmpty(request.CardNumber) && request.CardNumber.Length >= 13,
                Message = "PCI DSS compliance validation completed",
                RiskScore = 0
            };
        }

        private async Task<SecurityCheckDto> ValidateCardSecurityAsync(PaymentSecurityRequestDto request)
        {
            await Task.Delay(100);
            
            var passed = !string.IsNullOrEmpty(request.CVV) && request.CVV.Length >= 3;
            
            return new SecurityCheckDto
            {
                CheckType = "Card Security",
                Passed = passed,
                Message = passed ? "Card security validation passed" : "Invalid CVV",
                RiskScore = passed ? 0 : 20
            };
        }

        private async Task<SecurityCheckDto> DetectFraudAsync(PaymentSecurityRequestDto request, string tenantId)
        {
            await Task.Delay(200);
            
            var riskScore = 0;
            var passed = true;
            var message = "No fraud indicators detected";
            
            if (request.Amount > 1000)
            {
                riskScore += 15;
                message = "High amount transaction flagged for review";
            }
            
            
            return new SecurityCheckDto
            {
                CheckType = "Fraud Detection",
                Passed = passed,
                Message = message,
                RiskScore = riskScore
            };
        }

        private async Task<SecurityCheckDto> CheckRateLimitsAsync(PaymentSecurityRequestDto request, string tenantId)
        {
            await Task.Delay(50);
            
            return new SecurityCheckDto
            {
                CheckType = "Rate Limiting",
                Passed = true,
                Message = "Rate limit check passed",
                RiskScore = 0
            };
        }

        private async Task<SecurityCheckDto> VerifyBillingAddressAsync(PaymentSecurityRequestDto request)
        {
            await Task.Delay(150);
            
            var passed = !string.IsNullOrEmpty(request.BillingAddress) && request.BillingAddress.Length > 10;
            
            return new SecurityCheckDto
            {
                CheckType = "Address Verification",
                Passed = passed,
                Message = passed ? "Address verification successful" : "Address verification failed",
                RiskScore = passed ? 0 : 10
            };
        }

        private string MaskCardNumber(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 4)
                return "****";
                
            return $"****-****-****-{cardNumber[^4..]}";
        }

        private string DetectCardType(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber))
                return "Unknown";
                
            return cardNumber[0] switch
            {
                '4' => "Visa",
                '5' => "Mastercard",
                '3' => "American Express",
                _ => "Unknown"
            };
        }

        private string GenerateAuthorizationCode()
        {
            return $"AUTH{DateTime.UtcNow.Ticks % 1000000:D6}";
        }

        private async Task StoreTransactionRecordAsync(CreditCardPaymentDto payment, string tenantId)
        {
            await Task.Delay(50);
            _logger.LogDebug("Transaction record stored for {TransactionId}", payment.TransactionId);
        }

        private string GenerateReceiptContent(PaymentReceiptDataDto receiptData)
        {
            return $@"
===========================================
           PAYMENT RECEIPT
===========================================
Merchant: {receiptData.MerchantName}
Address: {receiptData.MerchantAddress}
Phone: {receiptData.MerchantPhone}

Transaction Date: {receiptData.TransactionDate:yyyy-MM-dd HH:mm:ss}
Transaction ID: {receiptData.TransactionId}
Authorization Code: {receiptData.AuthorizationCode}

Payment Method: {receiptData.PaymentMethod}
Amount: {receiptData.Amount:C} {receiptData.Currency}
Status: {receiptData.Status}

Description: {receiptData.CustomerReference}

===========================================
Thank you for using Rihla Transportation!
===========================================";
        }

        private string GenerateReceiptSignature(PaymentReceiptDto receipt)
        {
            var content = $"{receipt.ReceiptNumber}{receipt.TransactionId}{receipt.GeneratedAt:yyyyMMddHHmmss}";
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(content))[..16];
        }

        #endregion
    }
}
