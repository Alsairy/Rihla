using System.ComponentModel.DataAnnotations;

namespace SchoolTransportationSystem.Core.Entities
{
    public class RolePermission
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = string.Empty;
        
        public int PermissionId { get; set; }
        
        public Permission Permission { get; set; } = new();
        
        public bool IsGranted { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        [MaxLength(50)]
        public string TenantId { get; set; } = string.Empty;
    }
}
