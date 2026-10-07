using EcommerceManagement.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace EcommerceManagement.Core.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống.")]
        [StringLength(200, ErrorMessage = "Tên sản phẩm tối đa 200 ký tự.")]
        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; } = string.Empty;

        // Các field này được giữ tạm để tương thích với Order/Dashboard hiện tại.
        // Sau khi chuyển toàn bộ nghiệp vụ sang ProductVariant sẽ xóa.
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int OriginalStockQuantity { get; set; }

        [Display(Name = "Trạng thái")]
        public ProductStatus Status { get; set; } = ProductStatus.Selling;

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục.")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        [Display(Name = "Danh mục")]
        public string? CategoryName { get; set; }

        public string? ImagePath { get; set; }

        public int VariantCount { get; set; }

        public List<CategoryViewModel> Categories { get; set; } = new();

        public List<ProductVariantViewModel> Variants { get; set; } = new()
        {
            new ProductVariantViewModel()
        };
    }
}