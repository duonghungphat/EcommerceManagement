namespace EcommerceManagement.Core.Models
{
    public class ProductVariantAttributeSelection : BaseEntity
    {
        public int ProductVariantId { get; set; }

        public ProductVariant ProductVariant { get; set; } = null!;

        public int AttributeDefinitionId { get; set; }

        public ProductAttributeDefinition AttributeDefinition { get; set; } = null!;

        public int AttributeValueId { get; set; }

        public ProductAttributeValue AttributeValue { get; set; } = null!;
    }
}
