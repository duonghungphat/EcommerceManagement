using EcommerceManagement.Core.Enums;

namespace EcommerceManagement.Core.ViewModels
{
    public class PaymentListViewModel
    {
        public PaymentStatus? Status { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public List<PaymentViewModel> Payments { get; set; } = new();
    }
}