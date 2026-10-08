namespace EcommerceManagement.Core.Models
{
    public class ProductAttributeSelection : BaseEntity
    {
        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;

        public int AttributeDefinitionId { get; set; }

        public ProductAttributeDefinition AttributeDefinition { get; set; } = null!;

        public int AttributeValueId { get; set; }

        public ProductAttributeValue AttributeValue { get; set; } = null!;
    }
}
