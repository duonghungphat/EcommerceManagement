using EcommerceManagement.Core.Enums;
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
        private readonly IAuditLogService _auditLogService;

        public ProductService(IUnitOfWork unitOfWork, IAuditLogService auditLogService)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
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
                    Price = p.Variants.Any() ? p.Variants.Min(v => v.Price) : p.Price,
                    StockQuantity = p.Variants.Any() ? p.Variants.Sum(v => v.StockQuantity) : p.StockQuantity,
                    Status = p.Status,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    ImagePath = p.ImagePath,
                    VariantCount = p.Variants.Count()
                })
                .ToListAsync();
        }

        public async Task<ProductViewModel?> GetByIdAsync(int id)
        {
            var model = await _unitOfWork.Products
                .BuildQuery(p => p.Id == id)
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    Price = p.Variants.Any() ? p.Variants.Min(v => v.Price) : p.Price,
                    StockQuantity = p.Variants.Any() ? p.Variants.Sum(v => v.StockQuantity) : p.StockQuantity,
                    OriginalStockQuantity = p.StockQuantity,
                    Status = p.Status,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    ImagePath = p.ImagePath,
                    VariantCount = p.Variants.Count(),
                    Variants = p.Variants
                        .OrderBy(v => v.Id)
                        .Select(v => new ProductVariantViewModel
                        {
                            Id = v.Id,
                            Name = v.Name,
                            SKU = v.SKU,
                            Price = v.Price,
                            StockQuantity = v.StockQuantity,
                            OriginalStockQuantity = v.StockQuantity,
                            ProductId = v.ProductId
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (model == null)
                return null;

            // Sản phẩm cũ chưa có variant sẽ được chuyển thành một variant mặc định khi Edit.
            if (model.Variants.Count == 0)
            {
                model.Variants.Add(new ProductVariantViewModel
                {
                    Name = "Mặc định",
                    SKU = model.SKU,
                    Price = model.Price,
                    StockQuantity = model.StockQuantity,
                    OriginalStockQuantity = model.StockQuantity,
                    ProductId = model.Id
                });
            }

            return model;
        }

        public async Task CreateAsync(ProductViewModel model, int actorUserId, string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                throw new InvalidOperationException("Tên sản phẩm không được để trống.");

            bool categoryExists = await _unitOfWork.Categories
                .BuildQuery(c => c.Id == model.CategoryId)
                .AnyAsync();

            if (!categoryExists)
                throw new InvalidOperationException("Danh mục không tồn tại.");

            var variants = NormalizeVariants(model.Variants);

            await ValidateVariantsAsync(variants, null);

            int totalStock = CalculateTotalStock(variants);
            decimal minimumPrice = variants.Min(v => v.Price);
            string firstSku = variants.First().SKU;

            var product = new Product
            {
                Name = model.Name.Trim(),
                SKU = firstSku,
                Price = minimumPrice,
                StockQuantity = totalStock,
                Status = model.Status,
                CategoryId = model.CategoryId,
                ImagePath = model.ImagePath
            };

            foreach (var variant in variants)
            {
                product.Variants.Add(new ProductVariant
                {
                    Name = variant.Name,
                    SKU = variant.SKU,
                    Price = variant.Price,
                    StockQuantity = variant.StockQuantity
                });
            }

            await _unitOfWork.Products.AddAsync(product);

            await _auditLogService.RecordAsync(
                "CreateProduct",
                "Product",
                null,
                $"Thêm sản phẩm: {product.Name} với {variants.Count} biến thể.",
                ipAddress,
                actorUserId);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProductViewModel model, int actorUserId, string ipAddress)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(model.Id);

            if (product == null)
                throw new InvalidOperationException("Sản phẩm không tồn tại.");

            if (product.StockQuantity != model.OriginalStockQuantity)
                throw new InvalidOperationException("Tồn kho sản phẩm đã thay đổi trong lúc bạn chỉnh sửa. Vui lòng tải lại trang và thử lại.");

            if (string.IsNullOrWhiteSpace(model.Name))
                throw new InvalidOperationException("Tên sản phẩm không được để trống.");

            bool categoryExists = await _unitOfWork.Categories
                .BuildQuery(c => c.Id == model.CategoryId)
                .AnyAsync();

            if (!categoryExists)
                throw new InvalidOperationException("Danh mục không tồn tại.");

            var variants = NormalizeVariants(model.Variants);

            await ValidateVariantsAsync(variants, model.Id);

            var existingVariantIds = await _unitOfWork.ProductVariants
                .BuildQuery(v => v.ProductId == model.Id)
                .Select(v => v.Id)
                .ToListAsync();

            var postedVariantIds = variants
                .Where(v => v.Id > 0)
                .Select(v => v.Id)
                .ToHashSet();

            if (postedVariantIds.Any(id => !existingVariantIds.Contains(id)))
                throw new InvalidOperationException("Có biến thể không thuộc sản phẩm này.");

            foreach (int existingId in existingVariantIds)
            {
                if (postedVariantIds.Contains(existingId))
                    continue;

                var variantToDelete = await _unitOfWork.ProductVariants.GetByIdAsync(existingId);

                if (variantToDelete != null)
                    _unitOfWork.ProductVariants.Delete(variantToDelete);
            }

            foreach (var variantModel in variants)
            {
                if (variantModel.Id == 0)
                {
                    await _unitOfWork.ProductVariants.AddAsync(new ProductVariant
                    {
                        ProductId = product.Id,
                        Name = variantModel.Name,
                        SKU = variantModel.SKU,
                        Price = variantModel.Price,
                        StockQuantity = variantModel.StockQuantity
                    });

                    continue;
                }

                var variant = await _unitOfWork.ProductVariants.GetByIdAsync(variantModel.Id);

                if (variant == null || variant.ProductId != product.Id)
                    throw new InvalidOperationException("Biến thể sản phẩm không tồn tại.");

                if (variant.StockQuantity != variantModel.OriginalStockQuantity)
                    throw new InvalidOperationException($"Tồn kho biến thể {variant.Name} đã thay đổi. Vui lòng tải lại trang và thử lại.");

                variant.Name = variantModel.Name;
                variant.SKU = variantModel.SKU;
                variant.Price = variantModel.Price;
                variant.StockQuantity = variantModel.StockQuantity;
            }

            product.Name = model.Name.Trim();
            product.SKU = variants.First().SKU;
            product.Price = variants.Min(v => v.Price);
            product.StockQuantity = CalculateTotalStock(variants);
            product.Status = model.Status;
            product.CategoryId = model.CategoryId;
            product.ImagePath = model.ImagePath;

            await _auditLogService.RecordAsync(
                "UpdateProduct",
                "Product",
                product.Id,
                $"Cập nhật sản phẩm: {product.Name} với {variants.Count} biến thể.",
                ipAddress,
                actorUserId);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException("Dữ liệu sản phẩm vừa thay đổi. Vui lòng tải lại trang và thử lại.", ex);
            }
        }

        public async Task ToggleStatusAsync(int id, int actorUserId, string ipAddress)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);

            if (product == null)
                throw new InvalidOperationException("Sản phẩm không tồn tại.");

            ProductStatus oldStatus = product.Status;
            ProductStatus newStatus = oldStatus == ProductStatus.Selling ? ProductStatus.Stopped : ProductStatus.Selling;

            product.Status = newStatus;

            string oldStatusText = oldStatus == ProductStatus.Selling ? "Đang bán" : "Ngừng bán";
            string newStatusText = newStatus == ProductStatus.Selling ? "Đang bán" : "Ngừng bán";

            await _auditLogService.RecordAsync(
                "UpdateProductStatus",
                "Product",
                product.Id,
                $"Đổi trạng thái sản phẩm {product.Name} từ {oldStatusText} sang {newStatusText}.",
                ipAddress,
                actorUserId);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException("Sản phẩm vừa được thay đổi bởi một thao tác khác. Vui lòng tải lại trang và thử lại.", ex);
            }
        }

        public async Task DeleteAsync(int id, int actorUserId, string ipAddress)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);

            if (product == null)
                throw new InvalidOperationException("Sản phẩm không tồn tại.");

            bool hasOrders = await _unitOfWork.OrderItems
                .BuildQuery(item => item.ProductId == id)
                .AnyAsync();

            if (hasOrders)
                throw new InvalidOperationException("Sản phẩm đã có đơn hàng. Hãy chuyển sang trạng thái ngừng bán.");

            _unitOfWork.Products.Delete(product);

            await _auditLogService.RecordAsync(
                "DeleteProduct",
                "Product",
                product.Id,
                $"Xóa sản phẩm: {product.Name}",
                ipAddress,
                actorUserId);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ProductListViewModel> GetPagedAsync(string? searchTerm, int? categoryId, int page, int pageSize)
        {
            page = Math.Max(page, 1);

            var query = _unitOfWork.Products.BuildQuery(p => true);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();

                query = query.Where(p =>
                    p.Name.Contains(searchTerm) ||
                    p.SKU.Contains(searchTerm) ||
                    p.Variants.Any(v => v.SKU.Contains(searchTerm)));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            int totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            if (totalPages > 0 && page > totalPages)
                page = totalPages;

            var products = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    Price = p.Variants.Any() ? p.Variants.Min(v => v.Price) : p.Price,
                    StockQuantity = p.Variants.Any() ? p.Variants.Sum(v => v.StockQuantity) : p.StockQuantity,
                    Status = p.Status,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    ImagePath = p.ImagePath,
                    VariantCount = p.Variants.Count()
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

        private List<ProductVariantViewModel> NormalizeVariants(List<ProductVariantViewModel>? variants)
        {
            variants ??= new List<ProductVariantViewModel>();

            var result = variants
                .Where(v =>
                    v.Id > 0 ||
                    !string.IsNullOrWhiteSpace(v.Name) ||
                    !string.IsNullOrWhiteSpace(v.SKU) ||
                    v.Price != 0 ||
                    v.StockQuantity != 0)
                .ToList();

            if (result.Count == 0)
                throw new InvalidOperationException("Sản phẩm phải có ít nhất một biến thể.");

            foreach (var variant in result)
            {
                variant.Name = variant.Name.Trim();
                variant.SKU = variant.SKU.Trim();
            }

            return result;
        }

        private async Task ValidateVariantsAsync(List<ProductVariantViewModel> variants, int? productId)
        {
            var duplicateSku = variants
                .GroupBy(v => v.SKU, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateSku != null)
                throw new InvalidOperationException($"SKU {duplicateSku.Key} đang bị trùng giữa các biến thể.");

            foreach (var variant in variants)
            {
                if (string.IsNullOrWhiteSpace(variant.Name))
                    throw new InvalidOperationException("Tên biến thể không được để trống.");

                if (string.IsNullOrWhiteSpace(variant.SKU))
                    throw new InvalidOperationException("SKU biến thể không được để trống.");

                if (variant.Price <= 0)
                    throw new InvalidOperationException($"Giá của biến thể {variant.Name} phải lớn hơn 0.");

                if (variant.StockQuantity < 0)
                    throw new InvalidOperationException($"Tồn kho của biến thể {variant.Name} không được âm.");

                bool variantSkuExists = await _unitOfWork.ProductVariants
                    .BuildQuery(v => v.SKU == variant.SKU && v.Id != variant.Id)
                    .AnyAsync();

                if (variantSkuExists)
                    throw new InvalidOperationException($"SKU {variant.SKU} đã tồn tại.");

                bool legacyProductSkuExists = await _unitOfWork.Products
                    .BuildQuery(p => p.SKU == variant.SKU && (!productId.HasValue || p.Id != productId.Value))
                    .AnyAsync();

                if (legacyProductSkuExists)
                    throw new InvalidOperationException($"SKU {variant.SKU} đã được sử dụng bởi sản phẩm khác.");
            }
        }

        private int CalculateTotalStock(List<ProductVariantViewModel> variants)
        {
            long total = variants.Sum(v => (long)v.StockQuantity);

            if (total > int.MaxValue)
                throw new InvalidOperationException("Tổng tồn kho vượt quá giới hạn cho phép.");

            return (int)total;
        }
    }
}