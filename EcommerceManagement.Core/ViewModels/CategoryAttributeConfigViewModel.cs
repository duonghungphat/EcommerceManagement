namespace EcommerceManagement.Core.ViewModels
{
    public class CategoryAttributeConfigViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public List<CategoryAttributeMappingViewModel> Attributes { get; set; } = new();
    }
}
