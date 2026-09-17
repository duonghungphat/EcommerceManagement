using EcommerceManagement.Core.Enums;
namespace EcommerceManagement.Core.Models
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? ImagePath { get; set; }
        public ProductStatus Status { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
