using System.ComponentModel.DataAnnotations;

namespace SchoolTransportationSystem.Application.DTOs
{
    public class GPSTrackingDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public int? TripId { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal? Altitude { get; set; }
        public decimal Speed { get; set; }
        public decimal Heading { get; set; }
        public decimal Accuracy { get; set; }
        public string? Address { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal BatteryLevel { get; set; }
        public decimal SignalStrength { get; set; }
        public string? DeviceId { get; set; }
        public string? Notes { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class CreateGPSTrackingDto
    {
        [Required]
        public int VehicleId { get; set; }
        public int? TripId { get; set; }
        [Required]
        public decimal Latitude { get; set; }
        [Required]
        public decimal Longitude { get; set; }
        public decimal? Altitude { get; set; }
        [Required]
        public decimal Speed { get; set; }
        [Required]
        public decimal Heading { get; set; }
        [Required]
        public decimal Accuracy { get; set; }
        public string? Address { get; set; }
        [Required]
        public string Status { get; set; } = string.Empty;
        [Required]
        public decimal BatteryLevel { get; set; }
        [Required]
        public decimal SignalStrength { get; set; }
        public string? DeviceId { get; set; }
        public string? Notes { get; set; }
        [Required]
        public DateTime Timestamp { get; set; }
    }

    public class GeofenceViolationDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string VehicleNumber { get; set; } = string.Empty;
        public int? TripId { get; set; }
        public string? RouteNumber { get; set; }
        public string ViolationType { get; set; } = string.Empty;
        public string GeofenceName { get; set; } = string.Empty;
        public decimal ViolationLatitude { get; set; }
        public decimal ViolationLongitude { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal Distance { get; set; }
        public string Severity { get; set; } = string.Empty;
        public DateTime ViolationTime { get; set; }
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ActionTaken { get; set; }
        public string? ActionRequired { get; set; }
        public string? Notes { get; set; }
        public string Description { get; set; } = string.Empty;
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int StopId { get; set; }
        public string StopName { get; set; } = string.Empty;
    }

    public class GeofenceAlertDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string VehicleNumber { get; set; } = string.Empty;
        public int? TripId { get; set; }
        public string? RouteNumber { get; set; }
        public string ViolationType { get; set; } = string.Empty;
        public string GeofenceName { get; set; } = string.Empty;
        public decimal ViolationLatitude { get; set; }
        public decimal ViolationLongitude { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal Distance { get; set; }
        public string Severity { get; set; } = string.Empty;
        public DateTime ViolationTime { get; set; }
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ActionTaken { get; set; }
        public string? ActionRequired { get; set; }
        public string? Notes { get; set; }
        public string Description { get; set; } = string.Empty;
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int StopId { get; set; }
        public string StopName { get; set; } = string.Empty;
    }

    public class EstimatedArrivalDto
    {
        public int TripId { get; set; }
        public int RouteStopId { get; set; }
        public string RouteStopName { get; set; } = string.Empty;
        public DateTime EstimatedArrivalTime { get; set; }
        public DateTime? ActualArrivalTime { get; set; }
        public TimeSpan? Delay { get; set; }
        public decimal Confidence { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal CurrentLatitude { get; set; }
        public decimal CurrentLongitude { get; set; }
        public decimal DistanceToStop { get; set; }
        public decimal AverageSpeed { get; set; }
        public string? TrafficConditions { get; set; }
        public string? WeatherConditions { get; set; }
        public int StopId { get; set; }
        public string StopName { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public decimal AverageSpeedKmh { get; set; }
        public decimal ConfidenceLevel { get; set; }
        public DateTime LastUpdated { get; set; }
        public int DelayMinutes { get; set; }
    }

    public class RestrictedAreaDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal CenterLatitude { get; set; }
        public decimal CenterLongitude { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal Radius { get; set; }
        public decimal RadiusKm { get; set; }
        public string RestrictionType { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<string> AllowedVehicles { get; set; } = new();
        public string? Notes { get; set; }
        public List<string> RestrictedHours { get; set; } = new();
    }

    public class VehicleLocationDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public int? TripId { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal? Altitude { get; set; }
        public decimal Speed { get; set; }
        public decimal Heading { get; set; }
        public decimal Accuracy { get; set; }
        public string? Address { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal BatteryLevel { get; set; }
        public decimal SignalStrength { get; set; }
        public string? DeviceId { get; set; }
        public string? Notes { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsActive { get; set; }
        public string VehicleName { get; set; } = string.Empty;
    }
}
