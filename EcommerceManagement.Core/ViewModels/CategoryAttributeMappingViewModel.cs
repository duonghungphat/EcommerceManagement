namespace EcommerceManagement.Core.ViewModels
{
    public class CategoryAttributeMappingViewModel
    {
        public int DefinitionId { get; set; }
        public string DefinitionName { get; set; } = string.Empty;
        public string DefinitionCode { get; set; } = string.Empty;

        public bool CanUseForProduct { get; set; }
        public bool CanUseForVariant { get; set; }

        public bool UseForProduct { get; set; }
        public bool UseForVariant { get; set; }
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }
    }
}
