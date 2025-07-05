using System.ComponentModel.DataAnnotations;
using Rihla.Core.Enums;

namespace Rihla.Application.DTOs
{
    public class StudentDto
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Grade { get; set; } = string.Empty;
        public string School { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string ParentName { get; set; } = string.Empty;
        public string ParentPhone { get; set; } = string.Empty;
        public string? ParentEmail { get; set; }
        public string? EmergencyContact { get; set; }
        public string? EmergencyPhone { get; set; }
        public StudentStatus Status { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string? SpecialNeeds { get; set; }
        public string? MedicalConditions { get; set; }
        public string? Allergies { get; set; }
        public string? Notes { get; set; }
        public int? RouteId { get; set; }
        public string? RouteName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateStudentDto
    {
        [Required]
        [StringLength(50)]
        public string StudentNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? MiddleName { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(50)]
        public string Grade { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string School { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Street { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string ZipCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; }

        [EmailAddress]
        [StringLength(200)]
        public string? Email { get; set; }

        [Required]
        [StringLength(200)]
        public string ParentName { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(20)]
        public string ParentPhone { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(200)]
        public string? ParentEmail { get; set; }

        [StringLength(200)]
        public string? EmergencyContact { get; set; }

        [Phone]
        [StringLength(20)]
        public string? EmergencyPhone { get; set; }

        public StudentStatus Status { get; set; } = StudentStatus.Active;

        [Required]
        public DateTime EnrollmentDate { get; set; }

        public string? SpecialNeeds { get; set; }

        [StringLength(1000)]
        public string? MedicalConditions { get; set; }

        [StringLength(1000)]
        public string? Allergies { get; set; }

        [StringLength(2000)]
        public string? Notes { get; set; }

        public int? RouteId { get; set; }
    }

    public class UpdateStudentDto
    {
        [Required]
        [StringLength(50)]
        public string StudentNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? MiddleName { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(50)]
        public string Grade { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string School { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Street { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string ZipCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; }

        [EmailAddress]
        [StringLength(200)]
        public string? Email { get; set; }

        [Required]
        [StringLength(200)]
        public string ParentName { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(20)]
        public string ParentPhone { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(200)]
        public string? ParentEmail { get; set; }

        [StringLength(200)]
        public string? EmergencyContact { get; set; }

        [Phone]
        [StringLength(20)]
        public string? EmergencyPhone { get; set; }

        public StudentStatus Status { get; set; }

        public string? SpecialNeeds { get; set; }

        [StringLength(1000)]
        public string? MedicalConditions { get; set; }

        [StringLength(1000)]
        public string? Allergies { get; set; }

        [StringLength(2000)]
        public string? Notes { get; set; }

        public int? RouteId { get; set; }
    }

    public class StudentSearchDto
    {
        public string? SearchTerm { get; set; }
        public string? Grade { get; set; }
        public string? School { get; set; }
        public StudentStatus? Status { get; set; }
        public int? RouteId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }

    public class StudentTransportationHistoryDto
    {
        public DateTime Date { get; set; }
        public string TripType { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public string VehicleNumber { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;
        public DateTime? PickupTime { get; set; }
        public DateTime? DropoffTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class SchoolStatisticsDto
    {
        public string SchoolName { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int InactiveStudents { get; set; }
        public int StudentsWithRoutes { get; set; }
        public int StudentsWithoutRoutes { get; set; }
    }
}



    public class StudentStatisticsDto
    {
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int InactiveStudents { get; set; }
        public int StudentsWithSpecialNeeds { get; set; }
        public int StudentsWithMedicalConditions { get; set; }
        public double AverageAge { get; set; }
        public Dictionary<string, int> StudentsByGrade { get; set; } = new();
        public Dictionary<string, int> StudentsBySchool { get; set; } = new();
        public Dictionary<string, int> StudentsByStatus { get; set; } = new();
    }


    public class AssignStudentToRouteDto
    {
        public int RouteId { get; set; }
        public int RouteStopId { get; set; }
    }

