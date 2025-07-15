using SchoolTransportationSystem.Core.Enums;
using SchoolTransportationSystem.Core.ValueObjects;

namespace SchoolTransportationSystem.Core.Entities
{
    public class Student : TenantEntity
    {
        public string StudentNumber { get; set; } = string.Empty;
        public FullName FullName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public string Grade { get; set; } = string.Empty;
        public string School { get; set; } = string.Empty;
        public Address Address { get; set; } = null!;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ParentName { get; set; }
        public string? ParentPhone { get; set; }
        public string? ParentEmail { get; set; }
        public int? ParentId { get; set; }
        public string? EmergencyContact { get; set; }
        public string? EmergencyPhone { get; set; }
        public StudentStatus Status { get; set; } = StudentStatus.Active;
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
        public string? SpecialNeeds { get; set; }
        public string? MedicalConditions { get; set; }
        public string? Allergies { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public int? RouteId { get; set; }
        public Route? Route { get; set; }
        public int? RouteStopId { get; set; }
        public string? RfidTag { get; set; }
        public List<Attendance> Attendances { get; set; } = new();
        public List<Payment> Payments { get; set; } = new();

        public int GetAge()
        {
            var today = DateTime.Today;
            var age = today.Year - DateOfBirth.Year;
            if (DateOfBirth.Date > today.AddYears(-age)) age--;
            return age;
        }

        public bool IsMinor()
        {
            return GetAge() < 18;
        }

        public string GetDisplayName()
        {
            return FullName.GetDisplayName();
        }
    }
}

