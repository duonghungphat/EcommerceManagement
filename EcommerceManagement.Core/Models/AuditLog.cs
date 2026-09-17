namespace EcommerceManagement.Core.Models
{
    public class AuditLog : BaseEntity
    {
        public string Action { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int? EntityId { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public int? ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }

    }
}
