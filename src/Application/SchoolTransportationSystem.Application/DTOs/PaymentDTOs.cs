using System.ComponentModel.DataAnnotations;
using SchoolTransportationSystem.Core.Enums;

namespace SchoolTransportationSystem.Application.DTOs
{
    // Payment DTOs
    public class PaymentDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string PaymentReference { get; set; } = string.Empty;
        public PaymentType Type { get; set; }
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public DateTime DueDate { get; set; }
        public string? Description { get; set; }
        public string? TransactionId { get; set; }
        public string? GatewayReference { get; set; }
        public string? ReceiptNumber { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public string? ProcessedBy { get; set; }
        public string? Notes { get; set; }
        public decimal? RefundAmount { get; set; }
        public DateTime? RefundDate { get; set; }
        public string? RefundReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string TenantId { get; set; } = string.Empty;
        
        // Navigation properties
        public StudentDto Student { get; set; } = new();
    }

    public class CreatePaymentDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public PaymentType Type { get; set; }

        [Required]
        public PaymentMethod Method { get; set; }

        [Required]
        [Range(0.01, 999999.99)]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "SAR";

        [Required]
        public DateTime DueDate { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string? Notes { get; set; }

        [Required]
        public string TenantId { get; set; } = string.Empty;
    }

    public class UpdatePaymentDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public PaymentType Type { get; set; }

        [Required]
        public PaymentMethod Method { get; set; }

        [Required]
        [Range(0.01, 999999.99)]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "SAR";

        [Required]
        public DateTime DueDate { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string? Notes { get; set; }
    }

    public class ProcessPaymentDto
    {
        [Required]
        public int PaymentId { get; set; }

        [Required]
        [StringLength(100)]
        public string TransactionId { get; set; } = string.Empty;

        [StringLength(100)]
        public string? GatewayReference { get; set; }

        [Required]
        public DateTime ProcessedDate { get; set; }

        [Required]
        [StringLength(100)]
        public string ProcessedBy { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Notes { get; set; }
    }

    public class RefundPaymentDto
    {
        [Required]
        public int PaymentId { get; set; }

        [Required]
        [Range(0.01, 999999.99)]
        public decimal RefundAmount { get; set; }

        [Required]
        [StringLength(200)]
        public string RefundReason { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        [StringLength(100)]
        public string ProcessedBy { get; set; } = string.Empty;
    }

    public class PaymentSearchDto
    {
        public int? StudentId { get; set; }
        public string? PaymentReference { get; set; }
        public PaymentType? Type { get; set; }
        public PaymentMethod? Method { get; set; }
        public PaymentStatus? Status { get; set; }
        public decimal? AmountFrom { get; set; }
        public decimal? AmountTo { get; set; }
        public DateTime? PaymentDateFrom { get; set; }
        public DateTime? PaymentDateTo { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }
        public bool? IsOverdue { get; set; }
        public string? TenantId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }

    public class PaymentStatisticsDto
    {
        public int TotalPayments { get; set; }
        public int PendingPayments { get; set; }
        public int CompletedPayments { get; set; }
        public int FailedPayments { get; set; }
        public int RefundedPayments { get; set; }
        public int OverduePayments { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal CompletedAmount { get; set; }
        public decimal RefundedAmount { get; set; }
        public decimal OverdueAmount { get; set; }
        public decimal CollectionRate { get; set; }
        public decimal AveragePaymentAmount { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class StudentPaymentSummaryDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentNumber { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public decimal TotalOwed { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal OverdueAmount { get; set; }
        public int TotalPayments { get; set; }
        public int PendingPayments { get; set; }
        public int OverduePayments { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public DateTime? NextDueDate { get; set; }
        public PaymentStatus AccountStatus { get; set; }
        public List<PaymentDto> RecentPayments { get; set; } = new();
    }

    public class PaymentReportDto
    {
        public string ReportType { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal CollectedAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal RefundedAmount { get; set; }
        public decimal CollectionRate { get; set; }
        public List<PaymentMethodSummaryDto> MethodBreakdown { get; set; } = new();
        public List<PaymentTypeSummaryDto> TypeBreakdown { get; set; } = new();
        public List<MonthlyPaymentSummaryDto> MonthlyTrends { get; set; } = new();
    }

    public class PaymentMethodSummaryDto
    {
        public PaymentMethod Method { get; set; }
        public string MethodName { get; set; } = string.Empty;
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Percentage { get; set; }
        public decimal AverageAmount { get; set; }
    }

    public class PaymentTypeSummaryDto
    {
        public PaymentType Type { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Percentage { get; set; }
        public decimal AverageAmount { get; set; }
    }

    public class MonthlyPaymentSummaryDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal CollectedAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal CollectionRate { get; set; }
    }

    public class PaymentReminderDto
    {
        public int PaymentId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string ParentName { get; set; } = string.Empty;
        public string ParentEmail { get; set; } = string.Empty;
        public string ParentPhone { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysOverdue { get; set; }
        public string ReminderType { get; set; } = string.Empty;
        public DateTime LastReminderSent { get; set; }
        public int ReminderCount { get; set; }
        public bool IsUrgent { get; set; }
    }

    public class PaymentReceiptDto
    {
        public int PaymentId { get; set; }
        public string ReceiptNumber { get; set; } = string.Empty;
        public DateTime ReceiptDate { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentNumber { get; set; } = string.Empty;
        public string ParentName { get; set; } = string.Empty;
        public PaymentType Type { get; set; }
        public PaymentMethod Method { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public DateTime ProcessedDate { get; set; }
        public string ProcessedBy { get; set; } = string.Empty;
        public string SchoolName { get; set; } = string.Empty;
        public string SchoolAddress { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class PaymentPlanDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int InstallmentCount { get; set; }
        public decimal InstallmentAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public PaymentStatus Status { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PaymentInstallmentDto> Installments { get; set; } = new();
    }

    public class PaymentInstallmentDto
    {
        public int Id { get; set; }
        public int PaymentPlanId { get; set; }
        public int InstallmentNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public PaymentStatus Status { get; set; }
        public int? PaymentId { get; set; }
        public DateTime? PaidDate { get; set; }
        public string? Notes { get; set; }
    }

    public class CreatePaymentPlanDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        [StringLength(100)]
        public string PlanName { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 999999.99)]
        public decimal TotalAmount { get; set; }

        [Required]
        [Range(2, 12)]
        public int InstallmentCount { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public string TenantId { get; set; } = string.Empty;
    }

    public class PaymentGatewayDto
    {
        public string GatewayName { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string GatewayReference { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? ResponseCode { get; set; }
        public string? ResponseMessage { get; set; }
        public Dictionary<string, string> AdditionalData { get; set; } = new();
    }
}


    public class GenerateInvoiceDto
    {
        public int StudentId { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class InvoiceDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class ApplyDiscountDto
    {
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string AppliedBy { get; set; } = string.Empty;
    }

    public class PaymentMethodDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? Description { get; set; }
    }

    public class ValidatePaymentDto
    {
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string? Reference { get; set; }
    }

    public class PaymentValidationDto
    {
        public bool IsValid { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public string? TransactionId { get; set; }
        public DateTime ValidationDate { get; set; }
    }

