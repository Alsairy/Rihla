using Rihla.Core.Enums;

namespace Rihla.Application.DTOs
{
    public class DriverDto
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public DateTime LicenseExpiry { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = "USA";
        public DateTime HireDate { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DriverStatus Status { get; set; }
        public string? EmergencyContact { get; set; }
        public string? EmergencyPhone { get; set; }
        public DateTime? MedicalCertExpiry { get; set; }
        public DateTime? BackgroundCheckDate { get; set; }
        public DateTime? LastTrainingDate { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string FullName => string.IsNullOrEmpty(MiddleName) 
            ? $"{FirstName} {LastName}"
            : $"{FirstName} {MiddleName} {LastName}";

        public string FullAddress => $"{Street}, {City}, {State} {ZipCode}, {Country}";

        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - DateOfBirth.Year;
                if (DateOfBirth.Date > today.AddYears(-age)) age--;
                return age;
            }
        }

        public bool IsLicenseValid => LicenseExpiry > DateTime.Today;
        public bool IsMedicalCertValid => MedicalCertExpiry.HasValue && MedicalCertExpiry.Value > DateTime.Today;
        
        public int YearsOfService
        {
            get
            {
                var today = DateTime.Today;
                var years = today.Year - HireDate.Year;
                if (HireDate.Date > today.AddYears(-years)) years--;
                return years;
            }
        }
    }

    public class CreateDriverDto
    {
        public int TenantId { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public DateTime LicenseExpiry { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = "USA";
        public DateTime HireDate { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DriverStatus Status { get; set; } = DriverStatus.Active;
        public string? EmergencyContact { get; set; }
        public string? EmergencyPhone { get; set; }
        public DateTime? MedicalCertExpiry { get; set; }
        public DateTime? BackgroundCheckDate { get; set; }
        public DateTime? LastTrainingDate { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateDriverDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public DateTime LicenseExpiry { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = "USA";
        public DateTime DateOfBirth { get; set; }
        public DriverStatus Status { get; set; }
        public string? EmergencyContact { get; set; }
        public string? EmergencyPhone { get; set; }
        public DateTime? MedicalCertExpiry { get; set; }
        public DateTime? BackgroundCheckDate { get; set; }
        public DateTime? LastTrainingDate { get; set; }
        public string? Notes { get; set; }
    }

    public class DriverSummaryDto
    {
        public int Id { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DriverStatus Status { get; set; }
        public bool IsLicenseValid { get; set; }
        public bool IsMedicalCertValid { get; set; }
        public int AssignedVehiclesCount { get; set; }
        public int AssignedRoutesCount { get; set; }
    }

    public class DriverSearchDto
    {
        public int? TenantId { get; set; }
        public string? EmployeeNumber { get; set; }
        public string? Name { get; set; }
        public string? LicenseNumber { get; set; }
        public DriverStatus? Status { get; set; }
        public bool? LicenseExpiring { get; set; }
        public bool? MedicalCertExpiring { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }
}


    public class DriverSearchDto
    {
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }

