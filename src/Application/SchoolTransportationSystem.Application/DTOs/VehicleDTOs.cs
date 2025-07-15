
using System.ComponentModel.DataAnnotations;
using SchoolTransportationSystem.Core.Enums;

namespace SchoolTransportationSystem.Application.DTOs
{
    // Vehicle DTOs
    public class VehicleDto
    {
        public int Id { get; set; }
        public string VehicleNumber { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public VehicleType Type { get; set; }
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Color { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string VIN { get; set; } = string.Empty;
        public VehicleStatus Status { get; set; }
        public int Mileage { get; set; }
        public string FuelType { get; set; } = string.Empty;
        public DateTime? RegistrationExpiry { get; set; }
        public DateTime? InsuranceExpiry { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public DateTime LastMaintenanceDate { get; set; }
        public DateTime NextMaintenanceDate { get; set; }
        public string? GPSDeviceId { get; set; }
        public decimal? CurrentLatitude { get; set; }
        public decimal? CurrentLongitude { get; set; }
        public DateTime? LastLocationUpdate { get; set; }
        public decimal FuelLevel { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int TenantId { get; set; }
    }

    public class CreateVehicleDto
    {
        [Required]
        [StringLength(50)]
        public string VehicleNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string LicensePlate { get; set; } = string.Empty;

        [Required]
        public VehicleType Type { get; set; }

        [Required]
        [StringLength(50)]
        public string Make { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Model { get; set; } = string.Empty;

        [Range(1900, 2100)]
        public int Year { get; set; }

        [Range(1, 100)]
        public int Capacity { get; set; }

        public VehicleStatus Status { get; set; } = VehicleStatus.Active;

        [Range(0, 999999.99)]
        public decimal Mileage { get; set; }

        public DateTime LastMaintenanceDate { get; set; }
        public DateTime NextMaintenanceDate { get; set; }

        [StringLength(100)]
        public string? GPSDeviceId { get; set; }

        [Range(-90, 90)]
        public decimal? CurrentLatitude { get; set; }

        [Range(-180, 180)]
        public decimal? CurrentLongitude { get; set; }

        [Range(0, 100)]
        public decimal FuelLevel { get; set; } = 100;

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        public string TenantId { get; set; } = string.Empty;
    }

    public class UpdateVehicleDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string VehicleNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string LicensePlate { get; set; } = string.Empty;

        [Required]
        public VehicleType Type { get; set; }

        [Required]
        [StringLength(50)]
        public string Make { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Model { get; set; } = string.Empty;

        [Range(1900, 2100)]
        public int Year { get; set; }

        [Range(1, 100)]
        public int Capacity { get; set; }

        public VehicleStatus Status { get; set; }

        [Range(0, 999999.99)]
        public decimal Mileage { get; set; }

        public DateTime LastMaintenanceDate { get; set; }
        public DateTime NextMaintenanceDate { get; set; }

        [StringLength(100)]
        public string? GPSDeviceId { get; set; }

        [Range(-90, 90)]
        public decimal? CurrentLatitude { get; set; }

        [Range(-180, 180)]
        public decimal? CurrentLongitude { get; set; }

        [Range(0, 100)]
        public decimal FuelLevel { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }


    public class VehicleMaintenanceDto
    {
        public int VehicleId { get; set; }
        public string VehicleNumber { get; set; } = string.Empty;
        public DateTime LastMaintenanceDate { get; set; }
        public DateTime NextMaintenanceDate { get; set; }
        public decimal Mileage { get; set; }
        public bool IsMaintenanceDue { get; set; }
        public int DaysUntilMaintenance { get; set; }
        public List<MaintenanceRecordDto> RecentMaintenance { get; set; } = new();
    }

    public class VehicleSearchDto
    {
        public string? VehicleNumber { get; set; }
        public string? LicensePlate { get; set; }
        public VehicleType? Type { get; set; }
        public VehicleStatus? Status { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public int? YearFrom { get; set; }
        public int? YearTo { get; set; }
        public bool? IsActive { get; set; }
        public string? TenantId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }

    public class VehicleStatisticsDto
    {
        public int TotalVehicles { get; set; }
        public int ActiveVehicles { get; set; }
        public int AvailableVehicles { get; set; }
        public int InUseVehicles { get; set; }
        public int MaintenanceVehicles { get; set; }
        public int OutOfServiceVehicles { get; set; }
        public decimal AverageAge { get; set; }
        public decimal AverageMileage { get; set; }
        public decimal UtilizationRate { get; set; }
        public int VehiclesDueMaintenance { get; set; }
    }
}


    public class VehicleSearchDto
    {
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public string? VehicleType { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }

