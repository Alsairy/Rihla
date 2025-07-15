using System.ComponentModel.DataAnnotations;

namespace SchoolTransportationSystem.Application.DTOs
{
    public class RouteOptimizationRequestDto
    {
        [Required]
        public int RouteId { get; set; }
        
        public OptimizationType Type { get; set; } = OptimizationType.Distance;
        public OptimizationType OptimizationType { get; set; } = OptimizationType.Distance;
        public int? VehicleCapacity { get; set; }
        public decimal? OriginalDistance { get; set; }
        public TimeSpan? OriginalDuration { get; set; }
        public List<OptimizationConstraintDto> Constraints { get; set; } = new();
        public Dictionary<string, object> Parameters { get; set; } = new();
    }

    public class OptimizationConstraintDto
    {
        public string Type { get; set; } = string.Empty;
        public object Value { get; set; } = new();
        public string? Description { get; set; }
    }

    public enum OptimizationType
    {
        Distance,
        Time,
        Fuel,
        Cost,
        Hybrid
    }
}
