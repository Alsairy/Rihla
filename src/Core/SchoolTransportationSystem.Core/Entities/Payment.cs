using SchoolTransportationSystem.Core.Enums;

namespace SchoolTransportationSystem.Core.Entities
{
    public class Payment : TenantEntity
    {
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;
        public string PaymentNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public PaymentType Type { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public DateTime DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public string? Description { get; set; }
        public string? Notes { get; set; }

        public bool IsPaid()
        {
            return Status == PaymentStatus.Completed;
        }

        public bool IsOverdue()
        {
            return !IsPaid() && DueDate < DateTime.Today;
        }

        public bool IsPending()
        {
            return Status == PaymentStatus.Pending || Status == PaymentStatus.Processing;
        }

        public int GetDaysOverdue()
        {
            if (!IsOverdue()) return 0;
            return (DateTime.Today - DueDate).Days;
        }

        public string GetDisplayAmount()
        {
            return $"{Currency} {Amount:F2}";
        }
    }
}

