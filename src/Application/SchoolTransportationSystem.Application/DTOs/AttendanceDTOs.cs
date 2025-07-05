using System.ComponentModel.DataAnnotations;
using Rihla.Core.Enums;

namespace Rihla.Application.DTOs
{
    // Attendance DTOs
    public class AttendanceDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int TripId { get; set; }
        public int RouteStopId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public AttendanceStatus Status { get; set; }
        public DateTime? BoardingTime { get; set; }
        public DateTime? AlightingTime { get; set; }
        public decimal? BoardingLatitude { get; set; }
        public decimal? BoardingLongitude { get; set; }
        public decimal? AlightingLatitude { get; set; }
        public decimal? AlightingLongitude { get; set; }
        public string? Notes { get; set; }
        public bool ParentNotified { get; set; }
        public DateTime? ParentNotificationTime { get; set; }
        public string? ExceptionReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string TenantId { get; set; } = string.Empty;
        
        // Navigation properties
        public StudentDto Student { get; set; } = new();
        public TripDto Trip { get; set; } = new();
        public RouteStopDto RouteStop { get; set; } = new();
    }

    public class CreateAttendanceDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int TripId { get; set; }

        [Required]
        public int RouteStopId { get; set; }

        [Required]
        public DateTime AttendanceDate { get; set; }

        [Required]
        public AttendanceStatus Status { get; set; }

        public DateTime? BoardingTime { get; set; }

        public DateTime? AlightingTime { get; set; }

        [Range(-90, 90)]
        public decimal? BoardingLatitude { get; set; }

        [Range(-180, 180)]
        public decimal? BoardingLongitude { get; set; }

        [Range(-90, 90)]
        public decimal? AlightingLatitude { get; set; }

        [Range(-180, 180)]
        public decimal? AlightingLongitude { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(200)]
        public string? ExceptionReason { get; set; }

        [Required]
        public string TenantId { get; set; } = string.Empty;
    }

    public class UpdateAttendanceDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public AttendanceStatus Status { get; set; }

        public DateTime? BoardingTime { get; set; }

        public DateTime? AlightingTime { get; set; }

        [Range(-90, 90)]
        public decimal? BoardingLatitude { get; set; }

        [Range(-180, 180)]
        public decimal? BoardingLongitude { get; set; }

        [Range(-90, 90)]
        public decimal? AlightingLatitude { get; set; }

        [Range(-180, 180)]
        public decimal? AlightingLongitude { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(200)]
        public string? ExceptionReason { get; set; }
    }

    public class RecordBoardingDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int TripId { get; set; }

        [Required]
        public int RouteStopId { get; set; }

        [Required]
        public DateTime BoardingTime { get; set; }

        [Range(-90, 90)]
        public decimal? Latitude { get; set; }

        [Range(-180, 180)]
        public decimal? Longitude { get; set; }

        [StringLength(200)]
        public string? Notes { get; set; }

        public bool NotifyParent { get; set; } = true;
    }

    public class RecordAlightingDto
    {
        [Required]
        public int AttendanceId { get; set; }

        [Required]
        public DateTime AlightingTime { get; set; }

        [Range(-90, 90)]
        public decimal? Latitude { get; set; }

        [Range(-180, 180)]
        public decimal? Longitude { get; set; }

        [StringLength(200)]
        public string? Notes { get; set; }

        public bool NotifyParent { get; set; } = true;
    }

    public class MarkAbsentDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int TripId { get; set; }

        [Required]
        public int RouteStopId { get; set; }

        [Required]
        [StringLength(200)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notes { get; set; }

        public bool NotifyParent { get; set; } = true;
    }

    public class AttendanceSearchDto
    {
        public int? StudentId { get; set; }
        public int? TripId { get; set; }
        public int? RouteId { get; set; }
        public int? RouteStopId { get; set; }
        public DateTime? AttendanceDateFrom { get; set; }
        public DateTime? AttendanceDateTo { get; set; }
        public AttendanceStatus? Status { get; set; }
        public bool? ParentNotified { get; set; }
        public bool? HasExceptions { get; set; }
        public string? TenantId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }

    public class AttendanceStatisticsDto
    {
        public int TotalRecords { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public int EarlyDismissalCount { get; set; }
        public int ExceptionCount { get; set; }
        public decimal AttendanceRate { get; set; }
        public decimal PunctualityRate { get; set; }
        public int ParentNotificationsSent { get; set; }
        public decimal ParentNotificationRate { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class StudentAttendanceReportDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentNumber { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public string RouteNumber { get; set; } = string.Empty;
        public int TotalDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LateDays { get; set; }
        public int EarlyDismissalDays { get; set; }
        public decimal AttendanceRate { get; set; }
        public decimal PunctualityRate { get; set; }
        public int ConsecutiveAbsences { get; set; }
        public DateTime? LastAbsenceDate { get; set; }
        public List<AttendanceDto> RecentRecords { get; set; } = new();
    }

    public class RouteAttendanceReportDto
    {
        public int RouteId { get; set; }
        public string RouteNumber { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int AverageDaily { get; set; }
        public decimal UtilizationRate { get; set; }
        public decimal AttendanceRate { get; set; }
        public decimal PunctualityRate { get; set; }
        public int TotalExceptions { get; set; }
        public DateTime ReportDate { get; set; }
        public List<RouteStopAttendanceDto> StopAttendance { get; set; } = new();
    }

    public class RouteStopAttendanceDto
    {
        public int RouteStopId { get; set; }
        public string StopName { get; set; } = string.Empty;
        public int StudentsAssigned { get; set; }
        public int AverageBoarding { get; set; }
        public int AverageAlighting { get; set; }
        public decimal UtilizationRate { get; set; }
        public TimeSpan AverageBoardingTime { get; set; }
        public TimeSpan AverageAlightingTime { get; set; }
        public int ExceptionCount { get; set; }
    }

    public class AttendanceAlertDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime AlertTime { get; set; }
        public string Severity { get; set; } = string.Empty;
        public bool IsResolved { get; set; }
        public string? Resolution { get; set; }
        public List<string> NotificationChannels { get; set; } = new();
    }

    public class AttendanceExceptionDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int TripId { get; set; }
        public string RouteNumber { get; set; } = string.Empty;
        public DateTime ExceptionDate { get; set; }
        public string ExceptionType { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ReportedBy { get; set; } = string.Empty;
        public DateTime ReportedAt { get; set; }
        public bool IsResolved { get; set; }
        public string? Resolution { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolvedBy { get; set; }
    }

    public class AttendanceTrendDto
    {
        public DateTime Date { get; set; }
        public int TotalStudents { get; set; }
        public int PresentStudents { get; set; }
        public int AbsentStudents { get; set; }
        public int LateStudents { get; set; }
        public decimal AttendanceRate { get; set; }
        public decimal PunctualityRate { get; set; }
        public int ExceptionCount { get; set; }
        public string? Notes { get; set; }
    }

    public class ParentNotificationDto
    {
        public int AttendanceId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string ParentName { get; set; } = string.Empty;
        public string ParentEmail { get; set; } = string.Empty;
        public string ParentPhone { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime ScheduledTime { get; set; }
        public DateTime? SentTime { get; set; }
        public bool IsDelivered { get; set; }
        public string? DeliveryStatus { get; set; }
        public string Channel { get; set; } = string.Empty;
    }
}


    public class TripAttendanceSummaryDto
    {
        public int TripId { get; set; }
        public string TripName { get; set; } = string.Empty;
        public DateTime TripDate { get; set; }
        public int TotalStudents { get; set; }
        public int PresentStudents { get; set; }
        public int AbsentStudents { get; set; }
        public int LateStudents { get; set; }
        public double AttendanceRate { get; set; }
        public List<object> AttendanceRecords { get; set; } = new();
    }

