using EcommerceManagement.Service.DTOs;

namespace EcommerceManagement.Service.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResult> LoginAsync(string email, string password, string ipAddress);
    }
}