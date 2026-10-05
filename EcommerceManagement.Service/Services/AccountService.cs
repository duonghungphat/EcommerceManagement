using EcommerceManagement.Core.Models;
using EcommerceManagement.Core.ViewModels;
using EcommerceManagement.Data.UnitOfWork;
using EcommerceManagement.Service.Interfaces;
using EcommerceManagement.Service.Security;
using Microsoft.EntityFrameworkCore;

namespace EcommerceManagement.Service.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;

        public AccountService(IUnitOfWork unitOfWork, IAuditLogService auditLogService)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
        }

        public async Task<List<UserListItemViewModel>> GetAllAsync()
        {
            return await _unitOfWork.Users
                .BuildQuery(u => true)
                .OrderBy(u => u.FullName)
                .Select(u => new UserListItemViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    RoleId = u.RoleId,
                    RoleName = u.Role.Name,
                    IsActive = u.IsActive,
                    LockoutEnd = u.LockoutEnd
                })
                .ToListAsync();
        }

        public async Task<UserListItemViewModel?> GetByIdAsync(int userId)
        {
            return await _unitOfWork.Users
                .BuildQuery(u => u.Id == userId)
                .Select(u => new UserListItemViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    RoleId = u.RoleId,
                    RoleName = u.Role.Name,
                    IsActive = u.IsActive,
                    LockoutEnd = u.LockoutEnd
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<RoleOptionViewModel>> GetRolesAsync()
        {
            return await _unitOfWork.Roles
                .BuildQuery(r => true)
                .OrderBy(r => r.Id)
                .Select(r => new RoleOptionViewModel
                {
                    Id = r.Id,
                    Name = r.Name
                })
                .ToListAsync();
        }

        public async Task CreateAsync(UserCreateViewModel model, int actorUserId, string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(model.FullName))
                throw new InvalidOperationException("Họ tên không được để trống.");

            string email = model.Email.Trim().ToLowerInvariant();

            bool emailExists = await _unitOfWork.Users
                .BuildQuery(u => u.Email == email)
                .AnyAsync();

            if (emailExists)
                throw new InvalidOperationException("Email đã tồn tại.");

            ValidatePassword(model.Password);

            bool roleExists = await _unitOfWork.Roles
                .BuildQuery(r => r.Id == model.RoleId)
                .AnyAsync();

            if (!roleExists)
                throw new InvalidOperationException("Vai trò không tồn tại.");

            var user = new ApplicationUser
            {
                FullName = model.FullName.Trim(),
                Email = email,
                PasswordHash = PasswordHelper.HashPassword(model.Password),
                RoleId = model.RoleId,
                IsActive = true,
                FailedLoginAttempts = 0
            };

            await _unitOfWork.Users.AddAsync(user);

            await _auditLogService.RecordAsync("CreateUser", "ApplicationUser", null, $"Tạo tài khoản {user.Email}", ipAddress, actorUserId);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateRoleAsync(int userId, int roleId, int actorUserId, string ipAddress)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null)
                throw new InvalidOperationException("Tài khoản không tồn tại.");

            if (userId == actorUserId && user.RoleId != roleId)
            {
                throw new InvalidOperationException("Bạn không thể thay đổi vai trò của chính tài khoản đang đăng nhập.");
            }

            var newRole = await _unitOfWork.Roles.GetByIdAsync(roleId);

            if (newRole == null)
                throw new InvalidOperationException("Vai trò không tồn tại.");

            if (user.RoleId == roleId)
                return;

            var oldRole = await _unitOfWork.Roles.GetByIdAsync(user.RoleId);

            string oldRoleName = GetRoleDisplayName(oldRole?.Name);
            string newRoleName = GetRoleDisplayName(newRole.Name);

            user.RoleId = roleId;

            await _auditLogService.RecordAsync("RoleChanged", "ApplicationUser", user.Id, $"Thay đổi vai trò từ {oldRoleName} sang {newRoleName}.", ipAddress, actorUserId);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> ToggleActiveAsync(int id, int actorUserId, string ipAddress)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);

            if (user == null)
                throw new InvalidOperationException("Tài khoản không tồn tại.");

            bool newIsActive = !user.IsActive;

            if (id == actorUserId && !newIsActive)
            {
                throw new InvalidOperationException("Bạn không thể khóa chính tài khoản đang đăng nhập.");
            }

            user.IsActive = newIsActive;

            if (newIsActive)
            {
                user.FailedLoginAttempts = 0;
                user.LockoutEnd = null;
            }

            await _auditLogService.RecordAsync(newIsActive ? "UnlockUser" : "LockUser", "ApplicationUser", user.Id, newIsActive ? $"Mở khóa tài khoản {user.Email}" : $"Khóa tài khoản {user.Email}", ipAddress, actorUserId);

            await _unitOfWork.SaveChangesAsync();

            return newIsActive;
        }

        public async Task ResetPasswordAsync(int id, ResetPasswordViewModel model, int actorUserId, string ipAddress)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);

            if (user == null)
                throw new InvalidOperationException("Tài khoản không tồn tại.");

            if (id == actorUserId)
            {
                throw new InvalidOperationException("Bạn không thể đặt lại mật khẩu của chính mình. Vui lòng sử dụng chức năng Đổi mật khẩu.");
            }

            ValidatePassword(model.NewPassword);

            user.PasswordHash = PasswordHelper.HashPassword(model.NewPassword);

            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;

            await _auditLogService.RecordAsync("ResetPassword", "ApplicationUser", user.Id, $"Đặt lại mật khẩu cho {user.Email}", ipAddress, actorUserId);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null)
                throw new InvalidOperationException("Tài khoản không tồn tại.");

            if (!PasswordHelper.VerifyPassword(oldPassword, user.PasswordHash))
            {
                throw new InvalidOperationException("Mật khẩu hiện tại không đúng.");
            }

            ValidatePassword(newPassword);

            user.PasswordHash = PasswordHelper.HashPassword(newPassword);

            await _unitOfWork.SaveChangesAsync();
        }

        private static void ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8 || !password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit))
            {
                throw new InvalidOperationException("Mật khẩu phải có ít nhất 8 ký tự, gồm chữ hoa, chữ thường và số.");
            }
        }
        private static string GetRoleDisplayName(string? roleName)
        {
            return roleName switch
            {
                "Admin" => "Quản trị viên",
                "Manager" => "Quản lý",
                "Staff" => "Nhân viên",
                _ => "Không xác định"
            };
        }
    }
}