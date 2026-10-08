using System.ComponentModel.DataAnnotations;

namespace EcommerceManagement.Core.Models
{
    public class ProductVariant : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string SKU { get; set; } = string.Empty;

        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        [MaxLength(1000)]
        public string? ImagePath { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public ICollection<ProductVariantAttributeSelection> AttributeSelections { get; set; } = new List<ProductVariantAttributeSelection>();
    }
}
