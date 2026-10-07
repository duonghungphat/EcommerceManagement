using EcommerceManagement.Core.ViewModels;

namespace EcommerceManagement.Service.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductViewModel>> GetAllAsync();

        Task<ProductListViewModel> GetPagedAsync(string? searchTerm, int? categoryId, int page, int pageSize);

        Task<ProductViewModel?> GetByIdAsync(int id);

        Task CreateAsync(ProductViewModel model, int actorUserId, string ipAddress);

        Task UpdateAsync(ProductViewModel model, int actorUserId, string ipAddress);
        Task ToggleStatusAsync(int id, int actorUserId, string ipAddress);

        Task DeleteAsync(int id, int actorUserId, string ipAddress);
    }
}