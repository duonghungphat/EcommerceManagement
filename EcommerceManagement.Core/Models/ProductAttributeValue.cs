namespace EcommerceManagement.Core.Models
{
    public class ProductAttributeValue : BaseEntity
    {
        public string Value { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public int AttributeDefinitionId { get; set; }

        public ProductAttributeDefinition AttributeDefinition { get; set; } = null!;

        public ICollection<ProductAttributeSelection> ProductSelections { get; set; } = new List<ProductAttributeSelection>();

        public ICollection<ProductVariantAttributeSelection> VariantSelections { get; set; } = new List<ProductVariantAttributeSelection>();
    }
}
