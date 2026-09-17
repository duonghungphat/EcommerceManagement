using EcommerceManagement.Core.Enums;

namespace EcommerceManagement.Core.Models
{
    public class Payment : BaseEntity
    {
        public decimal Amount { get; set; }
        public DateTime PaymentAt { get; set; } = DateTime.Now;
        public string? TransactionCode { get; set; }
        public PaymentStatus Status { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
    }
}
