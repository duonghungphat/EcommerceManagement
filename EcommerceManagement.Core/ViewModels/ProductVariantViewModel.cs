using System.ComponentModel.DataAnnotations;

namespace EcommerceManagement.Core.ViewModels
{
    public class ProductVariantViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "SKU không được để trống.")]
        [StringLength(100, ErrorMessage = "SKU tối đa 100 ký tự.")]
        [Display(Name = "Mã SKU")]
        public string SKU { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "9999999999999999", ParseLimitsInInvariantCulture = true, ErrorMessage = "Giá biến thể phải lớn hơn 0.")]
        [Display(Name = "Giá")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho không được âm.")]
        [Display(Name = "Tồn kho")]
        public int StockQuantity { get; set; }

        public int OriginalStockQuantity { get; set; }
        public int ProductId { get; set; }
        public string? ImagePath { get; set; }

        public List<int> AttributeValueIds { get; set; } = new();
    }
}
