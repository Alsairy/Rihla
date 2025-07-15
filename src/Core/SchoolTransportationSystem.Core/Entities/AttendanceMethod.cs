using SchoolTransportationSystem.Core.Enums;

namespace SchoolTransportationSystem.Core.Entities;

public class AttendanceMethod : BaseEntity
{
    public int AttendanceId { get; set; }
    public Attendance Attendance { get; set; } = null!;
    
    public string MethodType { get; set; } = string.Empty;
    public string? DeviceId { get; set; }
    public string? RFIDTag { get; set; }
    public string? PhotoPath { get; set; }
    public string? BiometricData { get; set; }
    public string? QRCode { get; set; }
    public decimal? Confidence { get; set; }
    public decimal? ProcessingTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? Metadata { get; set; }
    public DateTime Timestamp { get; set; }
}
