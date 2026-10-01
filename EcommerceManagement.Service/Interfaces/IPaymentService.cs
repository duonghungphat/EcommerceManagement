using EcommerceManagement.Core.Enums;
using EcommerceManagement.Core.ViewModels;

namespace EcommerceManagement.Service.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentListViewModel> SearchAsync(PaymentStatus? status, DateTime? fromDate, DateTime? toDate);

        Task<PaymentCreateViewModel?> GetCreateModelAsync(int orderId);

        Task<int> CreateAsync(PaymentCreateViewModel model, int actorUserId, string ipAddress);

        Task RefundAsync(int paymentId, int actorUserId, string ipAddress);
    }
}