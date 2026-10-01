using EcommerceManagement.Core.Enums;

namespace EcommerceManagement.Core.ViewModels
{
    public class OrderDetailsViewModel
    {
        public int Id { get; set; }

        public string OrderCode { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string CustomerEmail { get; set; } = string.Empty;

        public string CustomerPhone { get; set; } = string.Empty;

        public string CreatedByName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public string? Note { get; set; }

        public OrderStatus Status { get; set; }

        public decimal TotalAmount { get; set; }

        // Tổng số tiền đã từng thu thành công,
        // bao gồm cả những khoản sau đó đã refund.
        public decimal OriginalPaidAmount { get; set; }

        // Tổng tiền đã hoàn lại.
        public decimal RefundedAmount { get; set; }

        // Số tiền thực tế cửa hàng đang giữ.
        public decimal PaidAmount { get; set; }

        // Số tiền khách còn phải thanh toán.
        public decimal RemainingAmount => TotalAmount - PaidAmount;

        public List<OrderItemViewModel> Items { get; set; } = new();

        public List<PaymentViewModel> Payments { get; set; } = new();

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