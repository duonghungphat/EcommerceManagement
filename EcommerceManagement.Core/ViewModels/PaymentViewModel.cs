using EcommerceManagement.Core.Enums;

namespace EcommerceManagement.Core.ViewModels
{
    public class PaymentViewModel
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public string OrderCode { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public PaymentStatus Status { get; set; }

        public string? TransactionCode { get; set; }

        public DateTime TransactionAt { get; set; }

        public string MethodText => PaymentMethod switch
        {
            PaymentMethod.Cash => "Tiền mặt",
            PaymentMethod.COD => "COD",
            PaymentMethod.BankTransfer => "Chuyển khoản",
            _ => "Không xác định"
        };

        public string StatusText => Status switch
        {
            PaymentStatus.Pending => "Đang chờ",
            PaymentStatus.Successful => "Thành công",
            PaymentStatus.Failed => "Thất bại",
            PaymentStatus.Refunded => "Đã hoàn tiền",
            _ => "Không xác định"
        };
    }
}