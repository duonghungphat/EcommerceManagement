using EcommerceManagement.Core.ViewModels;

namespace EcommerceManagement.Service.Interfaces
{
    public interface ICustomerService
    {
        Task<List<CustomerViewModel>> GetAllAsync();

        Task<CustomerViewModel?> GetByIdAsync(int id);

        Task CreateAsync(CustomerViewModel model);

        Task UpdateAsync(CustomerViewModel model);

        Task DeleteAsync(int id);
    }
}