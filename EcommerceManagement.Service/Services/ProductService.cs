using EcommerceManagement.Core.Models;
using EcommerceManagement.Core.ViewModels;
using EcommerceManagement.Data.UnitOfWork;
using EcommerceManagement.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceManagement.Service.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ProductViewModel>> GetAllAsync()
        {
            return await _unitOfWork.Products
                .BuildQuery(p => true)
                .OrderBy(p => p.Name)
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    Status = p.Status,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    ImagePath = p.ImagePath
                })
                .ToListAsync();
        }

        public async Task<ProductViewModel?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Products
                .BuildQuery(p => p.Id == id)
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    Status = p.Status,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    ImagePath = p.ImagePath
                })
                .FirstOrDefaultAsync();
        }

        public async Task CreateAsync(ProductViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                throw new InvalidOperationException("Tên sản phẩm không được để trống.");

            if (string.IsNullOrWhiteSpace(model.SKU))
                throw new InvalidOperationException("SKU không được để trống.");

            if (model.Price <= 0)
                throw new InvalidOperationException("Giá sản phẩm phải lớn hơn 0.");

            if (model.StockQuantity < 0)
                throw new InvalidOperationException("Số lượng tồn kho không hợp lệ.");

            string sku = model.SKU.Trim();

            bool skuExists = await _unitOfWork.Products
                .BuildQuery(p => p.SKU == sku)
                .AnyAsync();

            if (skuExists)
                throw new InvalidOperationException("SKU đã tồn tại.");

            bool categoryExists = await _unitOfWork.Categories
                .BuildQuery(c => c.Id == model.CategoryId)
                .AnyAsync();

            if (!categoryExists)
                throw new InvalidOperationException("Danh mục không tồn tại.");

            var product = new Product
            {
                Name = model.Name.Trim(),
                SKU = sku,
                Price = model.Price,
                StockQuantity = model.StockQuantity,
                Status = model.Status,
                CategoryId = model.CategoryId,
                ImagePath = model.ImagePath
            };

            await _unitOfWork.Products.AddAsync(product);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProductViewModel model)
        {
            var product = await _unitOfWork.Products
                .GetByIdAsync(model.Id);

            if (product == null)
                throw new InvalidOperationException("Sản phẩm không tồn tại.");

            if (string.IsNullOrWhiteSpace(model.Name))
                throw new InvalidOperationException("Tên sản phẩm không được để trống.");

            if (string.IsNullOrWhiteSpace(model.SKU))
                throw new InvalidOperationException("SKU không được để trống.");

            if (model.Price <= 0)
                throw new InvalidOperationException("Giá sản phẩm phải lớn hơn 0.");

            if (model.StockQuantity < 0)
                throw new InvalidOperationException("Số lượng tồn kho không hợp lệ.");

            string sku = model.SKU.Trim();

            bool skuExists = await _unitOfWork.Products
                .BuildQuery(p => p.SKU == sku && p.Id != model.Id)
                .AnyAsync();

            if (skuExists)
                throw new InvalidOperationException("SKU đã tồn tại.");

            bool categoryExists = await _unitOfWork.Categories
                .BuildQuery(c => c.Id == model.CategoryId)
                .AnyAsync();

            if (!categoryExists)
                throw new InvalidOperationException("Danh mục không tồn tại.");

            product.Name = model.Name.Trim();
            product.SKU = sku;
            product.Price = model.Price;
            product.StockQuantity = model.StockQuantity;
            product.Status = model.Status;
            product.CategoryId = model.CategoryId;
            product.ImagePath = model.ImagePath;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _unitOfWork.Products
                .GetByIdAsync(id);

            if (product == null)
                throw new InvalidOperationException("Sản phẩm không tồn tại.");

            bool hasOrders = await _unitOfWork.OrderItems
                .BuildQuery(item => item.ProductId == id)
                .AnyAsync();

            if (hasOrders)
                throw new InvalidOperationException("Sản phẩm đã có đơn hàng. Hãy chuyển sang trạng thái ngừng bán.");

            _unitOfWork.Products.Delete(product);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ProductListViewModel> GetPagedAsync(string? searchTerm, int? categoryId, int page, int pageSize)
        {
            page = Math.Max(page, 1);

            var query = _unitOfWork.Products
                .BuildQuery(p => true);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();

                query = query.Where(p => p.Name.Contains(searchTerm) || p.SKU.Contains(searchTerm));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            int totalCount = await query.CountAsync();

            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            var products = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    Status = p.Status,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    ImagePath = p.ImagePath
                })
                .ToListAsync();

            return new ProductListViewModel
            {
                Products = products,
                SearchTerm = searchTerm,
                CategoryId = categoryId,
                Page = page,
                TotalPages = totalPages
            };
        }
    }
}