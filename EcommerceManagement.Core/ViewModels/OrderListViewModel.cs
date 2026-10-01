using EcommerceManagement.Core.Enums;

namespace EcommerceManagement.Core.ViewModels
{
    public class OrderListViewModel
    {
        public string? SearchTerm { get; set; }

        public OrderStatus? Status { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public List<OrderListItemViewModel> Orders { get; set; } = new();
    }

    public class OrderListItemViewModel
    {
        public int Id { get; set; }

        public string OrderCode { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string CreatedByName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; }

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