using EcommerceManagement.Core.ViewModels;

namespace EcommerceManagement.Service.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryViewModel>> GetAllAsync();

        Task<CategoryViewModel?> GetByIdAsync(int id);

        Task CreateAsync(CategoryViewModel model, int actorUserId, string ipAddress);

        Task UpdateAsync(CategoryViewModel model, int actorUserId, string ipAddress);

        Task DeleteAsync(int id, int actorUserId, string ipAddress);
    }
}