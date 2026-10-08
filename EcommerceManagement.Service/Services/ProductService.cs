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
                    MinPrice = p.Variants.Select(v => (decimal?)v.Price).Min() ?? 0m,
                    MaxPrice = p.Variants.Select(v => (decimal?)v.Price).Max() ?? 0m,
                    TotalStockQuantity = p.Variants.Select(v => (int?)v.StockQuantity).Sum() ?? 0,
                    Status = p.Status,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    ImagePath = p.ImagePath,
                    VariantCount = p.Variants.Count()
                })
                .ToListAsync();
        }

        public async Task<List<ProductVariantOptionViewModel>> GetSellableVariantsAsync()
        {
            return await _unitOfWork.ProductVariants
                .BuildQuery(v => v.Product.Status == ProductStatus.Selling && v.StockQuantity > 0)
                .OrderBy(v => v.Product.Name)
                .ThenBy(v => v.Name)
                .Select(v => new ProductVariantOptionViewModel
                {
                    Id = v.Id,
                    ProductId = v.ProductId,
                    ProductName = v.Product.Name,
                    VariantName = v.Name,
                    SKU = v.SKU,
                    Price = v.Price,
                    StockQuantity = v.StockQuantity
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
                    MinPrice = p.Variants.Select(v => (decimal?)v.Price).Min() ?? 0m,
                    MaxPrice = p.Variants.Select(v => (decimal?)v.Price).Max() ?? 0m,
                    TotalStockQuantity = p.Variants.Select(v => (int?)v.StockQuantity).Sum() ?? 0,
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
                            ProductId = v.ProductId,
                            ImagePath = v.ImagePath
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (model == null)
                return null;

            var productSelections = await _unitOfWork.ProductAttributeSelections
                .BuildQuery(s => s.ProductId == id)
                .Select(s => new
                {
                    s.AttributeDefinitionId,
                    s.AttributeValueId
                })
                .ToListAsync();

            model.ProductAttributes = productSelections
                .GroupBy(s => s.AttributeDefinitionId)
                .Select(g => new ProductAttributeInputViewModel
                {
                    DefinitionId = g.Key,
                    SelectedValueIds = g.Select(x => x.AttributeValueId).Distinct().ToList()
                })
                .ToList();

            var variantSelections = await _unitOfWork.ProductVariantAttributeSelections
                .BuildQuery(s => s.ProductVariant.ProductId == id)
                .Select(s => new
                {
                    s.ProductVariantId,
                    s.AttributeDefinitionId,
                    s.AttributeValueId
                })
                .ToListAsync();

            var selectionsByVariant = variantSelections
                .GroupBy(s => s.ProductVariantId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.AttributeValueId).Distinct().ToList());

            foreach (var variant in model.Variants)
            {
                if (selectionsByVariant.TryGetValue(variant.Id, out var valueIds))
                    variant.AttributeValueIds = valueIds;
            }

            model.VariantAttributes = variantSelections
                .GroupBy(s => s.AttributeDefinitionId)
                .Select(g => new ProductAttributeInputViewModel
                {
                    DefinitionId = g.Key,
                    SelectedValueIds = g.Select(x => x.AttributeValueId).Distinct().ToList()
                })
                .ToList();

            if (model.Variants.Count == 0)
            {
                model.Variants.Add(new ProductVariantViewModel
                {
                    Name = "Mặc định",
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

            var productSelections = await ValidateProductSelectionsAsync(model.ProductAttributes, model.CategoryId);
            var variants = NormalizeVariants(model.Variants);

            await PrepareAndValidateVariantsAsync(variants, null, model.CategoryId);

            var product = new Product
            {
                Name = model.Name.Trim(),
                Status = model.Status,
                CategoryId = model.CategoryId,
                ImagePath = model.ImagePath
            };

            foreach (var selection in productSelections)
            {
                product.AttributeSelections.Add(new ProductAttributeSelection
                {
                    AttributeDefinitionId = selection.DefinitionId,
                    AttributeValueId = selection.ValueId
                });
            }

            foreach (var variantModel in variants)
            {
                var variant = new ProductVariant
                {
                    Name = variantModel.Name,
                    SKU = variantModel.SKU,
                    Price = variantModel.Price,
                    StockQuantity = variantModel.StockQuantity,
                    ImagePath = variantModel.ImagePath
                };

                foreach (var valueId in variantModel.AttributeValueIds.Distinct())
                {
                    var info = await GetAttributeValueInfoAsync(valueId);

                    variant.AttributeSelections.Add(new ProductVariantAttributeSelection
                    {
                        AttributeDefinitionId = info.DefinitionId,
                        AttributeValueId = info.ValueId
                    });
                }

                product.Variants.Add(variant);
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

            if (string.IsNullOrWhiteSpace(model.Name))
                throw new InvalidOperationException("Tên sản phẩm không được để trống.");

            bool categoryExists = await _unitOfWork.Categories
                .BuildQuery(c => c.Id == model.CategoryId)
                .AnyAsync();

            if (!categoryExists)
                throw new InvalidOperationException("Danh mục không tồn tại.");

            var productSelections = await ValidateProductSelectionsAsync(model.ProductAttributes, model.CategoryId);
            var variants = NormalizeVariants(model.Variants);

            await PrepareAndValidateVariantsAsync(variants, model.Id, model.CategoryId);

            var existingProductSelections = await _unitOfWork.ProductAttributeSelections
                .BuildQuery(s => s.ProductId == model.Id)
                .ToListAsync();

            foreach (var selection in existingProductSelections)
                _unitOfWork.ProductAttributeSelections.Delete(selection);

            foreach (var selection in productSelections)
            {
                await _unitOfWork.ProductAttributeSelections.AddAsync(new ProductAttributeSelection
                {
                    ProductId = product.Id,
                    AttributeDefinitionId = selection.DefinitionId,
                    AttributeValueId = selection.ValueId
                });
            }

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

                bool variantHasOrders = await _unitOfWork.OrderItems
                    .BuildQuery(i => i.ProductVariantId == existingId)
                    .AnyAsync();

                if (variantHasOrders)
                    throw new InvalidOperationException("Biến thể đã phát sinh đơn hàng nên không thể xóa.");

                var oldSelections = await _unitOfWork.ProductVariantAttributeSelections
                    .BuildQuery(s => s.ProductVariantId == existingId)
                    .ToListAsync();

                foreach (var selection in oldSelections)
                    _unitOfWork.ProductVariantAttributeSelections.Delete(selection);

                var variantToDelete = await _unitOfWork.ProductVariants.GetByIdAsync(existingId);

                if (variantToDelete != null)
                    _unitOfWork.ProductVariants.Delete(variantToDelete);
            }

            foreach (var variantModel in variants)
            {
                if (variantModel.Id == 0)
                {
                    var newVariant = new ProductVariant
                    {
                        ProductId = product.Id,
                        Name = variantModel.Name,
                        SKU = variantModel.SKU,
                        Price = variantModel.Price,
                        StockQuantity = variantModel.StockQuantity,
                        ImagePath = variantModel.ImagePath
                    };

                    foreach (var valueId in variantModel.AttributeValueIds.Distinct())
                    {
                        var info = await GetAttributeValueInfoAsync(valueId);

                        newVariant.AttributeSelections.Add(new ProductVariantAttributeSelection
                        {
                            AttributeDefinitionId = info.DefinitionId,
                            AttributeValueId = info.ValueId
                        });
                    }

                    await _unitOfWork.ProductVariants.AddAsync(newVariant);
                    continue;
                }

                var variant = await _unitOfWork.ProductVariants.GetByIdAsync(variantModel.Id);

                if (variant == null || variant.ProductId != product.Id)
                    throw new InvalidOperationException("Biến thể sản phẩm không tồn tại.");

                if (variant.StockQuantity != variantModel.OriginalStockQuantity)
                    throw new InvalidOperationException($"Tồn kho biến thể {variant.Name} đã thay đổi. Vui lòng tải lại trang và thử lại.");

                var oldSelections = await _unitOfWork.ProductVariantAttributeSelections
                    .BuildQuery(s => s.ProductVariantId == variant.Id)
                    .ToListAsync();

                foreach (var selection in oldSelections)
                    _unitOfWork.ProductVariantAttributeSelections.Delete(selection);

                foreach (var valueId in variantModel.AttributeValueIds.Distinct())
                {
                    var info = await GetAttributeValueInfoAsync(valueId);

                    await _unitOfWork.ProductVariantAttributeSelections.AddAsync(new ProductVariantAttributeSelection
                    {
                        ProductVariantId = variant.Id,
                        AttributeDefinitionId = info.DefinitionId,
                        AttributeValueId = info.ValueId
                    });
                }

                variant.Name = variantModel.Name;
                variant.SKU = variantModel.SKU;
                variant.Price = variantModel.Price;
                variant.StockQuantity = variantModel.StockQuantity;
                variant.ImagePath = variantModel.ImagePath;
            }

            product.Name = model.Name.Trim();
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
                .BuildQuery(item => item.ProductVariant.ProductId == id)
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
                    MinPrice = p.Variants.Select(v => (decimal?)v.Price).Min() ?? 0m,
                    MaxPrice = p.Variants.Select(v => (decimal?)v.Price).Max() ?? 0m,
                    TotalStockQuantity = p.Variants.Select(v => (int?)v.StockQuantity).Sum() ?? 0,
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
                    !string.IsNullOrWhiteSpace(v.SKU) ||
                    v.Price != 0 ||
                    v.StockQuantity != 0 ||
                    v.AttributeValueIds.Count > 0)
                .ToList();

            if (result.Count == 0)
                throw new InvalidOperationException("Sản phẩm phải có ít nhất một biến thể.");

            foreach (var variant in result)
            {
                variant.SKU = variant.SKU.Trim();
                variant.AttributeValueIds = variant.AttributeValueIds.Distinct().OrderBy(id => id).ToList();
            }

            return result;
        }

        private async Task<List<SelectionInfo>> ValidateProductSelectionsAsync(
            List<ProductAttributeInputViewModel>? inputs,
            int categoryId)
        {
            inputs ??= new List<ProductAttributeInputViewModel>();

            var rules = await GetCategoryRulesAsync(categoryId);
            var productRules = rules
                .Where(r => r.UseForProduct)
                .ToDictionary(r => r.DefinitionId);

            var selectedByDefinition = inputs
                .GroupBy(i => i.DefinitionId)
                .ToDictionary(
                    g => g.Key,
                    g => g.SelectMany(x => x.SelectedValueIds).Distinct().ToList());

            foreach (var selected in selectedByDefinition.Where(x => x.Value.Count > 0))
            {
                if (!productRules.ContainsKey(selected.Key))
                    throw new InvalidOperationException("Có thuộc tính thông tin chung không thuộc danh mục đã chọn.");
            }

            foreach (var requiredRule in productRules.Values.Where(r => r.IsRequired))
            {
                if (!selectedByDefinition.TryGetValue(requiredRule.DefinitionId, out var selectedIds) || selectedIds.Count == 0)
                    throw new InvalidOperationException($"Thuộc tính {requiredRule.DefinitionName} là bắt buộc.");
            }

            var result = new List<SelectionInfo>();

            foreach (var rule in productRules.Values)
            {
                if (!selectedByDefinition.TryGetValue(rule.DefinitionId, out var selectedIds) || selectedIds.Count == 0)
                    continue;

                var definition = await _unitOfWork.ProductAttributeDefinitions.GetByIdAsync(rule.DefinitionId);

                if (definition == null || !definition.IsActive || !definition.CanUseForProduct)
                    throw new InvalidOperationException($"Thuộc tính {rule.DefinitionName} không hợp lệ.");

                if (!definition.AllowMultipleProductValues && selectedIds.Count > 1)
                    throw new InvalidOperationException($"Thuộc tính {definition.Name} chỉ được chọn một giá trị.");

                foreach (var valueId in selectedIds)
                {
                    var info = await GetAttributeValueInfoAsync(valueId);

                    if (info.DefinitionId != definition.Id || !info.IsActive)
                        throw new InvalidOperationException($"Giá trị của thuộc tính {definition.Name} không hợp lệ.");

                    result.Add(new SelectionInfo
                    {
                        DefinitionId = definition.Id,
                        ValueId = info.ValueId
                    });
                }
            }

            return result;
        }

        private async Task PrepareAndValidateVariantsAsync(
            List<ProductVariantViewModel> variants,
            int? productId,
            int categoryId)
        {
            var rules = await GetCategoryRulesAsync(categoryId);
            var variantRules = rules
                .Where(r => r.UseForVariant)
                .ToDictionary(r => r.DefinitionId);

            var combinationKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var variant in variants)
            {
                if (string.IsNullOrWhiteSpace(variant.SKU))
                    throw new InvalidOperationException("SKU biến thể không được để trống.");

                if (variant.Price <= 0)
                    throw new InvalidOperationException("Giá biến thể phải lớn hơn 0.");

                if (variant.StockQuantity < 0)
                    throw new InvalidOperationException("Tồn kho biến thể không được âm.");

                var infos = new List<AttributeValueInfo>();

                foreach (var valueId in variant.AttributeValueIds.Distinct())
                {
                    var info = await GetAttributeValueInfoAsync(valueId);

                    if (!info.IsActive || !variantRules.ContainsKey(info.DefinitionId))
                        throw new InvalidOperationException("Có giá trị thuộc tính biến thể không thuộc danh mục đã chọn.");

                    infos.Add(info);
                }

                var duplicatedDefinition = infos
                    .GroupBy(i => i.DefinitionId)
                    .FirstOrDefault(g => g.Count() > 1);

                if (duplicatedDefinition != null)
                    throw new InvalidOperationException($"Một biến thể không thể có nhiều giá trị cho thuộc tính {duplicatedDefinition.First().DefinitionName}.");

                foreach (var requiredRule in variantRules.Values.Where(r => r.IsRequired))
                {
                    if (!infos.Any(i => i.DefinitionId == requiredRule.DefinitionId))
                        throw new InvalidOperationException($"Mỗi biến thể phải có giá trị cho thuộc tính {requiredRule.DefinitionName}.");
                }

                infos = infos
                    .OrderBy(i => variantRules[i.DefinitionId].DisplayOrder)
                    .ThenBy(i => i.DefinitionName)
                    .ToList();

                variant.Name = infos.Count == 0
                    ? "Mặc định"
                    : string.Join(" / ", infos.Select(i => i.Value));

                string combinationKey = infos.Count == 0
                    ? "default"
                    : string.Join("|", infos.Select(i => $"{i.DefinitionId}:{i.ValueId}"));

                if (!combinationKeys.Add(combinationKey))
                    throw new InvalidOperationException($"Biến thể {variant.Name} đang bị trùng.");

                bool variantSkuExists = await _unitOfWork.ProductVariants
                    .BuildQuery(v => v.SKU == variant.SKU && v.Id != variant.Id)
                    .AnyAsync();

                if (variantSkuExists)
                    throw new InvalidOperationException($"SKU {variant.SKU} đã tồn tại.");
            }
        }

        private async Task<List<CategoryAttributeRule>> GetCategoryRulesAsync(int categoryId)
        {
            return await _unitOfWork.CategoryAttributeDefinitions
                .BuildQuery(x => x.CategoryId == categoryId && x.AttributeDefinition.IsActive)
                .Select(x => new CategoryAttributeRule
                {
                    DefinitionId = x.AttributeDefinitionId,
                    DefinitionName = x.AttributeDefinition.Name,
                    UseForProduct = x.UseForProduct,
                    UseForVariant = x.UseForVariant,
                    IsRequired = x.IsRequired,
                    DisplayOrder = x.DisplayOrder
                })
                .ToListAsync();
        }

        private async Task<AttributeValueInfo> GetAttributeValueInfoAsync(int valueId)
        {
            var info = await _unitOfWork.ProductAttributeValues
                .BuildQuery(v => v.Id == valueId)
                .Select(v => new AttributeValueInfo
                {
                    ValueId = v.Id,
                    Value = v.Value,
                    IsActive = v.IsActive,
                    DefinitionId = v.AttributeDefinitionId,
                    DefinitionName = v.AttributeDefinition.Name,
                    DefinitionDisplayOrder = v.AttributeDefinition.DisplayOrder,
                    CanUseForVariant = v.AttributeDefinition.CanUseForVariant
                })
                .FirstOrDefaultAsync();

            if (info == null)
                throw new InvalidOperationException("Giá trị thuộc tính không tồn tại.");

            return info;
        }
        private sealed class CategoryAttributeRule
        {
            public int DefinitionId { get; set; }
            public string DefinitionName { get; set; } = string.Empty;
            public bool UseForProduct { get; set; }
            public bool UseForVariant { get; set; }
            public bool IsRequired { get; set; }
            public int DisplayOrder { get; set; }
        }

        private sealed class SelectionInfo
        {
            public int DefinitionId { get; set; }
            public int ValueId { get; set; }
        }

        private sealed class AttributeValueInfo
        {
            public int ValueId { get; set; }
            public string Value { get; set; } = string.Empty;
            public bool IsActive { get; set; }
            public int DefinitionId { get; set; }
            public string DefinitionName { get; set; } = string.Empty;
            public int DefinitionDisplayOrder { get; set; }
            public bool CanUseForVariant { get; set; }
        }
    }
}
