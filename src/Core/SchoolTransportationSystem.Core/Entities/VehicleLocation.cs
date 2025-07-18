using SchoolTransportationSystem.Core.Entities;

namespace SchoolTransportationSystem.Core.Entities
{
    public class VehicleLocation : BaseEntity
    {
        public int VehicleId { get; set; }
        public virtual Vehicle Vehicle { get; set; } = null!;
        
        public int? TripId { get; set; }
        public virtual Trip? Trip { get; set; }
        
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal? Altitude { get; set; }
        public decimal Speed { get; set; }
        public decimal Heading { get; set; }
        public decimal Accuracy { get; set; }
        public string? Address { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal BatteryLevel { get; set; }
        public decimal SignalStrength { get; set; }
        public string? DeviceId { get; set; }
        public string? Notes { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsActive { get; set; } = true;
        public string TenantId { get; set; } = string.Empty;
    }
}
