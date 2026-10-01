using EcommerceManagement.Core.Enums;
using EcommerceManagement.Core.ViewModels;

namespace EcommerceManagement.Service.Interfaces
{
    public interface IOrderService
    {
        Task<OrderListViewModel> SearchAsync(string? searchTerm, OrderStatus? status, DateTime? fromDate, DateTime? toDate);

        Task<OrderDetailsViewModel?> GetDetailsAsync(int id);

        Task<int> CreateAsync(OrderCreateViewModel model, int createdByUserId, string ipAddress);

        Task CancelAsync(int id, int actorUserId, string ipAddress);

        Task UpdateStatusAsync(int id, OrderStatus status, int actorUserId, string ipAddress);
    }
}