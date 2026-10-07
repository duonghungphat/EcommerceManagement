using EcommerceManagement.Core.ViewModels;

namespace EcommerceManagement.Service.Interfaces
{
    public interface ICustomerService
    {
        Task<List<CustomerViewModel>> GetAllAsync();

        Task<CustomerListViewModel> SearchAsync(string? searchTerm);

        Task<CustomerViewModel?> GetByIdAsync(int id);

        Task<CustomerOrderHistoryViewModel?> GetOrderHistoryAsync(int id);

        Task<int> CreateAsync(CustomerViewModel model, int actorUserId, string ipAddress);

        Task UpdateAsync(CustomerViewModel model, int actorUserId, string ipAddress);

        Task DeleteAsync(int id, int actorUserId, string ipAddress);
    }
}