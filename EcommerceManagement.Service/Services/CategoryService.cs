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
        private readonly IAuditLogService _auditLogService;

        public CategoryService(IUnitOfWork unitOfWork, IAuditLogService auditLogService)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
        }

        public async Task<List<CategoryViewModel>> GetAllAsync()
        {
            return await _unitOfWork.Categories
                .BuildQuery(c => true)
                .OrderBy(c => c.ParentCategoryId.HasValue)
                .ThenBy(c => c.ParentCategoryId)
                .ThenBy(c => c.Name)
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,

                    ParentCategoryId = c.ParentCategoryId,

                    ParentCategoryName = c.ParentCategory != null ? c.ParentCategory.Name : null
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
                    Description = c.Description,

                    ParentCategoryId = c.ParentCategoryId,

                    ParentCategoryName = c.ParentCategory != null ? c.ParentCategory.Name : null
                })
                .FirstOrDefaultAsync();
        }

        public async Task CreateAsync(CategoryViewModel model, int actorUserId, string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                throw new InvalidOperationException("Tên danh mục không được để trống.");
            }

            string name = model.Name.Trim();

            bool nameExists = await _unitOfWork.Categories
                .BuildQuery(c => c.Name == name)
                .AnyAsync();

            if (nameExists)
            {
                throw new InvalidOperationException("Tên danh mục đã tồn tại.");
            }

            if (model.ParentCategoryId.HasValue)
            {
                var parentCategory = await _unitOfWork.Categories
                        .GetByIdAsync(model.ParentCategoryId.Value);

                if (parentCategory == null)
                {
                    throw new InvalidOperationException("Danh mục cha không tồn tại.");
                }

                if (parentCategory.ParentCategoryId.HasValue)
                {
                    throw new InvalidOperationException("Chỉ hỗ trợ tối đa 2 cấp danh mục.");
                }
            }

            var category = new Category
            {
                Name = name,
                Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),

                ParentCategoryId = model.ParentCategoryId
            };

            await _unitOfWork.Categories.AddAsync(category);

            await _auditLogService.RecordAsync("CreateCategory", "Category", category.Id == 0 ? null : category.Id, $"Thêm danh mục: {category.Name}", ipAddress, actorUserId);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateAsync(CategoryViewModel model, int actorUserId, string ipAddress)
        {
            var category = await _unitOfWork.Categories
                    .GetByIdAsync(model.Id);

            if (category == null)
            {
                throw new InvalidOperationException("Danh mục không tồn tại.");
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                throw new InvalidOperationException("Tên danh mục không được để trống.");
            }

            string name = model.Name.Trim();

            bool nameExists = await _unitOfWork.Categories
                .BuildQuery(c => c.Name == name && c.Id != model.Id)
                .AnyAsync();

            if (nameExists)
            {
                throw new InvalidOperationException("Tên danh mục đã tồn tại.");
            }

            if (model.ParentCategoryId == model.Id)
            {
                throw new InvalidOperationException("Danh mục không thể là cha của chính nó.");
            }

            if (model.ParentCategoryId.HasValue)
            {
                var parentCategory = await _unitOfWork.Categories
                        .GetByIdAsync(model.ParentCategoryId.Value);

                if (parentCategory == null)
                {
                    throw new InvalidOperationException("Danh mục cha không tồn tại.");
                }

                if (parentCategory.ParentCategoryId.HasValue)
                {
                    throw new InvalidOperationException("Chỉ hỗ trợ tối đa 2 cấp danh mục.");
                }

                bool hasSubCategories = await _unitOfWork.Categories
                        .BuildQuery(c => c.ParentCategoryId == model.Id)
                        .AnyAsync();

                if (hasSubCategories)
                {
                    throw new InvalidOperationException("Danh mục đang có danh mục con nên không thể chuyển thành danh mục con.");
                }
            }

            category.Name = name;

            category.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();

            category.ParentCategoryId = model.ParentCategoryId;

            await _auditLogService.RecordAsync("UpdateCategory", "Category", category.Id, $"Cập nhật danh mục: {category.Name}", ipAddress, actorUserId);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id, int actorUserId, string ipAddress)
        {
            var category = await _unitOfWork.Categories
                    .GetByIdAsync(id);

            if (category == null)
            {
                throw new InvalidOperationException("Danh mục không tồn tại.");
            }

            bool hasProducts = await _unitOfWork.Products
                    .BuildQuery(p => p.CategoryId == id)
                    .AnyAsync();

            if (hasProducts)
            {
                throw new InvalidOperationException("Danh mục đang có sản phẩm nên không thể xóa.");
            }

            bool hasSubCategories = await _unitOfWork.Categories
                    .BuildQuery(c => c.ParentCategoryId == id)
                    .AnyAsync();

            if (hasSubCategories)
            {
                throw new InvalidOperationException("Danh mục đang có danh mục con nên không thể xóa.");
            }

            _unitOfWork.Categories.Delete(category);

            await _auditLogService.RecordAsync("DeleteCategory", "Category", category.Id, $"Xóa danh mục: {category.Name}", ipAddress, actorUserId);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}