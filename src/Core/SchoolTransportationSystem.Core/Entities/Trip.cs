using Rihla.Core.Enums;

namespace Rihla.Core.Entities
{
    public class Trip : TenantEntity
    {
        public int RouteId { get; set; }
        public Route Route { get; set; } = null!;
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;
        public int DriverId { get; set; }
        public Driver Driver { get; set; } = null!;
        public DateTime ScheduledStartTime { get; set; }
        public DateTime ScheduledEndTime { get; set; }
        public DateTime? ActualStartTime { get; set; }
        public DateTime? ActualEndTime { get; set; }
        public TripStatus Status { get; set; } = TripStatus.Scheduled;
        public decimal? StartMileage { get; set; }
        public decimal? EndMileage { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public List<Attendance> Attendances { get; set; } = new();

        public TimeSpan GetScheduledDuration()
        {
            return ScheduledEndTime - ScheduledStartTime;
        }

        public TimeSpan? GetActualDuration()
        {
            if (ActualStartTime.HasValue && ActualEndTime.HasValue)
                return ActualEndTime.Value - ActualStartTime.Value;
            return null;
        }

        public decimal? GetTripDistance()
        {
            if (StartMileage.HasValue && EndMileage.HasValue)
                return EndMileage.Value - StartMileage.Value;
            return null;
        }

        public bool IsOnTime()
        {
            if (!ActualEndTime.HasValue) return false;
            return ActualEndTime.Value <= ScheduledEndTime.AddMinutes(5); // 5 minute tolerance
        }

        public bool IsInProgress()
        {
            return Status == TripStatus.InProgress;
        }

        public bool IsCompleted()
        {
            return Status == TripStatus.Completed;
        }
    }
}

