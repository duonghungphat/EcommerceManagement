using EcommerceManagement.Core.Models;
using EcommerceManagement.Data.UnitOfWork;
using EcommerceManagement.Service.DTOs;
using EcommerceManagement.Service.Interfaces;
using EcommerceManagement.Service.Security;
using Microsoft.EntityFrameworkCore;

namespace EcommerceManagement.Service.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AccountService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateAsync(CreateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new InvalidOperationException("Tên nhân viên không được để trống.");

            string email = request.Email?.Trim().ToLowerInvariant()
                ?? string.Empty;

            if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
                throw new InvalidOperationException("Email không hợp lệ.");

            ValidatePassword(request.Password);

            bool emailExists = await _unitOfWork.Users
                .BuildQuery(u => u.Email == email)
                .AnyAsync();

            if (emailExists)
                throw new InvalidOperationException("Email đã tồn tại.");

            bool roleExists = await _unitOfWork.Roles
                .BuildQuery(r => r.Id == request.RoleId)
                .AnyAsync();

            if (!roleExists)
                throw new InvalidOperationException("Role không tồn tại.");

            var user = new ApplicationUser
            {
                FullName = request.FullName.Trim(),
                Email = email,
                PasswordHash = PasswordHelper.HashPassword(request.Password),
                RoleId = request.RoleId,
                IsActive = true,
                FailedLoginAttempts = 0
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task SetActiveAsync(int userId, bool isActive)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null)
                throw new InvalidOperationException("Tài khoản không tồn tại.");

            user.IsActive = isActive;

            if (isActive)
            {
                user.FailedLoginAttempts = 0;
                user.LockoutEnd = null;
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ResetPasswordAsync(int userId, string newPassword)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null)
                throw new InvalidOperationException("Tài khoản không tồn tại.");

            ValidatePassword(newPassword);

            user.PasswordHash = PasswordHelper.HashPassword(newPassword);
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;

            await _unitOfWork.SaveChangesAsync();
        }

        private static void ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) ||
                password.Length < 8 ||
                !password.Any(char.IsUpper) ||
                !password.Any(char.IsLower) ||
                !password.Any(char.IsDigit))
            {
                throw new InvalidOperationException("Mật khẩu phải có ít nhất 8 ký tự, gồm chữ hoa, chữ thường và số.");
            }
        }
    }
}