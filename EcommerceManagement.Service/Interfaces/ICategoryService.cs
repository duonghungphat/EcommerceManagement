using EcommerceManagement.Core.ViewModels;

namespace EcommerceManagement.Service.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryViewModel>> GetAllAsync();

        Task<CategoryViewModel?> GetByIdAsync(int id);

        Task CreateAsync(CategoryViewModel model);

        Task UpdateAsync(CategoryViewModel model);

        Task DeleteAsync(int id);
    }
}