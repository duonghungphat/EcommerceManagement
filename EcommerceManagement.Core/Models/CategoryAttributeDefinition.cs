namespace EcommerceManagement.Core.Models
{
    public class CategoryAttributeDefinition : BaseEntity
    {
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public int AttributeDefinitionId { get; set; }
        public ProductAttributeDefinition AttributeDefinition { get; set; } = null!;

        public bool UseForProduct { get; set; }
        public bool UseForVariant { get; set; }

        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }
    }
}
