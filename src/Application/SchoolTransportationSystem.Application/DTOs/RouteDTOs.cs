using System.ComponentModel.DataAnnotations;
using SchoolTransportationSystem.Core.Enums;

namespace SchoolTransportationSystem.Application.DTOs
{
    // Route DTOs
    public class RouteDto
    {
        public int Id { get; set; }
        public string RouteNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public RouteType Type { get; set; }
        public RouteStatus Status { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal EstimatedDistance { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public int MaxCapacity { get; set; }
        public int CurrentOccupancy { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string TenantId { get; set; } = string.Empty;
        
        // Navigation properties
        public List<RouteStopDto> RouteStops { get; set; } = new();
        public List<StudentDto> AssignedStudents { get; set; } = new();
        public VehicleDto? AssignedVehicle { get; set; }
        public DriverDto? AssignedDriver { get; set; }
    }

    public class CreateRouteDto
    {
        [Required]
        [StringLength(20)]
        public string RouteNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public RouteType Type { get; set; }

        public RouteStatus Status { get; set; } = RouteStatus.Active;

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Range(0, 999.99)]
        public decimal EstimatedDistance { get; set; }

        [Required]
        public TimeSpan EstimatedDuration { get; set; }

        [Range(1, 100)]
        public int MaxCapacity { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        public string TenantId { get; set; } = string.Empty;

        public List<CreateRouteStopDto> RouteStops { get; set; } = new();
    }

    public class UpdateRouteDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string RouteNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public RouteType Type { get; set; }

        public RouteStatus Status { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Range(0, 999.99)]
        public decimal EstimatedDistance { get; set; }

        [Required]
        public TimeSpan EstimatedDuration { get; set; }

        [Range(1, 100)]
        public int MaxCapacity { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class RouteStopDto
    {
        public int Id { get; set; }
        public int RouteId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string StopName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int StopOrder { get; set; }
        public int SequenceNumber { get; set; }
        public TimeSpan ScheduledArrivalTime { get; set; }
        public TimeSpan EstimatedArrivalTime { get; set; }
        public TimeSpan ScheduledDepartureTime { get; set; }
        public bool IsPickupPoint { get; set; }
        public bool IsDropoffPoint { get; set; }
        public string? Landmarks { get; set; }
        public string? SpecialInstructions { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public List<StudentDto> StudentsAtStop { get; set; } = new();
    }

    public class CreateRouteStopDto
    {
        [Required]
        [StringLength(100)]
        public string StopName { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [Range(-90, 90)]
        public decimal Latitude { get; set; }

        [Required]
        [Range(-180, 180)]
        public decimal Longitude { get; set; }

        [Required]
        [Range(1, 100)]
        public int StopOrder { get; set; }

        [Required]
        public TimeSpan ScheduledArrivalTime { get; set; }

        [Required]
        public TimeSpan ScheduledDepartureTime { get; set; }

        public bool IsPickupPoint { get; set; } = true;
        public bool IsDropoffPoint { get; set; } = true;

        [StringLength(200)]
        public string? Landmarks { get; set; }

        [StringLength(500)]
        public string? SpecialInstructions { get; set; }
    }

    public class UpdateRouteStopDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string StopName { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [Range(-90, 90)]
        public decimal Latitude { get; set; }

        [Required]
        [Range(-180, 180)]
        public decimal Longitude { get; set; }

        [Required]
        [Range(1, 100)]
        public int StopOrder { get; set; }

        [Required]
        public TimeSpan ScheduledArrivalTime { get; set; }

        [Required]
        public TimeSpan ScheduledDepartureTime { get; set; }

        public bool IsPickupPoint { get; set; }
        public bool IsDropoffPoint { get; set; }

        [StringLength(200)]
        public string? Landmarks { get; set; }

        [StringLength(500)]
        public string? SpecialInstructions { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class RouteAssignmentDto
    {
        public int RouteId { get; set; }
        public string RouteNumber { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public int? VehicleId { get; set; }
        public string? VehicleNumber { get; set; }
        public int? DriverId { get; set; }
        public string? DriverName { get; set; }
        public DateTime AssignmentDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class RouteOptimizationDto
    {
        public int RouteId { get; set; }
        public string RouteNumber { get; set; } = string.Empty;
        public decimal CurrentDistance { get; set; }
        public TimeSpan CurrentDuration { get; set; }
        public decimal OptimizedDistance { get; set; }
        public TimeSpan OptimizedDuration { get; set; }
        public decimal DistanceSavings { get; set; }
        public TimeSpan TimeSavings { get; set; }
        public decimal FuelSavings { get; set; }
        public List<RouteStopDto> OptimizedStops { get; set; } = new();
    }

    public class RouteSearchDto
    {
        public string? RouteNumber { get; set; }
        public string? Name { get; set; }
        public RouteType? Type { get; set; }
        public RouteStatus? Status { get; set; }
        public TimeSpan? StartTimeFrom { get; set; }
        public TimeSpan? StartTimeTo { get; set; }
        public int? VehicleId { get; set; }
        public int? DriverId { get; set; }
        public bool? IsActive { get; set; }
        public string? TenantId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }

    public class RouteStatisticsDto
    {
        public int TotalRoutes { get; set; }
        public int ActiveRoutes { get; set; }
        public int MorningRoutes { get; set; }
        public int AfternoonRoutes { get; set; }
        public int SpecialRoutes { get; set; }
        public decimal AverageDistance { get; set; }
        public TimeSpan AverageDuration { get; set; }
        public decimal TotalDistance { get; set; }
        public int TotalStops { get; set; }
        public decimal AverageOccupancy { get; set; }
        public decimal UtilizationRate { get; set; }
    }

    public class RoutePerformanceDto
    {
        public int RouteId { get; set; }
        public string RouteNumber { get; set; } = string.Empty;
        public decimal OnTimePerformance { get; set; }
        public decimal AverageDelay { get; set; }
        public int TotalTrips { get; set; }
        public int OnTimeTrips { get; set; }
        public int DelayedTrips { get; set; }
        public decimal FuelEfficiency { get; set; }
        public decimal StudentSatisfaction { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class OptimalRouteRequestDto
    {
        [Required]
        [StringLength(100)]
        public string? RouteName { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        [Range(1, 100)]
        public int VehicleCapacity { get; set; }

        [Required]
        public List<RouteStopDto> Stops { get; set; } = new();

        public RouteType Type { get; set; } = RouteType.Regular;
        
        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class RouteEfficiencyMetricsDto
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public decimal TotalDistance { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public int NumberOfStops { get; set; }
        public int StudentCapacity { get; set; }
        public TimeSpan AverageActualDuration { get; set; }
        public decimal OnTimePerformance { get; set; }
        public decimal FuelEfficiency { get; set; }
        public decimal CostPerStudent { get; set; }
        public decimal OptimizationScore { get; set; }
        public DateTime LastCalculated { get; set; }
    }

    public class RouteOptimizationResultDto
    {
        public int RouteId { get; set; }
        public string RouteNumber { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public decimal OriginalDistance { get; set; }
        public decimal OptimizedDistance { get; set; }
        public decimal DistanceSavings { get; set; }
        public TimeSpan OriginalDuration { get; set; }
        public TimeSpan OptimizedDuration { get; set; }
        public TimeSpan TimeSavings { get; set; }
        public decimal FuelSavings { get; set; }
        public decimal CostSavings { get; set; }
        public List<RouteStopDto> OptimizedStops { get; set; } = new();
        public DateTime OptimizedAt { get; set; }
    }
}

