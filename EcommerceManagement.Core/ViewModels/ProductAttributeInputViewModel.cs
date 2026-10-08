namespace EcommerceManagement.Core.ViewModels
{
    public class ProductAttributeInputViewModel
    {
        public int DefinitionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool AllowMultipleValues { get; set; }
        public bool IsRequired { get; set; }
        public List<int> SelectedValueIds { get; set; } = new();
        public List<ProductAttributeValueOptionViewModel> Values { get; set; } = new();
    }
}
