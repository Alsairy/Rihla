using SchoolTransportationSystem.Core.Enums;

namespace SchoolTransportationSystem.Core.Entities;

public class PaymentGateway : BaseEntity
{
    public string GatewayName { get; set; } = string.Empty;
    public string GatewayType { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string? ExternalTransactionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public string? CardLast4 { get; set; }
    public string? CardType { get; set; }
    public string? ProcessorResponse { get; set; }
    public string? SecurityToken { get; set; }
    public decimal? FraudScore { get; set; }
    public decimal? ProcessingFee { get; set; }
    public decimal? RefundAmount { get; set; }
    public string? Notes { get; set; }
    public int PaymentId { get; set; }
    public DateTime? ProcessedAt { get; set; }
    
    // Navigation properties
    public Payment Payment { get; set; } = null!;
}
