using SchoolTransportationSystem.Core.Enums;

namespace SchoolTransportationSystem.Core.Entities;

public class RouteOptimization : BaseEntity
{
    public int RouteId { get; set; }
    public Route Route { get; set; } = null!;
    
    public string OptimizationType { get; set; } = string.Empty;
    public string? Parameters { get; set; }
    public string? Results { get; set; }
    public decimal OriginalDistance { get; set; }
    public decimal OptimizedDistance { get; set; }
    public decimal OriginalDuration { get; set; }
    public decimal OptimizedDuration { get; set; }
    public decimal TimeSavings { get; set; }
    public decimal FuelSavings { get; set; }
    public decimal CostSavings { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OptimizedAt { get; set; }
    public string OptimizedBy { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string TenantId { get; set; } = string.Empty;
}
