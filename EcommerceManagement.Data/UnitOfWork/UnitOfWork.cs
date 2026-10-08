using EcommerceManagement.Core.Models;
using EcommerceManagement.Data.Context;
using EcommerceManagement.Data.Repository;

namespace EcommerceManagement.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IRepository<Product> Products { get; }
        public IRepository<ProductVariant> ProductVariants { get; }
        public IRepository<ProductAttributeDefinition> ProductAttributeDefinitions { get; }
        public IRepository<ProductAttributeValue> ProductAttributeValues { get; }
        public IRepository<ProductAttributeSelection> ProductAttributeSelections { get; }
        public IRepository<ProductVariantAttributeSelection> ProductVariantAttributeSelections { get; }
        public IRepository<CategoryAttributeDefinition> CategoryAttributeDefinitions { get; }

        public IRepository<Category> Categories { get; }
        public IRepository<Customer> Customers { get; }
        public IRepository<Order> Orders { get; }
        public IRepository<OrderItem> OrderItems { get; }
        public IRepository<Payment> Payments { get; }
        public IRepository<ApplicationUser> Users { get; }
        public IRepository<Role> Roles { get; }
        public IRepository<AuditLog> AuditLogs { get; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;

            Products = new Repository<Product>(context);
            ProductVariants = new Repository<ProductVariant>(context);
            ProductAttributeDefinitions = new Repository<ProductAttributeDefinition>(context);
            ProductAttributeValues = new Repository<ProductAttributeValue>(context);
            ProductAttributeSelections = new Repository<ProductAttributeSelection>(context);
            ProductVariantAttributeSelections = new Repository<ProductVariantAttributeSelection>(context);
            CategoryAttributeDefinitions = new Repository<CategoryAttributeDefinition>(context);

            Categories = new Repository<Category>(context);
            Customers = new Repository<Customer>(context);
            Orders = new Repository<Order>(context);
            OrderItems = new Repository<OrderItem>(context);
            Payments = new Repository<Payment>(context);
            Users = new Repository<ApplicationUser>(context);
            Roles = new Repository<Role>(context);
            AuditLogs = new Repository<AuditLog>(context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
