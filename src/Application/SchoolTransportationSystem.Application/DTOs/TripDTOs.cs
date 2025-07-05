using System.ComponentModel.DataAnnotations;
using Rihla.Core.Enums;

namespace Rihla.Application.DTOs
{
    // Trip DTOs
    public class TripDto
    {
        public int Id { get; set; }
        public int RouteId { get; set; }
        public int VehicleId { get; set; }
        public int DriverId { get; set; }
        public DateTime TripDate { get; set; }
        public TripType Type { get; set; }
        public TripStatus Status { get; set; }
        public DateTime ScheduledStartTime { get; set; }
        public DateTime? ActualStartTime { get; set; }
        public DateTime ScheduledEndTime { get; set; }
        public DateTime? ActualEndTime { get; set; }
        public decimal? ActualDistance { get; set; }
        public decimal? FuelConsumed { get; set; }
        public int StudentsPickedUp { get; set; }
        public int StudentsDroppedOff { get; set; }
        public string? Notes { get; set; }
        public string? IncidentReports { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string TenantId { get; set; } = string.Empty;
        
        // Navigation properties
        public RouteDto Route { get; set; } = new();
        public VehicleDto Vehicle { get; set; } = new();
        public DriverDto Driver { get; set; } = new();
        public List<AttendanceDto> AttendanceRecords { get; set; } = new();
    }

    public class CreateTripDto
    {
        [Required]
        public int RouteId { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        public int DriverId { get; set; }

        [Required]
        public DateTime TripDate { get; set; }

        [Required]
        public TripType Type { get; set; }

        public TripStatus Status { get; set; } = TripStatus.Scheduled;

        [Required]
        public DateTime ScheduledStartTime { get; set; }

        [Required]
        public DateTime ScheduledEndTime { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        public string TenantId { get; set; } = string.Empty;
    }

    public class UpdateTripDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int RouteId { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        public int DriverId { get; set; }

        [Required]
        public DateTime TripDate { get; set; }

        [Required]
        public TripType Type { get; set; }

        public TripStatus Status { get; set; }

        [Required]
        public DateTime ScheduledStartTime { get; set; }

        public DateTime? ActualStartTime { get; set; }

        [Required]
        public DateTime ScheduledEndTime { get; set; }

        public DateTime? ActualEndTime { get; set; }

        [Range(0, 999.99)]
        public decimal? ActualDistance { get; set; }

        [Range(0, 999.99)]
        public decimal? FuelConsumed { get; set; }

        [Range(0, 100)]
        public int StudentsPickedUp { get; set; }

        [Range(0, 100)]
        public int StudentsDroppedOff { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(1000)]
        public string? IncidentReports { get; set; }
    }

    public class StartTripDto
    {
        [Required]
        public int TripId { get; set; }

        [Required]
        public DateTime ActualStartTime { get; set; }

        [Range(-90, 90)]
        public decimal? StartLatitude { get; set; }

        [Range(-180, 180)]
        public decimal? StartLongitude { get; set; }

        [Range(0, 999999.99)]
        public decimal? StartMileage { get; set; }

        [StringLength(200)]
        public string? StartNotes { get; set; }
    }

    public class EndTripDto
    {
        [Required]
        public int TripId { get; set; }

        [Required]
        public DateTime ActualEndTime { get; set; }

        [Range(-90, 90)]
        public decimal? EndLatitude { get; set; }

        [Range(-180, 180)]
        public decimal? EndLongitude { get; set; }

        [Range(0, 999999.99)]
        public decimal? EndMileage { get; set; }

        [Range(0, 999.99)]
        public decimal? ActualDistance { get; set; }

        [Range(0, 999.99)]
        public decimal? FuelConsumed { get; set; }

        [Range(0, 100)]
        public int StudentsPickedUp { get; set; }

        [Range(0, 100)]
        public int StudentsDroppedOff { get; set; }

        [StringLength(500)]
        public string? EndNotes { get; set; }

        [StringLength(1000)]
        public string? IncidentReports { get; set; }
    }

    public class TripSearchDto
    {
        public int? RouteId { get; set; }
        public int? VehicleId { get; set; }
        public int? DriverId { get; set; }
        public DateTime? TripDateFrom { get; set; }
        public DateTime? TripDateTo { get; set; }
        public TripType? Type { get; set; }
        public TripStatus? Status { get; set; }
        public bool? HasIncidents { get; set; }
        public bool? IsDelayed { get; set; }
        public string? TenantId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }

    public class TripStatisticsDto
    {
        public int TotalTrips { get; set; }
        public int CompletedTrips { get; set; }
        public int CancelledTrips { get; set; }
        public int DelayedTrips { get; set; }
        public int OnTimeTrips { get; set; }
        public decimal OnTimePerformance { get; set; }
        public decimal AverageDelay { get; set; }
        public decimal TotalDistance { get; set; }
        public decimal TotalFuelConsumed { get; set; }
        public decimal AverageFuelEfficiency { get; set; }
        public int TotalStudentsTransported { get; set; }
        public int IncidentCount { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class TripPerformanceDto
    {
        public int TripId { get; set; }
        public string RouteNumber { get; set; } = string.Empty;
        public string VehicleNumber { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;
        public DateTime TripDate { get; set; }
        public TripType Type { get; set; }
        public TimeSpan ScheduledDuration { get; set; }
        public TimeSpan? ActualDuration { get; set; }
        public TimeSpan? Delay { get; set; }
        public decimal? DistanceVariance { get; set; }
        public decimal? FuelEfficiency { get; set; }
        public int StudentsTransported { get; set; }
        public bool HasIncidents { get; set; }
        public decimal PerformanceScore { get; set; }
    }

    public class TripTrackingDto
    {
        public int TripId { get; set; }
        public string RouteNumber { get; set; } = string.Empty;
        public string VehicleNumber { get; set; } = string.Empty;
        public TripStatus Status { get; set; }
        public decimal? CurrentLatitude { get; set; }
        public decimal? CurrentLongitude { get; set; }
        public DateTime? LastLocationUpdate { get; set; }
        public int NextStopId { get; set; }
        public string NextStopName { get; set; } = string.Empty;
        public TimeSpan? EstimatedArrival { get; set; }
        public int StudentsOnBoard { get; set; }
        public decimal? Speed { get; set; }
        public decimal? Heading { get; set; }
    }

    public class TripReportDto
    {
        public int TripId { get; set; }
        public string RouteNumber { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public string VehicleNumber { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;
        public DateTime TripDate { get; set; }
        public TripType Type { get; set; }
        public TripStatus Status { get; set; }
        public DateTime ScheduledStartTime { get; set; }
        public DateTime? ActualStartTime { get; set; }
        public DateTime ScheduledEndTime { get; set; }
        public DateTime? ActualEndTime { get; set; }
        public TimeSpan? Delay { get; set; }
        public decimal? ActualDistance { get; set; }
        public decimal? FuelConsumed { get; set; }
        public decimal? FuelEfficiency { get; set; }
        public int StudentsPickedUp { get; set; }
        public int StudentsDroppedOff { get; set; }
        public bool HasIncidents { get; set; }
        public string? IncidentSummary { get; set; }
        public decimal PerformanceScore { get; set; }
    }

    public class TripIncidentDto
    {
        public int TripId { get; set; }
        public string IncidentType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime IncidentTime { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? Location { get; set; }
        public string Severity { get; set; } = string.Empty;
        public string? ActionTaken { get; set; }
        public string ReportedBy { get; set; } = string.Empty;
        public DateTime ReportedAt { get; set; }
        public bool IsResolved { get; set; }
        public string? Resolution { get; set; }
    }
}


    public class UpdateTripLocationDto
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Notes { get; set; }
    }

