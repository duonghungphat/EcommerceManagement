namespace EcommerceManagement.Core.ViewModels
{
    public class ProductListViewModel
    {
        public List<ProductViewModel> Products { get; set; } = new();

        public string? SearchTerm { get; set; }

        public int? CategoryId { get; set; }

        public List<CategoryViewModel> Categories { get; set; } = new();

        public int Page { get; set; } = 1;

        public int TotalPages { get; set; }
    }
}