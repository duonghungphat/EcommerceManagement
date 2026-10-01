using EcommerceManagement.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace EcommerceManagement.Core.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống.")]
        [StringLength(200)]
        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "SKU không được để trống.")]
        [StringLength(100)]
        [Display(Name = "Mã SKU")]
        public string SKU { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "9999999999999999", ErrorMessage = "Giá sản phẩm phải lớn hơn 0.")]
        [Display(Name = "Giá bán")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho không được âm.")]
        [Display(Name = "Số lượng tồn kho")]
        public int StockQuantity { get; set; }

        public int OriginalStockQuantity { get; set; }

        [Display(Name = "Trạng thái")]
        public ProductStatus Status { get; set; } = ProductStatus.Selling;

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục.")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }
        [Display(Name = "Danh mục")]
        public string? CategoryName { get; set; }
        [Display(Name = "Ảnh sản phẩm")]
        public string? ImagePath { get; set; }

        public List<CategoryViewModel> Categories { get; set; } = new();
    }
}