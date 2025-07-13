using System.ComponentModel.DataAnnotations;

namespace SchoolTransportationSystem.Core.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        
        public int? UserId { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(45)]
        public string IpAddress { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string UserAgent { get; set; } = string.Empty;
        
        public bool Success { get; set; }
        
        public string? Details { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        [Required]
        [MaxLength(50)]
        public string TenantId { get; set; } = string.Empty;
    }
}
