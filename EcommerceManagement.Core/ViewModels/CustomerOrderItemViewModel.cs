using EcommerceManagement.Core.Enums;

namespace EcommerceManagement.Core.ViewModels
{
    public class CustomerOrderItemViewModel
    {
        public int Id { get; set; }

        public string OrderCode { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public OrderStatus Status { get; set; }

        public decimal TotalAmount { get; set; }
        public string StatusText => Status switch
        {
            OrderStatus.Pending => "Chờ xử lý",
            OrderStatus.Confirmed => "Đã xác nhận",
            OrderStatus.Shipping => "Đang giao",
            OrderStatus.Completed => "Hoàn thành",
            OrderStatus.Cancelled => "Đã hủy",
            _ => "Không xác định"
        };
    }
}