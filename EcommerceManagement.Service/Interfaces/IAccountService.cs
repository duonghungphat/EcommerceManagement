using EcommerceManagement.Core.ViewModels;

namespace EcommerceManagement.Service.Interfaces
{
    public interface IAccountService
    {
        Task<List<UserListItemViewModel>> GetAllAsync();

        Task<UserListItemViewModel?> GetByIdAsync(int userId);

        Task<List<RoleOptionViewModel>> GetRolesAsync();

        Task CreateAsync(UserCreateViewModel model);

        Task UpdateRoleAsync(int userId, int roleId, int actorUserId, string ipAddress);

        Task SetActiveAsync(int userId, bool isActive);

        Task ResetPasswordAsync(int userId, string newPassword);

        Task ChangePasswordAsync(int userId, string oldPassword, string newPassword);
    }
}