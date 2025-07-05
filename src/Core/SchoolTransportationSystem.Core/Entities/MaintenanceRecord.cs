namespace Rihla.Core.Entities
{
    public class MaintenanceRecord : TenantEntity
    {
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;
        public string MaintenanceType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal Cost { get; set; }
        public string Currency { get; set; } = "USD";
        public string? ServiceProvider { get; set; }
        public string? InvoiceNumber { get; set; }
        public int? MileageAtService { get; set; }
        public string? PartsReplaced { get; set; }
        public string? Notes { get; set; }
        public bool IsCompleted { get; set; } = false;

        public bool IsOverdue()
        {
            return !IsCompleted && ScheduledDate < DateTime.Today;
        }

        public int GetDaysOverdue()
        {
            if (!IsOverdue()) return 0;
            return (DateTime.Today - ScheduledDate).Days;
        }

        public string GetDisplayCost()
        {
            return $"{Currency} {Cost:F2}";
        }
    }
}

