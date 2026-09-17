using Microsoft.AspNetCore.Identity;

namespace EcommerceManagement.Core.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FullName { get; set; } = string.Empty;
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public ICollection<Order> CreatedOrders { get; set; } = new List<Order>();
    }
}
