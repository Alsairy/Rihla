using System.ComponentModel.DataAnnotations;
using SchoolTransportationSystem.Core.Enums;

namespace SchoolTransportationSystem.Application.DTOs
{
    public class DailyScheduleRequestDto
    {
        [Required]
        public DateTime Date { get; set; }
        
        public List<int>? RouteIds { get; set; }
        public List<int>? VehicleIds { get; set; }
        public List<int>? DriverIds { get; set; }
        public bool IncludeWeekends { get; set; } = false;
    }

    public class RescheduleTripDto
    {
        [Required]
        public int TripId { get; set; }
        
        [Required]
        public DateTime NewStartTime { get; set; }
        
        [Required]
        public DateTime NewEndTime { get; set; }
        
        public int? NewVehicleId { get; set; }
        public int? NewDriverId { get; set; }
        public string? Reason { get; set; }
        public bool NotifyParents { get; set; } = true;
    }

    public class ScheduleConflictDto
    {
        public int ConflictId { get; set; }
        public ScheduleConflictType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime ConflictTime { get; set; }
        public List<int> AffectedTripIds { get; set; } = new();
        public ScheduleConflictSeverity Severity { get; set; }
        public List<string> SuggestedResolutions { get; set; } = new();
    }

    public class ScheduleOptimizationRequestDto
    {
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        public List<int>? RouteIds { get; set; }
        public OptimizationGoal Goal { get; set; } = OptimizationGoal.MinimizeTime;
        public Dictionary<string, object> Constraints { get; set; } = new();
    }

    public class ScheduleOptimizationResultDto
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public List<OptimizedTripDto> OptimizedTrips { get; set; } = new();
        public OptimizationMetricsDto Metrics { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    public class OptimizedTripDto
    {
        public int TripId { get; set; }
        public DateTime OriginalStartTime { get; set; }
        public DateTime OptimizedStartTime { get; set; }
        public DateTime OriginalEndTime { get; set; }
        public DateTime OptimizedEndTime { get; set; }
        public int? OriginalVehicleId { get; set; }
        public int? OptimizedVehicleId { get; set; }
        public int? OriginalDriverId { get; set; }
        public int? OptimizedDriverId { get; set; }
        public decimal TimeSaved { get; set; }
        public decimal FuelSaved { get; set; }
    }

    public class OptimizationMetricsDto
    {
        public decimal TotalTimeSaved { get; set; }
        public decimal TotalFuelSaved { get; set; }
        public decimal TotalCostSaved { get; set; }
        public int TripsOptimized { get; set; }
        public decimal EfficiencyImprovement { get; set; }
        public TimeSpan ProcessingTime { get; set; }
    }

    public class ScheduleAnalyticsDto
    {
        public int TotalTrips { get; set; }
        public int CompletedTrips { get; set; }
        public int CancelledTrips { get; set; }
        public int DelayedTrips { get; set; }
        public decimal OnTimePerformance { get; set; }
        public decimal AverageDelay { get; set; }
        public List<RoutePerformanceDto> RoutePerformance { get; set; } = new();
        public List<VehicleUtilizationDto> VehicleUtilization { get; set; } = new();
        public List<DriverPerformanceDto> DriverPerformance { get; set; } = new();
    }


    public class VehicleUtilizationDto
    {
        public int VehicleId { get; set; }
        public string VehicleNumber { get; set; } = string.Empty;
        public int TotalTrips { get; set; }
        public decimal TotalHours { get; set; }
        public decimal UtilizationRate { get; set; }
        public decimal FuelConsumption { get; set; }
        public decimal MaintenanceCost { get; set; }
    }

    public class DriverPerformanceDto
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public int TotalTrips { get; set; }
        public int OnTimeTrips { get; set; }
        public decimal OnTimePercentage { get; set; }
        public decimal SafetyScore { get; set; }
        public int IncidentCount { get; set; }
    }

    public enum OptimizationGoal
    {
        MinimizeTime,
        MinimizeFuel,
        MinimizeCost,
        MaximizeEfficiency,
        BalanceLoad
    }

    public enum ScheduleConflictType
    {
        VehicleConflict,
        DriverConflict,
        RouteConflict,
        TimeConflict,
        ResourceConflict
    }

    public enum ScheduleConflictSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }
}
