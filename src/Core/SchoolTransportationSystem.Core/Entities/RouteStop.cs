using SchoolTransportationSystem.Core.ValueObjects;

namespace SchoolTransportationSystem.Core.Entities
{
    public class RouteStop : TenantEntity
    {
        public int RouteId { get; set; }
        public Route Route { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public Address Address { get; set; } = null!;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int StopOrder { get; set; }
        public TimeSpan ScheduledArrival { get; set; }
        public TimeSpan ScheduledDeparture { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }

        public string GetDisplayName()
        {
            return $"Stop {StopOrder}: {Name}";
        }

        public TimeSpan GetStopDuration()
        {
            return ScheduledDeparture - ScheduledArrival;
        }
    }
}

