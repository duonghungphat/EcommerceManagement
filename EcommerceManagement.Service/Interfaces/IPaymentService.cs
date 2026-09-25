using EcommerceManagement.Core.Enums;
using EcommerceManagement.Core.Models;

namespace EcommerceManagement.Service.Interfaces
{
    public interface IPaymentService
    {
        Task<List<Payment>> GetByOrderIdAsync(int orderId);

        Task<int> CreateAsync(int orderId, decimal amount, PaymentMethod method, string? transactionCode);

        Task RefundAsync(int paymentId);
    }
}