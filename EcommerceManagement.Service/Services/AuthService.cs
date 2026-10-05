using EcommerceManagement.Core.Enums;
using EcommerceManagement.Core.Models;
using EcommerceManagement.Data.UnitOfWork;
using EcommerceManagement.Service.DTOs;
using EcommerceManagement.Service.Interfaces;
using EcommerceManagement.Service.Security;
using Microsoft.EntityFrameworkCore;

namespace EcommerceManagement.Service.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;

        public AuthService(IUnitOfWork unitOfWork, IAuditLogService auditLogService)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
        }

        public async Task<LoginResult> LoginAsync(string email, string password, string ipAddress)
        {
            string normalizedEmail = email.Trim().ToLowerInvariant();

            // 1. Tìm tài khoản theo email
            var foundUser = await _unitOfWork.Users
                .BuildQuery(u => u.Email == normalizedEmail)
                .FirstOrDefaultAsync();

            if (foundUser == null)
            {
                await RejectLoginAsync(null, ipAddress);

                return new LoginResult
                {
                    Status = LoginStatus.InvalidCredentials
                };
            }

            // Lấy entity có tracking để cập nhật số lần đăng nhập sai
            var user = await _unitOfWork.Users
                .GetByIdAsync(foundUser.Id);

            if (user == null)
            {
                await RejectLoginAsync(null, ipAddress);

                return new LoginResult
                {
                    Status = LoginStatus.InvalidCredentials
                };
            }

            if (!user.IsActive)
            {
                await RejectLoginAsync(user, ipAddress);

                return new LoginResult
                {
                    Status = LoginStatus.Inactive
                };
            }

            DateTime now = DateTime.Now;

            // 2. Tài khoản còn trong thời gian khóa tạm
            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > now)
            {
                await RejectLoginAsync(user, ipAddress);

                return new LoginResult
                {
                    Status = LoginStatus.TemporarilyLocked,
                    LockoutEnd = user.LockoutEnd
                };
            }

            // 3. Nếu đã hết thời gian khóa, cho phép thử lại
            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value <= now)
            {
                user.LockoutEnd = null;
                user.FailedLoginAttempts = 0;
            }

            // 4. Kiểm tra mật khẩu
            bool passwordIsCorrect = PasswordHelper.VerifyPassword(password, user.PasswordHash);

            if (!passwordIsCorrect)
            {
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockoutEnd = now.AddMinutes(5);

                    await RejectLoginAsync(user, ipAddress);

                    return new LoginResult
                    {
                        Status = LoginStatus.TemporarilyLocked,
                        LockoutEnd = user.LockoutEnd
                    };
                }

                await RejectLoginAsync(user, ipAddress);

                return new LoginResult
                {
                    Status = LoginStatus.InvalidCredentials
                };
            }

            // 5. Đăng nhập thành công
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;

            var role = await _unitOfWork.Roles
                .GetByIdAsync(user.RoleId);

            if (role == null)
                throw new InvalidOperationException("Tài khoản chưa có Role hợp lệ.");

            await _auditLogService.RecordAsync("LoginSuccess", "ApplicationUser", user.Id, "Đăng nhập thành công.", ipAddress, user.Id);

            await _unitOfWork.SaveChangesAsync();

            return new LoginResult
            {
                Status = LoginStatus.Success,

                User = new AuthenticatedUser
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    RoleName = role.Name
                }
            };
        }

        private async Task RejectLoginAsync(ApplicationUser? user, string ipAddress)
        {
            await _auditLogService.RecordAsync("LoginFailed", "ApplicationUser", user?.Id, "Đăng nhập thất bại.", ipAddress, user?.Id);

            await _unitOfWork.SaveChangesAsync();

        }
    }
}