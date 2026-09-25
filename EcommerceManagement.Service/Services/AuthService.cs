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

        public AuthService(
            IUnitOfWork unitOfWork,
            IAuditLogService auditLogService)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
        }

        public async Task<AuthenticatedUser?> LoginAsync(
            string? email,
            string? password,
            string ipAddress)
        {
            string normalizedEmail = email?.Trim().ToLowerInvariant()
                ?? string.Empty;

            // 1. Tìm tài khoản theo email
            var foundUser = await _unitOfWork.Users
                .BuildQuery(u => u.Email == normalizedEmail)
                .FirstOrDefaultAsync();

            if (foundUser == null)
                return await RejectLoginAsync(null, ipAddress);

            // Lấy entity có tracking để cập nhật số lần đăng nhập sai
            var user = await _unitOfWork.Users
                .GetByIdAsync(foundUser.Id);

            if (user == null || !user.IsActive)
                return await RejectLoginAsync(user, ipAddress);

            DateTime now = DateTime.UtcNow;

            // 2. Tài khoản còn trong thời gian khóa tạm
            if (user.LockoutEnd.HasValue &&
                user.LockoutEnd.Value > now)
            {
                return await RejectLoginAsync(user, ipAddress);
            }

            // 3. Nếu đã hết thời gian khóa, cho phép thử lại
            if (user.LockoutEnd.HasValue &&
                user.LockoutEnd.Value <= now)
            {
                user.LockoutEnd = null;
                user.FailedLoginAttempts = 0;
            }

            // 4. Kiểm tra mật khẩu
            bool passwordIsCorrect =
                !string.IsNullOrEmpty(password) &&
                PasswordHelper.VerifyPassword(password, user.PasswordHash);

            if (!passwordIsCorrect)
            {
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockoutEnd = now.AddMinutes(5);
                }

                return await RejectLoginAsync(user, ipAddress);
            }

            // 5. Đăng nhập thành công
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;

            var role = await _unitOfWork.Roles
                .GetByIdAsync(user.RoleId);

            if (role == null)
                throw new InvalidOperationException("Tài khoản chưa có Role hợp lệ.");

            await _auditLogService.RecordAsync(
                "LoginSuccess",
                "ApplicationUser",
                user.Id,
                "Đăng nhập thành công.",
                ipAddress,
                user.Id);

            await _unitOfWork.SaveChangesAsync();

            return new AuthenticatedUser
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                RoleName = role.Name
            };
        }

        private async Task<AuthenticatedUser?> RejectLoginAsync(
            ApplicationUser? user,
            string ipAddress)
        {
            await _auditLogService.RecordAsync(
                "LoginFailed",
                "ApplicationUser",
                user?.Id,
                "Đăng nhập thất bại.",
                ipAddress,
                user?.Id);

            await _unitOfWork.SaveChangesAsync();

            return null;
        }
    }
}