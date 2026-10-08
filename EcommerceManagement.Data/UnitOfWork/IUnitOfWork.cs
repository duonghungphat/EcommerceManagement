using EcommerceManagement.Core.Models;
using EcommerceManagement.Data.Repository;

namespace EcommerceManagement.Data.UnitOfWork
{
    public interface IUnitOfWork
    {
        IRepository<Product> Products { get; }
        IRepository<ProductVariant> ProductVariants { get; }
        IRepository<ProductAttributeDefinition> ProductAttributeDefinitions { get; }
        IRepository<ProductAttributeValue> ProductAttributeValues { get; }
        IRepository<ProductAttributeSelection> ProductAttributeSelections { get; }
        IRepository<ProductVariantAttributeSelection> ProductVariantAttributeSelections { get; }
        IRepository<CategoryAttributeDefinition> CategoryAttributeDefinitions { get; }

        IRepository<Category> Categories { get; }
        IRepository<Customer> Customers { get; }
        IRepository<Order> Orders { get; }
        IRepository<OrderItem> OrderItems { get; }
        IRepository<Payment> Payments { get; }
        IRepository<ApplicationUser> Users { get; }
        IRepository<Role> Roles { get; }
        IRepository<AuditLog> AuditLogs { get; }

        Task<int> SaveChangesAsync();
    }
}
