using EcommerceManagement.Core.ViewModels;

namespace EcommerceManagement.Service.Interfaces
{
    public interface IAccountService
    {
        Task<List<UserListItemViewModel>> GetAllAsync();

        Task<UserListItemViewModel?> GetByIdAsync(int userId);

        Task<List<RoleOptionViewModel>> GetRolesAsync();

        Task CreateAsync(UserCreateViewModel model, int actorUserId, string ipAddress);

        Task UpdateRoleAsync(int userId, int roleId, int actorUserId, string ipAddress);

        Task SetActiveAsync(int id, bool isActive, int actorUserId, string ipAddress);

        Task ResetPasswordAsync(int id, ResetPasswordViewModel model, int actorUserId, string ipAddress);

        Task ChangePasswordAsync(int userId, string oldPassword, string newPassword);
    }
}