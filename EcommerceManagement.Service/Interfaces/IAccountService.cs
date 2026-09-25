using EcommerceManagement.Service.DTOs;

namespace EcommerceManagement.Service.Interfaces
{
    public interface IAccountService
    {
        Task CreateAsync(CreateUserRequest request);

        Task SetActiveAsync(int userId, bool isActive);

        Task ResetPasswordAsync(int userId, string newPassword);
    }
}