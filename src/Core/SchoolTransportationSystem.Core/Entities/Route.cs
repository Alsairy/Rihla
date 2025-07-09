using SchoolTransportationSystem.Core.Enums;

namespace SchoolTransportationSystem.Core.Entities
{
    public class Route : TenantEntity
    {
        public string RouteNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public RouteStatus Status { get; set; } = RouteStatus.Active;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal Distance { get; set; }
        public int EstimatedDuration { get; set; } // in minutes
        public string StartLocation { get; set; } = string.Empty;
        public string EndLocation { get; set; } = string.Empty;
        public string? Notes { get; set; }

        // Navigation properties
        public int? AssignedVehicleId { get; set; }
        public Vehicle? AssignedVehicle { get; set; }
        public int? AssignedDriverId { get; set; }
        public Driver? AssignedDriver { get; set; }
        public List<Student> Students { get; set; } = new();
        public List<RouteStop> RouteStops { get; set; } = new();
        public List<Trip> Trips { get; set; } = new();

        public int GetStudentCount()
        {
            return Students.Count(s => s.Status == StudentStatus.Active);
        }

        public bool IsFullyAssigned()
        {
            return AssignedVehicleId.HasValue && AssignedDriverId.HasValue;
        }

        public bool HasCapacity()
        {
            if (AssignedVehicle == null) return false;
            return GetStudentCount() < AssignedVehicle.Capacity;
        }

        public TimeSpan GetDuration()
        {
            return EndTime - StartTime;
        }

        public string GetDisplayName()
        {
            return $"{RouteNumber} - {Name}";
        }
    }
}

