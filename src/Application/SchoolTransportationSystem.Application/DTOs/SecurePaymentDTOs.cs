using System.ComponentModel.DataAnnotations;

namespace SchoolTransportationSystem.Application.DTOs
{
    public class SecurePaymentRequestDto
    {
        [Required]
        public decimal Amount { get; set; }
        
        [Required]
        public string Currency { get; set; } = "USD";
        
        [Required]
        public string CardNumber { get; set; } = string.Empty;
        
        [Required]
        public string ExpiryMonth { get; set; } = string.Empty;
        
        [Required]
        public string ExpiryYear { get; set; } = string.Empty;
        
        [Required]
        public string CVV { get; set; } = string.Empty;
        
        [Required]
        public string CardHolderName { get; set; } = string.Empty;
        
        public string? BillingAddress { get; set; }
        public string? BillingCity { get; set; }
        public string? BillingState { get; set; }
        public string? BillingZip { get; set; }
        public string? BillingCountry { get; set; }
    }

    public class SecurePaymentResultDto
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
        public string? AuthorizationCode { get; set; }
        public string? ReceiptUrl { get; set; }
    }

    public class InvoiceGenerationRequestDto
    {
        [Required]
        public int StudentId { get; set; }
        
        [Required]
        public DateTime BillingPeriodStart { get; set; }
        
        [Required]
        public DateTime BillingPeriodEnd { get; set; }
        
        public List<int> ServiceIds { get; set; } = new();
        public decimal? DiscountAmount { get; set; }
        public string? Notes { get; set; }
    }

    public class FamilyDiscountRequestDto
    {
        [Required]
        public int FamilyId { get; set; }
        
        [Required]
        public List<int> StudentIds { get; set; } = new();
        
        [Required]
        public decimal DiscountPercentage { get; set; }
        
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }

    public class FamilyDiscountResultDto
    {
        public bool Success { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class RefundRequestDto
    {
        [Required]
        public string TransactionId { get; set; } = string.Empty;
        
        [Required]
        public decimal RefundAmount { get; set; }
        
        [Required]
        public string Reason { get; set; } = string.Empty;
        
        public string? Notes { get; set; }
    }

    public class RefundResultDto
    {
        public bool Success { get; set; }
        public string RefundId { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class PaymentGatewayStatusDto
    {
        public string GatewayName { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime LastChecked { get; set; }
        public decimal? ResponseTime { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class FraudDetectionResultDto
    {
        public bool IsFraudulent { get; set; }
        public decimal RiskScore { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public List<string> Flags { get; set; } = new();
        public string? Recommendation { get; set; }
    }

    public class PaymentAnalyticsDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalTransactions { get; set; }
        public decimal AverageTransactionAmount { get; set; }
        public int SuccessfulTransactions { get; set; }
        public int FailedTransactions { get; set; }
        public decimal SuccessRate { get; set; }
        public List<PaymentMethodStatsDto> PaymentMethodStats { get; set; } = new();
        public List<DailyRevenueDto> DailyRevenue { get; set; } = new();
    }

    public class PaymentMethodStatsDto
    {
        public string Method { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int TransactionCount { get; set; }
    }
}
