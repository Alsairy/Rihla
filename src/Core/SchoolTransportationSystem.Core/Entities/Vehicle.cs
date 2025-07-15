using SchoolTransportationSystem.Core.Enums;

namespace SchoolTransportationSystem.Core.Entities
{
    public class Vehicle : TenantEntity
    {
        public string VehicleNumber { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public VehicleType Type { get; set; }
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Color { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string VIN { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public int Mileage { get; set; }
        public string FuelType { get; set; } = string.Empty;
        public VehicleStatus Status { get; set; } = VehicleStatus.Active;
        public DateTime? RegistrationExpiry { get; set; }
        public DateTime? InspectionExpiry { get; set; }
        public DateTime? InsuranceExpiry { get; set; }
        public string? InsuranceProvider { get; set; }
        public string? InsurancePolicyNumber { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public int? AssignedDriverId { get; set; }
        public Driver? AssignedDriver { get; set; }
        public List<Route> Routes { get; set; } = new();
        public List<Trip> Trips { get; set; } = new();
        public List<MaintenanceRecord> MaintenanceRecords { get; set; } = new();
        public List<VehicleLocation> VehicleLocations { get; set; } = new();
        
        // GPS tracking properties
        public decimal? CurrentLatitude { get; set; }
        public decimal? CurrentLongitude { get; set; }
        public DateTime? LastLocationUpdate { get; set; }

        public int GetAge()
        {
            return DateTime.Now.Year - Year;
        }

        public bool IsRegistrationValid()
        {
            return RegistrationExpiry.HasValue && RegistrationExpiry.Value > DateTime.Today;
        }

        public bool IsInspectionValid()
        {
            return InspectionExpiry.HasValue && InspectionExpiry.Value > DateTime.Today;
        }

        public bool IsInsuranceValid()
        {
            return InsuranceExpiry.HasValue && InsuranceExpiry.Value > DateTime.Today;
        }

        public bool IsOperational()
        {
            return Status == VehicleStatus.Active && 
                   IsRegistrationValid() && 
                   IsInspectionValid() && 
                   IsInsuranceValid();
        }

        public string GetDisplayName()
        {
            return $"{VehicleNumber} - {Make} {Model} ({Year})";
        }
    }
}

