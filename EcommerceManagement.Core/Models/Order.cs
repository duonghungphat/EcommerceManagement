using EcommerceManagement.Core.Enums;

namespace EcommerceManagement.Core.Models
{
    public class Order : BaseEntity
    {
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Note { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public int CreatedByUserId { get; set; }
        public ApplicationUser CreatedByUser { get; set; } = null!;
        public Customer Customer { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
