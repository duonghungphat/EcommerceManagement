using EcommerceManagement.Service.DTOs;

namespace EcommerceManagement.Service.Interfaces
{
    public interface IAuthService
    {
        Task<AuthenticatedUser?> LoginAsync(
            string? email,
            string? password,
            string ipAddress);
    }
}