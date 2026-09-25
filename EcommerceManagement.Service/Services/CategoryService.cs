using EcommerceManagement.Core.Models;
using EcommerceManagement.Core.ViewModels;
using EcommerceManagement.Data.UnitOfWork;
using EcommerceManagement.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceManagement.Service.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CategoryViewModel>> GetAllAsync()
        {
            return await _unitOfWork.Categories
                .BuildQuery(c => true)
                .OrderBy(c => c.Name)
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                })
                .ToListAsync();
        }

        public async Task<CategoryViewModel?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Categories
                .BuildQuery(c => c.Id == id)
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                })
                .FirstOrDefaultAsync();
        }

        public async Task CreateAsync(CategoryViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                throw new InvalidOperationException("Tên danh mục không được để trống.");

            string name = model.Name.Trim();

            bool nameExists = await _unitOfWork.Categories
                .BuildQuery(c => c.Name == name)
                .AnyAsync();

            if (nameExists)
                throw new InvalidOperationException("Tên danh mục đã tồn tại.");

            var category = new Category
            {
                Name = name,
                Description = model.Description
            };

            await _unitOfWork.Categories.AddAsync(category);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateAsync(CategoryViewModel model)
        {
            var category = await _unitOfWork.Categories
                .GetByIdAsync(model.Id);

            if (category == null)
                throw new InvalidOperationException("Danh mục không tồn tại.");

            if (string.IsNullOrWhiteSpace(model.Name))
                throw new InvalidOperationException("Tên danh mục không được để trống.");

            string name = model.Name.Trim();

            bool nameExists = await _unitOfWork.Categories
                .BuildQuery(c => c.Name == name && c.Id != model.Id)
                .AnyAsync();

            if (nameExists)
                throw new InvalidOperationException("Tên danh mục đã tồn tại.");

            category.Name = name;
            category.Description = model.Description;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _unitOfWork.Categories
                .GetByIdAsync(id);

            if (category == null)
                throw new InvalidOperationException("Danh mục không tồn tại.");

            bool hasProducts = await _unitOfWork.Products
                .BuildQuery(p => p.CategoryId == id)
                .AnyAsync();

            if (hasProducts)
                throw new InvalidOperationException("Danh mục đang có sản phẩm nên không thể xóa.");

            _unitOfWork.Categories.Delete(category);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}