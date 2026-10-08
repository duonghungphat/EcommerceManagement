using EcommerceManagement.Core.ViewModels;

namespace EcommerceManagement.Service.Interfaces
{
    public interface IProductAttributeService
    {
        Task<List<ProductAttributeDefinitionViewModel>> GetDefinitionsAsync();
        Task<ProductAttributeDefinitionViewModel?> GetDefinitionByIdAsync(int id);
        Task CreateDefinitionAsync(ProductAttributeDefinitionViewModel model, int actorUserId, string ipAddress);
        Task UpdateDefinitionAsync(ProductAttributeDefinitionViewModel model, int actorUserId, string ipAddress);
        Task DeleteDefinitionAsync(int id, int actorUserId, string ipAddress);

        Task<ProductAttributeValuesPageViewModel?> GetValuesPageAsync(int definitionId);
        Task CreateValueAsync(ProductAttributeValueViewModel model, int actorUserId, string ipAddress);
        Task UpdateValueAsync(ProductAttributeValueViewModel model, int actorUserId, string ipAddress);
        Task DeleteValueAsync(int id, int actorUserId, string ipAddress);

        Task<List<ProductAttributeInputViewModel>> GetInputsAsync(bool forVariant);
        Task<List<ProductAttributeInputViewModel>> GetInputsForCategoryAsync(int categoryId, bool forVariant);
        Task<CategoryAttributeConfigViewModel?> GetCategoryConfigAsync(int categoryId);
        Task SaveCategoryConfigAsync(CategoryAttributeConfigViewModel model, int actorUserId, string ipAddress);
    }
}
