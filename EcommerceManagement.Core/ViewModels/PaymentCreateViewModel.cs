using EcommerceManagement.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace EcommerceManagement.Core.ViewModels
{
    public class PaymentCreateViewModel
    {
        public int OrderId { get; set; }

        public string OrderCode { get; set; } = string.Empty;

        public decimal RemainingAmount { get; set; }

        [Range(typeof(decimal), "0.01", "999999999999", ErrorMessage = "Số tiền phải lớn hơn 0.")]
        public decimal Amount { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Successful;

        [StringLength(100)]
        public string? TransactionCode { get; set; }
    }
}