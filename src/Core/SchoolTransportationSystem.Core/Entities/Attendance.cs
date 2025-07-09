using SchoolTransportationSystem.Core.Enums;

namespace SchoolTransportationSystem.Core.Entities
{
    public class Attendance : TenantEntity
    {
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;
        public int TripId { get; set; }
        public Trip Trip { get; set; } = null!;
        public DateTime Date { get; set; }
        public AttendanceStatus Status { get; set; }
        public DateTime? BoardingTime { get; set; }
        public DateTime? AlightingTime { get; set; }
        public string? BoardingLocation { get; set; }
        public string? AlightingLocation { get; set; }
        public string? Notes { get; set; }
        public string RecordedBy { get; set; } = string.Empty;

        public bool IsPresent()
        {
            return Status == AttendanceStatus.Present;
        }

        public bool IsAbsent()
        {
            return Status == AttendanceStatus.Absent || Status == AttendanceStatus.NoShow;
        }

        public bool IsLate()
        {
            return Status == AttendanceStatus.Late;
        }

        public TimeSpan? GetTripDuration()
        {
            if (BoardingTime.HasValue && AlightingTime.HasValue)
                return AlightingTime.Value - BoardingTime.Value;
            return null;
        }
    }
}

