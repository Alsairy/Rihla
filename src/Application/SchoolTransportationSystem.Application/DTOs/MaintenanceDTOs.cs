using System.ComponentModel.DataAnnotations;
using Rihla.Core.Enums;

namespace Rihla.Application.DTOs
{
    // Maintenance DTOs
    public class MaintenanceRecordDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public MaintenanceType Type { get; set; }
        public MaintenanceStatus Status { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal Mileage { get; set; }
        public decimal Cost { get; set; }
        public string? ServiceProvider { get; set; }
        public string? TechnicianName { get; set; }
        public string? PartsReplaced { get; set; }
        public string? WorkPerformed { get; set; }
        public string? Notes { get; set; }
        public DateTime? NextServiceDate { get; set; }
        public decimal? NextServiceMileage { get; set; }
        public string? WarrantyInfo { get; set; }
        public string? InvoiceNumber { get; set; }
        public bool IsWarrantyWork { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string TenantId { get; set; } = string.Empty;
        
        // Navigation properties
        public VehicleDto Vehicle { get; set; } = new();
    }

    public class CreateMaintenanceRecordDto
    {
        [Required]
        public int VehicleId { get; set; }

        [Required]
        public MaintenanceType Type { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime ScheduledDate { get; set; }

        [Required]
        [Range(0, 999999.99)]
        public decimal Mileage { get; set; }

        [Range(0, 999999.99)]
        public decimal Cost { get; set; }

        [StringLength(200)]
        public string? ServiceProvider { get; set; }

        [StringLength(100)]
        public string? TechnicianName { get; set; }

        [StringLength(1000)]
        public string? PartsReplaced { get; set; }

        [StringLength(1000)]
        public string? WorkPerformed { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime? NextServiceDate { get; set; }

        [Range(0, 999999.99)]
        public decimal? NextServiceMileage { get; set; }

        [StringLength(500)]
        public string? WarrantyInfo { get; set; }

        [StringLength(100)]
        public string? InvoiceNumber { get; set; }

        public bool IsWarrantyWork { get; set; } = false;

        [Required]
        public string TenantId { get; set; } = string.Empty;
    }

    public class UpdateMaintenanceRecordDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public MaintenanceType Type { get; set; }

        [Required]
        public MaintenanceStatus Status { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime ScheduledDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        [Required]
        [Range(0, 999999.99)]
        public decimal Mileage { get; set; }

        [Range(0, 999999.99)]
        public decimal Cost { get; set; }

        [StringLength(200)]
        public string? ServiceProvider { get; set; }

        [StringLength(100)]
        public string? TechnicianName { get; set; }

        [StringLength(1000)]
        public string? PartsReplaced { get; set; }

        [StringLength(1000)]
        public string? WorkPerformed { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime? NextServiceDate { get; set; }

        [Range(0, 999999.99)]
        public decimal? NextServiceMileage { get; set; }

        [StringLength(500)]
        public string? WarrantyInfo { get; set; }

        [StringLength(100)]
        public string? InvoiceNumber { get; set; }

        public bool IsWarrantyWork { get; set; }
    }

    public class CompleteMaintenanceDto
    {
        [Required]
        public int MaintenanceId { get; set; }

        [Required]
        public DateTime CompletedDate { get; set; }

        [Required]
        [Range(0, 999999.99)]
        public decimal FinalCost { get; set; }

        [Required]
        [StringLength(100)]
        public string TechnicianName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? PartsReplaced { get; set; }

        [Required]
        [StringLength(1000)]
        public string WorkPerformed { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime? NextServiceDate { get; set; }

        [Range(0, 999999.99)]
        public decimal? NextServiceMileage { get; set; }

        [StringLength(100)]
        public string? InvoiceNumber { get; set; }

        [StringLength(500)]
        public string? WarrantyInfo { get; set; }
    }

    public class MaintenanceSearchDto
    {
        public int? VehicleId { get; set; }
        public string? VehicleNumber { get; set; }
        public MaintenanceType? Type { get; set; }
        public MaintenanceStatus? Status { get; set; }
        public DateTime? ScheduledDateFrom { get; set; }
        public DateTime? ScheduledDateTo { get; set; }
        public DateTime? CompletedDateFrom { get; set; }
        public DateTime? CompletedDateTo { get; set; }
        public decimal? CostFrom { get; set; }
        public decimal? CostTo { get; set; }
        public string? ServiceProvider { get; set; }
        public bool? IsOverdue { get; set; }
        public bool? IsWarrantyWork { get; set; }
        public string? TenantId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }

    public class MaintenanceStatisticsDto
    {
        public int TotalRecords { get; set; }
        public int ScheduledMaintenance { get; set; }
        public int InProgressMaintenance { get; set; }
        public int CompletedMaintenance { get; set; }
        public int OverdueMaintenance { get; set; }
        public int CancelledMaintenance { get; set; }
        public decimal TotalCost { get; set; }
        public decimal AverageCost { get; set; }
        public decimal MonthlyMaintenanceCost { get; set; }
        public decimal PreventiveMaintenanceRate { get; set; }
        public decimal EmergencyMaintenanceRate { get; set; }
        public int VehiclesNeedingMaintenance { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class VehicleMaintenanceHistoryDto
    {
        public int VehicleId { get; set; }
        public string VehicleNumber { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal CurrentMileage { get; set; }
        public DateTime LastMaintenanceDate { get; set; }
        public DateTime NextMaintenanceDate { get; set; }
        public decimal TotalMaintenanceCost { get; set; }
        public int MaintenanceCount { get; set; }
        public decimal AverageMaintenanceCost { get; set; }
        public List<MaintenanceRecordDto> MaintenanceHistory { get; set; } = new();
    }

    public class MaintenanceScheduleDto
    {
        public int VehicleId { get; set; }
        public string VehicleNumber { get; set; } = string.Empty;
        public MaintenanceType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public decimal CurrentMileage { get; set; }
        public decimal? TargetMileage { get; set; }
        public int DaysUntilDue { get; set; }
        public decimal? MilesUntilDue { get; set; }
        public bool IsOverdue { get; set; }
        public bool IsUrgent { get; set; }
        public string Priority { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
        public string? ServiceProvider { get; set; }
    }

    public class MaintenanceReportDto
    {
        public string ReportType { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int TotalVehicles { get; set; }
        public int VehiclesMaintained { get; set; }
        public int TotalMaintenanceRecords { get; set; }
        public decimal TotalMaintenanceCost { get; set; }
        public decimal AverageCostPerVehicle { get; set; }
        public decimal PreventiveMaintenancePercentage { get; set; }
        public decimal EmergencyMaintenancePercentage { get; set; }
        public List<MaintenanceTypeSummaryDto> TypeBreakdown { get; set; } = new();
        public List<VehicleMaintenanceSummaryDto> VehicleBreakdown { get; set; } = new();
        public List<MonthlyMaintenanceSummaryDto> MonthlyTrends { get; set; } = new();
    }

    public class MaintenanceTypeSummaryDto
    {
        public MaintenanceType Type { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public int RecordCount { get; set; }
        public decimal TotalCost { get; set; }
        public decimal AverageCost { get; set; }
        public decimal Percentage { get; set; }
        public int AverageFrequencyDays { get; set; }
    }

    public class VehicleMaintenanceSummaryDto
    {
        public int VehicleId { get; set; }
        public string VehicleNumber { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int MaintenanceCount { get; set; }
        public decimal TotalCost { get; set; }
        public decimal AverageCost { get; set; }
        public DateTime LastMaintenanceDate { get; set; }
        public DateTime NextMaintenanceDate { get; set; }
        public bool IsOverdue { get; set; }
    }

    public class MonthlyMaintenanceSummaryDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int RecordCount { get; set; }
        public decimal TotalCost { get; set; }
        public decimal AverageCost { get; set; }
        public int VehicleCount { get; set; }
        public decimal CostPerVehicle { get; set; }
    }

    public class MaintenanceAlertDto
    {
        public int VehicleId { get; set; }
        public string VehicleNumber { get; set; } = string.Empty;
        public MaintenanceType Type { get; set; }
        public string AlertType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime AlertDate { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysOverdue { get; set; }
        public string Severity { get; set; } = string.Empty;
        public bool IsAcknowledged { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public string? AcknowledgedBy { get; set; }
    }

    public class MaintenanceWorkOrderDto
    {
        public int Id { get; set; }
        public int MaintenanceRecordId { get; set; }
        public string WorkOrderNumber { get; set; } = string.Empty;
        public int VehicleId { get; set; }
        public string VehicleNumber { get; set; } = string.Empty;
        public MaintenanceType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime? StartedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? AssignedTechnician { get; set; }
        public string? ServiceProvider { get; set; }
        public decimal EstimatedCost { get; set; }
        public decimal? ActualCost { get; set; }
        public string? Notes { get; set; }
        public List<MaintenanceTaskDto> Tasks { get; set; } = new();
    }

    public class MaintenanceTaskDto
    {
        public int Id { get; set; }
        public int WorkOrderId { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? AssignedTo { get; set; }
        public decimal EstimatedHours { get; set; }
        public decimal? ActualHours { get; set; }
        public string? Notes { get; set; }
        public List<string> PartsUsed { get; set; } = new();
    }

    public class CreateMaintenanceWorkOrderDto
    {
        [Required]
        public int VehicleId { get; set; }

        [Required]
        public MaintenanceType Type { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Priority { get; set; } = string.Empty;

        [Required]
        public DateTime ScheduledDate { get; set; }

        [StringLength(100)]
        public string? AssignedTechnician { get; set; }

        [StringLength(200)]
        public string? ServiceProvider { get; set; }

        [Range(0, 999999.99)]
        public decimal EstimatedCost { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        public string TenantId { get; set; } = string.Empty;

        public List<CreateMaintenanceTaskDto> Tasks { get; set; } = new();
    }

    public class CreateMaintenanceTaskDto
    {
        [Required]
        [StringLength(200)]
        public string TaskName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(100)]
        public string? AssignedTo { get; set; }

        [Range(0, 999.99)]
        public decimal EstimatedHours { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}


    public class MaintenancePartDto
    {
        public int Id { get; set; }
        public string PartName { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public int Quantity { get; set; }
        public string Supplier { get; set; } = string.Empty;
        public DateTime? WarrantyExpiry { get; set; }
        public string? Notes { get; set; }
    }

    public class ScheduleMaintenanceDto
    {
        public int VehicleId { get; set; }
        public string MaintenanceType { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
        public string? Notes { get; set; }
        public List<int> RequiredParts { get; set; } = new();
    }


    public class MaintenanceProviderDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
    }

