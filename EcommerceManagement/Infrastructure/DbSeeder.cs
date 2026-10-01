using EcommerceManagement.Core.Models;
using EcommerceManagement.Data.Context;
using Microsoft.EntityFrameworkCore;
using EcommerceManagement.Service.Security;
using EcommerceManagement.Core.Enums;
using Microsoft.Extensions.Configuration;

namespace EcommerceManagement.Infrastructure
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            string[] roleNames = { "Admin", "Manager", "Staff" };

            foreach (var roleName in roleNames)
            {
                bool exists = await db.Roles.AnyAsync(r => r.Name == roleName);

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

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // 1. Tìm Role Admin đã seed ở bước trước
            var adminRole = await db.Roles.SingleAsync(r => r.Name == "Admin");

            // 2. Nếu hệ thống đã có Admin thì không tạo thêm
            bool adminExists = await db.ApplicationUsers.AnyAsync(u => u.RoleId == adminRole.Id);

            if (adminExists)
            {
                return;
            }

            // 3. Lấy thông tin Admin đầu tiên
            string email = configuration["SeedAdmin:Email"]?.Trim() ?? throw new InvalidOperationException("Chưa cấu hình SeedAdmin:Email.");

            string password = configuration["SeedAdmin:Password"] ?? throw new InvalidOperationException("Chưa cấu hình SeedAdmin:Password.");

            string fullName = configuration["SeedAdmin:FullName"]?.Trim() ?? throw new InvalidOperationException("Chưa cấu hình SeedAdmin:FullName.");

            // 4. Không cho tạo Admin nếu email đã thuộc tài khoản khác
            bool emailExists = await db.ApplicationUsers.AnyAsync(u => u.Email == email);

            if (emailExists)
            {
                throw new InvalidOperationException("Email dùng để seed Admin đã tồn tại.");
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
        public static async Task SeedSampleDataAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();


            // CATEGORY

            string[] categoryNames =
            {
                 "Điện thoại",
                 "Laptop",
                 "Phụ kiện"
            };

            foreach (string name in categoryNames)
            {
                bool exists = await db.Categories.AnyAsync(c =>  c.Name == name);

                if (!exists)
                {
                    db.Categories.Add(
                        new Category
                        {
                            Name = name,
                            Description = $"Danh mục {name}"
                        });
                }
            }

            await db.SaveChangesAsync();


            var phoneCategory = await db.Categories.FirstAsync(c => c.Name == "Điện thoại");

            var laptopCategory = await db.Categories.FirstAsync(c => c.Name == "Laptop");

            var accessoryCategory = await db.Categories.FirstAsync(c => c.Name == "Phụ kiện");


            // PRODUCT

            if (!await db.Products.AnyAsync(p => p.SKU == "PHONE001"))
            {
                db.Products.Add(
                    new Product
                    {
                        Name = "Điện thoại mẫu",
                        SKU = "PHONE001",
                        Price = 15000000,
                        StockQuantity = 20,
                        Status = ProductStatus.Selling,
                        CategoryId = phoneCategory.Id
                    });
            }


            if (!await db.Products.AnyAsync(p => p.SKU == "LAPTOP001"))
            {
                db.Products.Add(
                    new Product
                    {
                        Name = "Laptop mẫu",
                        SKU = "LAPTOP001",
                        Price = 25000000,
                        StockQuantity = 8,
                        Status = ProductStatus.Selling,
                        CategoryId = laptopCategory.Id
                    });
            }


            if (!await db.Products.AnyAsync(p => p.SKU == "ACCESSORY001"))
            {
                db.Products.Add(
                    new Product
                    {
                        Name = "Chuột không dây",
                        SKU = "ACCESSORY001",
                        Price = 500000,
                        StockQuantity = 30,
                        Status = ProductStatus.Selling,
                        CategoryId = accessoryCategory.Id
                    });
            }


            // CUSTOMER

            if (!await db.Customers.AnyAsync(c => c.Email == "khachhang1@example.com"))
            {
                db.Customers.Add(
                    new Customer
                    {
                        FullName = "Nguyễn Văn A",
                        Email = "khachhang1@example.com",
                        PhoneNumber = "0901234567",
                        Address = "Thành phố Hồ Chí Minh",
                        CreatedAt = DateTime.Now
                    });
            }


            if (!await db.Customers.AnyAsync(c => c.Email == "khachhang2@example.com"))
            {
                db.Customers.Add(
                    new Customer
                    {
                        FullName = "Trần Thị B",
                        Email = "khachhang2@example.com",
                        PhoneNumber = "0912345678",
                        Address = "Hà Nội",
                        CreatedAt = DateTime.Now
                    });
            }

            await db.SaveChangesAsync();
        }
    }
}