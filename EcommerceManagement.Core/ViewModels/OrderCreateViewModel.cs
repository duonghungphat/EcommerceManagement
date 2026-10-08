using System.ComponentModel.DataAnnotations;

namespace EcommerceManagement.Core.ViewModels
{
    public class OrderCreateViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn khách hàng.")]
        public int CustomerId { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }

        public List<OrderItemViewModel> Items { get; set; } = new()
        {
            new OrderItemViewModel()
        };

        public List<CustomerViewModel> Customers { get; set; } = new();
        public List<ProductVariantOptionViewModel> ProductVariants { get; set; } = new();
    }
}
