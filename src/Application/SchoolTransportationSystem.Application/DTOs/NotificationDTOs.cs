using System.ComponentModel.DataAnnotations;
using SchoolTransportationSystem.Core.Enums;

namespace SchoolTransportationSystem.Application.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationPriority Priority { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public int UserId { get; set; }
        public int? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
        public NotificationChannel Channel { get; set; }
        public bool EmailSent { get; set; }
        public bool SmsSent { get; set; }
        public DateTime? EmailSentAt { get; set; }
        public DateTime? SmsSentAt { get; set; }
        public string? EmailError { get; set; }
        public string? SmsError { get; set; }
        public string? Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }
        public int TenantId { get; set; }
    }

    public class CreateNotificationDto
    {
        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        [Required]
        public int UserId { get; set; }

        public int? RelatedEntityId { get; set; }

        [StringLength(50)]
        public string? RelatedEntityType { get; set; }

        public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;

        [StringLength(1000)]
        public string? Metadata { get; set; }

        [Required]
        [StringLength(50)]
        public string TenantId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class UpdateNotificationDto
    {
        [Required]
        public int Id { get; set; }

        [StringLength(50)]
        public string? Type { get; set; }

        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(1000)]
        public string? Message { get; set; }

        public NotificationPriority? Priority { get; set; }

        public bool? IsRead { get; set; }

        public int? UserId { get; set; }

        public int? RelatedEntityId { get; set; }

        [StringLength(50)]
        public string? RelatedEntityType { get; set; }

        public NotificationChannel? Channel { get; set; }

        [StringLength(1000)]
        public string? Metadata { get; set; }

        [Required]
        [StringLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;
    }

    public class NotificationSearchDto
    {
        public int? UserId { get; set; }
        public string? Type { get; set; }
        public NotificationPriority? Priority { get; set; }
        public bool? IsRead { get; set; }
        public NotificationChannel? Channel { get; set; }
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? TenantId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
    }

    public class NotificationStatisticsDto
    {
        public int TotalNotifications { get; set; }
        public int UnreadNotifications { get; set; }
        public int UrgentNotifications { get; set; }
        public int CriticalNotifications { get; set; }
        public int TodayNotifications { get; set; }
        public int EmailsSent { get; set; }
        public int SmssSent { get; set; }
        public int FailedEmails { get; set; }
        public int FailedSms { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class MarkNotificationReadDto
    {
        [Required]
        public int NotificationId { get; set; }

        [Required]
        [StringLength(100)]
        public string ReadBy { get; set; } = string.Empty;
    }

    public class BulkNotificationDto
    {
        [Required]
        public List<int> UserIds { get; set; } = new();

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;

        public int? RelatedEntityId { get; set; }

        [StringLength(50)]
        public string? RelatedEntityType { get; set; }

        [StringLength(1000)]
        public string? Metadata { get; set; }

        [Required]
        [StringLength(50)]
        public string TenantId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class NotificationDeliveryStatusDto
    {
        public int NotificationId { get; set; }
        public bool EmailSent { get; set; }
        public bool SmsSent { get; set; }
        public DateTime? EmailSentAt { get; set; }
        public DateTime? SmsSentAt { get; set; }
        public string? EmailError { get; set; }
        public string? SmsError { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
