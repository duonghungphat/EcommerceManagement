using EcommerceManagement.Core.Enums;
using EcommerceManagement.Core.Models;
using EcommerceManagement.Service.DTOs;

namespace EcommerceManagement.Service.Interfaces
{
    public interface IOrderService
    {
        Task<List<Order>> GetAllAsync();

        Task<Order?> GetByIdAsync(int id);

        Task<int> CreateAsync(CreateOrderRequest request);

        Task CancelAsync(int id);

        Task UpdateStatusAsync(int id, OrderStatus newStatus);
    }
}