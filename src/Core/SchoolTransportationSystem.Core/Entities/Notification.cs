using SchoolTransportationSystem.Core.Enums;

namespace SchoolTransportationSystem.Core.Entities
{
    public class Notification : TenantEntity
    {
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
        public int UserId { get; set; }
        public int? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
        public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;
        public bool EmailSent { get; set; } = false;
        public bool SmsSent { get; set; } = false;
        public DateTime? EmailSentAt { get; set; }
        public DateTime? SmsSentAt { get; set; }
        public string? EmailError { get; set; }
        public string? SmsError { get; set; }
        public string? Metadata { get; set; }

        // Navigation properties
        public User? User { get; set; }

        public void MarkAsRead(string readBy)
        {
            IsRead = true;
            ReadAt = DateTime.UtcNow;
            MarkAsUpdated(readBy);
        }

        public void MarkEmailSent()
        {
            EmailSent = true;
            EmailSentAt = DateTime.UtcNow;
        }

        public void MarkSmsSent()
        {
            SmsSent = true;
            SmsSentAt = DateTime.UtcNow;
        }

        public void SetEmailError(string error)
        {
            EmailError = error;
            EmailSent = false;
        }

        public void SetSmsError(string error)
        {
            SmsError = error;
            SmsSent = false;
        }
    }
}
