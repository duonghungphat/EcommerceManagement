using EcommerceManagement.Core.Models;
using EcommerceManagement.Core.ViewModels;
using EcommerceManagement.Data.UnitOfWork;
using EcommerceManagement.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceManagement.Service.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CustomerViewModel>> GetAllAsync()
        {
            return await _unitOfWork.Customers
                .BuildQuery(c => true)
                .OrderBy(c => c.FullName)
                .Select(c => new CustomerViewModel
                {
                    Id = c.Id,
                    FullName = c.FullName,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber,
                    Address = c.Address,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<CustomerViewModel?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Customers
                .BuildQuery(c => c.Id == id)
                .Select(c => new CustomerViewModel
                {
                    Id = c.Id,
                    FullName = c.FullName,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber,
                    Address = c.Address,
                    CreatedAt = c.CreatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task CreateAsync(CustomerViewModel model)
        {
            string email = model.Email.Trim();

            bool emailExists = await _unitOfWork.Customers
                .BuildQuery(c => c.Email == email)
                .AnyAsync();

            if (emailExists)
                throw new InvalidOperationException("Email đã tồn tại.");

            var customer = new Customer
            {
                FullName = model.FullName.Trim(),
                Email = email,
                PhoneNumber = model.PhoneNumber.Trim(),
                Address = model.Address.Trim(),
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.Customers.AddAsync(customer);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateAsync(CustomerViewModel model)
        {
            var customer = await _unitOfWork.Customers
                .GetByIdAsync(model.Id);

            if (customer == null)
                throw new InvalidOperationException("Khách hàng không tồn tại.");

            string email = model.Email.Trim();

            bool emailExists = await _unitOfWork.Customers
                .BuildQuery(c =>
                    c.Email == email &&
                    c.Id != model.Id)
                .AnyAsync();

            if (emailExists)
                throw new InvalidOperationException("Email đã tồn tại.");

            customer.FullName = model.FullName.Trim();
            customer.Email = email;
            customer.PhoneNumber = model.PhoneNumber.Trim();
            customer.Address = model.Address.Trim();

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var customer = await _unitOfWork.Customers
                .GetByIdAsync(id);

            if (customer == null)
                throw new InvalidOperationException("Khách hàng không tồn tại.");

            bool hasOrders = await _unitOfWork.Orders
                .BuildQuery(o => o.CustomerId == id)
                .AnyAsync();

            if (hasOrders)
                throw new InvalidOperationException("Khách hàng đã có đơn hàng nên không thể xóa.");

            _unitOfWork.Customers.Delete(customer);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}