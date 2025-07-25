using System.ComponentModel.DataAnnotations;

namespace SchoolTransportationSystem.Core.Entities
{
    public class Permission
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Resource { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        [MaxLength(50)]
        public string TenantId { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
