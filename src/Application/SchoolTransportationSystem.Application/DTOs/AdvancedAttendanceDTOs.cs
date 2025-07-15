using System.ComponentModel.DataAnnotations;
using SchoolTransportationSystem.Core.Enums;

namespace SchoolTransportationSystem.Application.DTOs
{
    public class RFIDAttendanceDto
    {
        [Required]
        public string RFIDTag { get; set; } = string.Empty;
        
        [Required]
        public int TripId { get; set; }
        
        [Required]
        public int RouteStopId { get; set; }
        
        [Required]
        public AttendanceStatus Status { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? DeviceId { get; set; }
    }

    public class PhotoAttendanceDto
    {
        [Required]
        public int StudentId { get; set; }
        
        [Required]
        public int TripId { get; set; }
        
        [Required]
        public int RouteStopId { get; set; }
        
        [Required]
        public string PhotoBase64 { get; set; } = string.Empty;
        
        [Required]
        public AttendanceStatus Status { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? ConfidenceScore { get; set; }
    }

    public class BiometricAttendanceDto
    {
        [Required]
        public int StudentId { get; set; }
        
        [Required]
        public int TripId { get; set; }
        
        [Required]
        public int RouteStopId { get; set; }
        
        [Required]
        public string BiometricData { get; set; } = string.Empty;
        
        [Required]
        public string BiometricType { get; set; } = string.Empty; // "fingerprint", "face", "iris"
        
        [Required]
        public AttendanceStatus Status { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? MatchScore { get; set; }
    }

    public class OfflineAttendanceSyncDto
    {
        [Required]
        public List<OfflineAttendanceDto> AttendanceRecords { get; set; } = new();
        
        [Required]
        public string DeviceId { get; set; } = string.Empty;
        
        public DateTime LastSyncTime { get; set; }
    }

    public class OfflineAttendanceSyncResultDto
    {
        public bool Success { get; set; }
        public int ProcessedRecords { get; set; }
        public int SuccessfulRecords { get; set; }
        public int FailedRecords { get; set; }
        public List<SyncErrorDto> SyncErrors { get; set; } = new();
        public DateTime LastSyncTime { get; set; }
    }

    public class SyncErrorDto
    {
        public int RecordIndex { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string? RecordData { get; set; }
    }

    public class AttendanceMethodDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public bool RequiresDevice { get; set; }
        public string? DeviceType { get; set; }
        public Dictionary<string, object> Configuration { get; set; } = new();
    }

    public class AttendanceAnalyticsDto
    {
        public int TotalStudents { get; set; }
        public int PresentToday { get; set; }
        public int AbsentToday { get; set; }
        public decimal AttendanceRate { get; set; }
        public List<RouteAttendanceStatsDto> RouteStats { get; set; } = new();
        public List<DailyAttendanceDto> DailyTrends { get; set; } = new();
        public List<AttendanceMethodUsageDto> MethodUsage { get; set; } = new();
    }

    public class RouteAttendanceStatsDto
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int Present { get; set; }
        public int Absent { get; set; }
        public decimal AttendanceRate { get; set; }
    }

    public class DailyAttendanceDto
    {
        public DateTime Date { get; set; }
        public int Present { get; set; }
        public int Absent { get; set; }
        public decimal AttendanceRate { get; set; }
    }

    public class AttendanceMethodUsageDto
    {
        public string Method { get; set; } = string.Empty;
        public int UsageCount { get; set; }
        public decimal Percentage { get; set; }
        public decimal AverageProcessingTime { get; set; }
    }
}
