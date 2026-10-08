using System.ComponentModel.DataAnnotations;

namespace EcommerceManagement.Core.ViewModels
{
    public class OrderItemViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn biến thể sản phẩm.")]
        public int ProductVariantId { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public string VariantName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải từ 1 trở lên.")]
        public int Quantity { get; set; } = 1;

        public decimal UnitPrice { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }
}
