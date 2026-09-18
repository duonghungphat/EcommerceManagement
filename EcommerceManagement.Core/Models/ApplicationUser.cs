using EcommerceManagement.Core.Enums;

namespace EcommerceManagement.Core.Models
{
    public class ApplicationUser : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime? LockoutEnd { get; set; }
        public bool IsActive { get; set; } = true;
        public int FailedLoginAttempts { get; set; }
        public int RoleId { get; set; }
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public ICollection<Order> CreatedOrders { get; set; } = new List<Order>();
        public Role Role { get; set; } = null!;
    }
}
