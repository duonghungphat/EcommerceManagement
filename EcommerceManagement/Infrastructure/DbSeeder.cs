using EcommerceManagement.Core.Models;
using EcommerceManagement.Data.Context;
using Microsoft.EntityFrameworkCore;
using EcommerceManagement.Service.Security;
using Microsoft.Extensions.Configuration;

namespace EcommerceManagement.Infrastructure
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            string[] roleNames = { "Admin", "Manager", "Staff" };

            foreach (var roleName in roleNames)
            {
                bool exists = await db.Roles
                    .AnyAsync(r => r.Name == roleName);

                if (!exists)
                {
                    db.Roles.Add(new Role
                    {
                        Name = roleName
                    });
                }
            }

            await db.SaveChangesAsync();
        }

        public static async Task SeedAdminAsync(IServiceProvider services,IConfiguration configuration)
        {
            using var scope = services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            // 1. Tìm Role Admin đã seed ở bước trước
            var adminRole = await db.Roles
                .SingleAsync(r => r.Name == "Admin");

            // 2. Nếu hệ thống đã có Admin thì không tạo thêm
            bool adminExists = await db.ApplicationUsers
                .AnyAsync(u => u.RoleId == adminRole.Id);

            if (adminExists)
            {
                return;
            }

            // 3. Lấy thông tin Admin đầu tiên từ User Secrets
            string email = configuration["SeedAdmin:Email"]?.Trim()
                ?? throw new InvalidOperationException(
                    "Chưa cấu hình SeedAdmin:Email.");

            string password = configuration["SeedAdmin:Password"]
                ?? throw new InvalidOperationException(
                    "Chưa cấu hình SeedAdmin:Password.");

            string fullName = configuration["SeedAdmin:FullName"]?.Trim()
                ?? throw new InvalidOperationException(
                    "Chưa cấu hình SeedAdmin:FullName.");

            // 4. Không cho tạo Admin nếu email đã thuộc tài khoản khác
            bool emailExists = await db.ApplicationUsers
                .AnyAsync(u => u.Email == email);

            if (emailExists)
            {
                throw new InvalidOperationException(
                    "Email dùng để seed Admin đã tồn tại.");
            }

            // 5. Tạo tài khoản Admin
            var admin = new ApplicationUser
            {
                FullName = fullName,
                Email = email,
                PasswordHash = PasswordHelper.HashPassword(password),
                RoleId = adminRole.Id,
                IsActive = true
            };

            db.ApplicationUsers.Add(admin);

            await db.SaveChangesAsync();
        }
    }
}