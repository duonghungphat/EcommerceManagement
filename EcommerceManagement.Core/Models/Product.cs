using EcommerceManagement.Core.Enums;

namespace EcommerceManagement.Core.Models
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public ProductStatus Status { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public ICollection<ProductAttributeSelection> AttributeSelections { get; set; } = new List<ProductAttributeSelection>();
    }
}
