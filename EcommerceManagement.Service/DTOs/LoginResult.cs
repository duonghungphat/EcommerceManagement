using EcommerceManagement.Core.Enums;

namespace EcommerceManagement.Service.DTOs
{
    public class LoginResult
    {
        public LoginStatus Status { get; set; }

        public AuthenticatedUser? User { get; set; }

        public DateTime? LockoutEnd { get; set; }
    }
}