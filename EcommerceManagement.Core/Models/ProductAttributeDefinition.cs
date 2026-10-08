namespace EcommerceManagement.Core.Models
{
    public class ProductAttributeDefinition : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;

        public bool CanUseForVariant { get; set; }
        public bool CanUseForProduct { get; set; }
        public bool IsFilterable { get; set; }
        public bool AllowMultipleProductValues { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<ProductAttributeValue> Values { get; set; } = new List<ProductAttributeValue>();
        public ICollection<CategoryAttributeDefinition> CategoryMappings { get; set; } = new List<CategoryAttributeDefinition>();
    }
}
