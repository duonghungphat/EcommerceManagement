using EcommerceManagement.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceManagement.Data.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<ProductVariant> ProductVariants { get; set; } = null!;
        public DbSet<ProductAttributeDefinition> ProductAttributeDefinitions { get; set; } = null!;
        public DbSet<ProductAttributeValue> ProductAttributeValues { get; set; } = null!;
        public DbSet<ProductAttributeSelection> ProductAttributeSelections { get; set; } = null!;
        public DbSet<ProductVariantAttributeSelection> ProductVariantAttributeSelections { get; set; } = null!;
        public DbSet<CategoryAttributeDefinition> CategoryAttributeDefinitions { get; set; } = null!;

        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<ApplicationUser> ApplicationUsers { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<Product>()
                .Property(p => p.ImagePath)
                .HasMaxLength(1000);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductVariant>()
                .Property(v => v.Name)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<ProductVariant>()
                .Property(v => v.SKU)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<ProductVariant>()
                .HasIndex(v => v.SKU)
                .IsUnique();

            modelBuilder.Entity<ProductVariant>()
                .Property(v => v.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProductVariant>()
                .Property(v => v.StockQuantity)
                .IsConcurrencyToken();

            modelBuilder.Entity<ProductVariant>()
                .HasOne(v => v.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductVariant>()
                .Property(v => v.ImagePath)
                .HasMaxLength(1000);

            modelBuilder.Entity<ProductAttributeDefinition>()
                .Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<ProductAttributeDefinition>()
                .Property(a => a.Code)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<ProductAttributeDefinition>()
                .HasIndex(a => a.Code)
                .IsUnique();

            modelBuilder.Entity<ProductAttributeValue>()
                .Property(v => v.Value)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<ProductAttributeValue>()
                .HasIndex(v => new { v.AttributeDefinitionId, v.Value })
                .IsUnique();

            // Alternate key này giúp DB đảm bảo AttributeValue thật sự thuộc đúng AttributeDefinition.
            modelBuilder.Entity<ProductAttributeValue>()
                .HasAlternateKey(v => new { v.Id, v.AttributeDefinitionId });

            modelBuilder.Entity<ProductAttributeValue>()
                .HasOne(v => v.AttributeDefinition)
                .WithMany(a => a.Values)
                .HasForeignKey(v => v.AttributeDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductAttributeSelection>()
                .HasIndex(s => new { s.ProductId, s.AttributeValueId })
                .IsUnique();

            modelBuilder.Entity<ProductAttributeSelection>()
                .HasOne(s => s.Product)
                .WithMany(p => p.AttributeSelections)
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductAttributeSelection>()
                .HasOne(s => s.AttributeDefinition)
                .WithMany()
                .HasForeignKey(s => s.AttributeDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductAttributeSelection>()
                .HasOne(s => s.AttributeValue)
                .WithMany(v => v.ProductSelections)
                .HasForeignKey(s => new { s.AttributeValueId, s.AttributeDefinitionId })
                .HasPrincipalKey(v => new { v.Id, v.AttributeDefinitionId })
                .OnDelete(DeleteBehavior.Restrict);

            // Một Variant chỉ được có tối đa một giá trị cho mỗi loại thuộc tính.
            // Ví dụ không thể vừa Màu = Xanh vừa Màu = Đỏ trên cùng một SKU.
            modelBuilder.Entity<ProductVariantAttributeSelection>()
                .HasIndex(s => new { s.ProductVariantId, s.AttributeDefinitionId })
                .IsUnique();

            modelBuilder.Entity<ProductVariantAttributeSelection>()
                .HasOne(s => s.ProductVariant)
                .WithMany(v => v.AttributeSelections)
                .HasForeignKey(s => s.ProductVariantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductVariantAttributeSelection>()
                .HasOne(s => s.AttributeDefinition)
                .WithMany()
                .HasForeignKey(s => s.AttributeDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductVariantAttributeSelection>()
                .HasOne(s => s.AttributeValue)
                .WithMany(v => v.VariantSelections)
                .HasForeignKey(s => new { s.AttributeValueId, s.AttributeDefinitionId })
                .HasPrincipalKey(v => new { v.Id, v.AttributeDefinitionId })
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CategoryAttributeDefinition>()
                .HasIndex(x => new { x.CategoryId, x.AttributeDefinitionId })
                .IsUnique();

            modelBuilder.Entity<CategoryAttributeDefinition>()
                .HasOne(x => x.Category)
                .WithMany(c => c.AttributeDefinitions)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CategoryAttributeDefinition>()
                .HasOne(x => x.AttributeDefinition)
                .WithMany(a => a.CategoryMappings)
                .HasForeignKey(x => x.AttributeDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>()
                .Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Category>()
                .Property(c => c.Description)
                .HasMaxLength(500);

            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderCode)
                .IsUnique();

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.OrderCode)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.CreatedByUser)
                .WithMany(u => u.CreatedOrders)
                .HasForeignKey(o => o.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.Payments)
                .WithOne(p => p.Order)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .Property(o => o.PaymentVersion)
                .IsConcurrencyToken();

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.ProductVariant)
                .WithMany(v => v.OrderItems)
                .HasForeignKey(oi => oi.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique();

            modelBuilder.Entity<Customer>()
                .Property(c => c.FullName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Customer>()
                .Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Customer>()
                .Property(c => c.PhoneNumber)
                .IsRequired()
                .HasMaxLength(10);

            modelBuilder.Entity<Customer>()
                .Property(c => c.Address)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Orders)
                .WithOne(o => o.Customer)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .IsRequired()
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(p => p.TransactionCode)
                .HasMaxLength(100);

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.AuditLogs)
                .WithOne(al => al.ApplicationUser)
                .HasForeignKey(al => al.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();

            modelBuilder.Entity<Role>()
                .Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<AuditLog>()
                .Property(al => al.Description)
                .HasMaxLength(200);

            modelBuilder.Entity<AuditLog>()
                .Property(al => al.Action)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<AuditLog>()
                .Property(al => al.IpAddress)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<AuditLog>()
                .Property(al => al.EntityName)
                .IsRequired()
                .HasMaxLength(100);
        }
    }
}
