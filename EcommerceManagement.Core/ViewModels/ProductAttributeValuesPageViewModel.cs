namespace EcommerceManagement.Core.ViewModels
{
    public class ProductAttributeValuesPageViewModel
    {
        public int AttributeDefinitionId { get; set; }

        public string AttributeName { get; set; } = string.Empty;

        public ProductAttributeValueViewModel NewValue { get; set; } = new();

        public List<ProductAttributeValueViewModel> Values { get; set; } = new();
    }
}
