using SchoolTransportationSystem.Core.Enums;
using SchoolTransportationSystem.Core.ValueObjects;

namespace SchoolTransportationSystem.Core.Entities
{
    public class Driver : TenantEntity
    {
        public string EmployeeNumber { get; set; } = string.Empty;
        public FullName FullName { get; set; } = null!;
        public string LicenseNumber { get; set; } = string.Empty;
        public DateTime LicenseExpiry { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Address Address { get; set; } = null!;
        public DateTime HireDate { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DriverStatus Status { get; set; } = DriverStatus.Active;
        public string? EmergencyContact { get; set; }
        public string? EmergencyPhone { get; set; }
        public DateTime? MedicalCertExpiry { get; set; }
        public DateTime? BackgroundCheckDate { get; set; }
        public DateTime? LastTrainingDate { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public List<Vehicle> Vehicles { get; set; } = new();
        public List<Route> Routes { get; set; } = new();
        public List<Trip> Trips { get; set; } = new();

        public int GetAge()
        {
            var today = DateTime.Today;
            var age = today.Year - DateOfBirth.Year;
            if (DateOfBirth.Date > today.AddYears(-age)) age--;
            return age;
        }

        public bool IsLicenseValid()
        {
            return LicenseExpiry > DateTime.Today;
        }

        public bool IsMedicalCertValid()
        {
            return MedicalCertExpiry.HasValue && MedicalCertExpiry.Value > DateTime.Today;
        }

        public int GetYearsOfService()
        {
            var today = DateTime.Today;
            var years = today.Year - HireDate.Year;
            if (HireDate.Date > today.AddYears(-years)) years--;
            return years;
        }

        public string GetDisplayName()
        {
            return FullName.GetDisplayName();
        }
    }
}

