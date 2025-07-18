using SchoolTransportationSystem.Core.Enums;

namespace SchoolTransportationSystem.Core.Entities;

public class GeofenceAlert : BaseEntity
{
    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;
    
    public int? TripId { get; set; }
    public Trip? Trip { get; set; }
    
    public string AlertType { get; set; } = string.Empty;
    public string GeofenceName { get; set; } = string.Empty;
    public double CenterLatitude { get; set; }
    public double CenterLongitude { get; set; }
    public double Radius { get; set; }
    public double? ViolationLatitude { get; set; }
    public double? ViolationLongitude { get; set; }
    public double? Distance { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActionTaken { get; set; }
    public string? ResolvedBy { get; set; }
    public string? Notes { get; set; }
    public DateTime Timestamp { get; set; }
}
