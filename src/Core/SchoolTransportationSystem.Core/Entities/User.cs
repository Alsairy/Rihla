using System.ComponentModel.DataAnnotations;

namespace SchoolTransportationSystem.Core.Entities
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Required]
        public string Salt { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string TenantId { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastLoginAt { get; set; }
        
        public string? RefreshToken { get; set; }
        
        public DateTime? RefreshTokenExpiryTime { get; set; }
        
        public int FailedLoginAttempts { get; set; } = 0;
        
        public DateTime? AccountLockedUntil { get; set; }
        
        public bool MfaEnabled { get; set; } = false;
        
        public string? MfaSecret { get; set; }
        
        public string? BackupCodes { get; set; }
        
        public DateTime? LastPasswordChangeAt { get; set; }
        
        public string? PasswordHistory { get; set; }
    }
}
