using EcommerceManagement.Core.ViewModels;

namespace EcommerceManagement.Service.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductViewModel>> GetAllAsync();

        Task<ProductViewModel?> GetByIdAsync(int id);

        Task CreateAsync(ProductViewModel model);

        Task UpdateAsync(ProductViewModel model);

        Task DeleteAsync(int id);
    }
}