using EcommerceManagement.Core.Models;
using EcommerceManagement.Data.Repository;

namespace EcommerceManagement.Data.UnitOfWork
{
    public interface IUnitOfWork
    {
        IRepository<Product> Products { get; }
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