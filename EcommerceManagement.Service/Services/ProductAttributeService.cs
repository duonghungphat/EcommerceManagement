using EcommerceManagement.Core.Models;
using EcommerceManagement.Core.ViewModels;
using EcommerceManagement.Data.UnitOfWork;
using EcommerceManagement.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceManagement.Service.Services
{
    public class ProductAttributeService : IProductAttributeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;

        public ProductAttributeService(IUnitOfWork unitOfWork, IAuditLogService auditLogService)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
        }

        public async Task<List<ProductAttributeDefinitionViewModel>> GetDefinitionsAsync()
        {
            return await _unitOfWork.ProductAttributeDefinitions
                .BuildQuery(a => true)
                .OrderBy(a => a.DisplayOrder)
                .ThenBy(a => a.Name)
                .Select(a => new ProductAttributeDefinitionViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Code = a.Code,
                    CanUseForVariant = a.CanUseForVariant,
                    CanUseForProduct = a.CanUseForProduct && !a.CanUseForVariant,
                    IsFilterable = a.IsFilterable,
                    AllowMultipleProductValues = !a.CanUseForVariant && a.AllowMultipleProductValues,
                    DisplayOrder = a.DisplayOrder,
                    IsActive = a.IsActive,
                    ValueCount = a.Values.Count()
                })
                .ToListAsync();
        }

        public async Task<ProductAttributeDefinitionViewModel?> GetDefinitionByIdAsync(int id)
        {
            return await _unitOfWork.ProductAttributeDefinitions
                .BuildQuery(a => a.Id == id)
                .Select(a => new ProductAttributeDefinitionViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Code = a.Code,
                    CanUseForVariant = a.CanUseForVariant,
                    CanUseForProduct = a.CanUseForProduct && !a.CanUseForVariant,
                    IsFilterable = a.IsFilterable,
                    AllowMultipleProductValues = !a.CanUseForVariant && a.AllowMultipleProductValues,
                    DisplayOrder = a.DisplayOrder,
                    IsActive = a.IsActive,
                    ValueCount = a.Values.Count()
                })
                .FirstOrDefaultAsync();
        }

        public async Task CreateDefinitionAsync(ProductAttributeDefinitionViewModel model, int actorUserId, string ipAddress)
        {
            string name = model.Name.Trim();
            string code = model.Code.Trim().ToLowerInvariant();

            if (!model.CanUseForProduct && !model.CanUseForVariant)
                throw new InvalidOperationException("Thuộc tính phải được sử dụng ở cấp sản phẩm hoặc cấp biến thể.");

            bool codeExists = await _unitOfWork.ProductAttributeDefinitions
                .BuildQuery(a => a.Code == code)
                .AnyAsync();

            if (codeExists)
                throw new InvalidOperationException("Mã thuộc tính đã tồn tại.");

            bool nameExists = await _unitOfWork.ProductAttributeDefinitions
                .BuildQuery(a => a.Name == name)
                .AnyAsync();

            if (nameExists)
                throw new InvalidOperationException("Tên thuộc tính đã tồn tại.");

            // Một thuộc tính dùng để tạo SKU/biến thể thì không hiển thị thêm ở cấp Product.
            // IsFilterable vẫn cho phép dùng chính thuộc tính biến thể làm bộ lọc.
            if (model.CanUseForVariant)
            {
                model.CanUseForProduct = false;
                model.AllowMultipleProductValues = false;
            }
            else if (!model.CanUseForProduct)
            {
                model.AllowMultipleProductValues = false;
            }

            var definition = new ProductAttributeDefinition
            {
                Name = name,
                Code = code,
                CanUseForVariant = model.CanUseForVariant,
                CanUseForProduct = model.CanUseForProduct,
                IsFilterable = model.IsFilterable,
                AllowMultipleProductValues = model.AllowMultipleProductValues,
                DisplayOrder = model.DisplayOrder,
                IsActive = model.IsActive
            };

            await _unitOfWork.ProductAttributeDefinitions.AddAsync(definition);

            await _auditLogService.RecordAsync(
                "CreateProductAttribute",
                "ProductAttributeDefinition",
                null,
                $"Thêm thuộc tính sản phẩm: {definition.Name}.",
                ipAddress,
                actorUserId);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateDefinitionAsync(ProductAttributeDefinitionViewModel model, int actorUserId, string ipAddress)
        {
            var definition = await _unitOfWork.ProductAttributeDefinitions.GetByIdAsync(model.Id);

            if (definition == null)
                throw new InvalidOperationException("Thuộc tính không tồn tại.");

            string name = model.Name.Trim();
            string code = model.Code.Trim().ToLowerInvariant();

            if (!model.CanUseForProduct && !model.CanUseForVariant)
                throw new InvalidOperationException("Thuộc tính phải được sử dụng ở cấp sản phẩm hoặc cấp biến thể.");

            bool codeExists = await _unitOfWork.ProductAttributeDefinitions
                .BuildQuery(a => a.Code == code && a.Id != model.Id)
                .AnyAsync();

            if (codeExists)
                throw new InvalidOperationException("Mã thuộc tính đã tồn tại.");

            bool nameExists = await _unitOfWork.ProductAttributeDefinitions
                .BuildQuery(a => a.Name == name && a.Id != model.Id)
                .AnyAsync();

            if (nameExists)
                throw new InvalidOperationException("Tên thuộc tính đã tồn tại.");

            // Một thuộc tính dùng để tạo SKU/biến thể thì không hiển thị thêm ở cấp Product.
            // IsFilterable vẫn cho phép dùng chính thuộc tính biến thể làm bộ lọc.
            if (model.CanUseForVariant)
            {
                model.CanUseForProduct = false;
                model.AllowMultipleProductValues = false;
            }
            else if (!model.CanUseForProduct)
            {
                model.AllowMultipleProductValues = false;
            }

            definition.Name = name;
            definition.Code = code;
            definition.CanUseForVariant = model.CanUseForVariant;
            definition.CanUseForProduct = model.CanUseForProduct;
            definition.IsFilterable = model.IsFilterable;
            definition.AllowMultipleProductValues = model.AllowMultipleProductValues;
            definition.DisplayOrder = model.DisplayOrder;
            definition.IsActive = model.IsActive;

            await _auditLogService.RecordAsync(
                "UpdateProductAttribute",
                "ProductAttributeDefinition",
                definition.Id,
                $"Cập nhật thuộc tính sản phẩm: {definition.Name}.",
                ipAddress,
                actorUserId);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteDefinitionAsync(int id, int actorUserId, string ipAddress)
        {
            var definition = await _unitOfWork.ProductAttributeDefinitions.GetByIdAsync(id);

            if (definition == null)
                throw new InvalidOperationException("Thuộc tính không tồn tại.");

            bool hasProductSelections = await _unitOfWork.ProductAttributeSelections
                .BuildQuery(s => s.AttributeDefinitionId == id)
                .AnyAsync();

            bool hasVariantSelections = await _unitOfWork.ProductVariantAttributeSelections
                .BuildQuery(s => s.AttributeDefinitionId == id)
                .AnyAsync();

            bool assignedToCategory = await _unitOfWork.CategoryAttributeDefinitions
                .BuildQuery(x => x.AttributeDefinitionId == id)
                .AnyAsync();

            if (hasProductSelections || hasVariantSelections || assignedToCategory)
                throw new InvalidOperationException("Thuộc tính đang được sử dụng hoặc đã gán cho danh mục nên không thể xóa.");

            var values = await _unitOfWork.ProductAttributeValues
                .BuildQuery(v => v.AttributeDefinitionId == id)
                .ToListAsync();

            foreach (var value in values)
                _unitOfWork.ProductAttributeValues.Delete(value);

            _unitOfWork.ProductAttributeDefinitions.Delete(definition);

            await _auditLogService.RecordAsync(
                "DeleteProductAttribute",
                "ProductAttributeDefinition",
                definition.Id,
                $"Xóa thuộc tính sản phẩm: {definition.Name}.",
                ipAddress,
                actorUserId);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ProductAttributeValuesPageViewModel?> GetValuesPageAsync(int definitionId)
        {
            var definition = await _unitOfWork.ProductAttributeDefinitions.GetByIdAsync(definitionId);

            if (definition == null)
                return null;

            var values = await _unitOfWork.ProductAttributeValues
                .BuildQuery(v => v.AttributeDefinitionId == definitionId)
                .OrderBy(v => v.DisplayOrder)
                .ThenBy(v => v.Value)
                .Select(v => new ProductAttributeValueViewModel
                {
                    Id = v.Id,
                    AttributeDefinitionId = v.AttributeDefinitionId,
                    AttributeName = definition.Name,
                    Value = v.Value,
                    DisplayOrder = v.DisplayOrder,
                    IsActive = v.IsActive
                })
                .ToListAsync();

            return new ProductAttributeValuesPageViewModel
            {
                AttributeDefinitionId = definition.Id,
                AttributeName = definition.Name,
                NewValue = new ProductAttributeValueViewModel
                {
                    AttributeDefinitionId = definition.Id,
                    AttributeName = definition.Name,
                    IsActive = true
                },
                Values = values
            };
        }

        public async Task CreateValueAsync(ProductAttributeValueViewModel model, int actorUserId, string ipAddress)
        {
            var definition = await _unitOfWork.ProductAttributeDefinitions.GetByIdAsync(model.AttributeDefinitionId);

            if (definition == null)
                throw new InvalidOperationException("Thuộc tính không tồn tại.");

            string valueText = model.Value.Trim();

            bool valueExists = await _unitOfWork.ProductAttributeValues
                .BuildQuery(v => v.AttributeDefinitionId == model.AttributeDefinitionId && v.Value == valueText)
                .AnyAsync();

            if (valueExists)
                throw new InvalidOperationException("Giá trị này đã tồn tại.");

            var value = new ProductAttributeValue
            {
                AttributeDefinitionId = model.AttributeDefinitionId,
                Value = valueText,
                DisplayOrder = model.DisplayOrder,
                IsActive = model.IsActive
            };

            await _unitOfWork.ProductAttributeValues.AddAsync(value);

            await _auditLogService.RecordAsync(
                "CreateProductAttributeValue",
                "ProductAttributeValue",
                null,
                $"Thêm giá trị {value.Value} cho thuộc tính {definition.Name}.",
                ipAddress,
                actorUserId);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateValueAsync(ProductAttributeValueViewModel model, int actorUserId, string ipAddress)
        {
            var value = await _unitOfWork.ProductAttributeValues.GetByIdAsync(model.Id);

            if (value == null)
                throw new InvalidOperationException("Giá trị thuộc tính không tồn tại.");

            string valueText = model.Value.Trim();

            bool valueExists = await _unitOfWork.ProductAttributeValues
                .BuildQuery(v => v.AttributeDefinitionId == value.AttributeDefinitionId && v.Value == valueText && v.Id != model.Id)
                .AnyAsync();

            if (valueExists)
                throw new InvalidOperationException("Giá trị này đã tồn tại.");

            value.Value = valueText;
            value.DisplayOrder = model.DisplayOrder;
            value.IsActive = model.IsActive;

            await _auditLogService.RecordAsync(
                "UpdateProductAttributeValue",
                "ProductAttributeValue",
                value.Id,
                $"Cập nhật giá trị thuộc tính: {value.Value}.",
                ipAddress,
                actorUserId);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteValueAsync(int id, int actorUserId, string ipAddress)
        {
            var value = await _unitOfWork.ProductAttributeValues.GetByIdAsync(id);

            if (value == null)
                throw new InvalidOperationException("Giá trị thuộc tính không tồn tại.");

            bool usedByProduct = await _unitOfWork.ProductAttributeSelections
                .BuildQuery(s => s.AttributeValueId == id)
                .AnyAsync();

            bool usedByVariant = await _unitOfWork.ProductVariantAttributeSelections
                .BuildQuery(s => s.AttributeValueId == id)
                .AnyAsync();

            if (usedByProduct || usedByVariant)
                throw new InvalidOperationException("Giá trị đang được sử dụng nên không thể xóa.");

            _unitOfWork.ProductAttributeValues.Delete(value);

            await _auditLogService.RecordAsync(
                "DeleteProductAttributeValue",
                "ProductAttributeValue",
                value.Id,
                $"Xóa giá trị thuộc tính: {value.Value}.",
                ipAddress,
                actorUserId);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<ProductAttributeInputViewModel>> GetInputsAsync(bool forVariant)
        {
            return await _unitOfWork.ProductAttributeDefinitions
                .BuildQuery(a => a.IsActive &&
                    (forVariant
                        ? a.CanUseForVariant
                        : a.CanUseForProduct && !a.CanUseForVariant))
                .OrderBy(a => a.DisplayOrder)
                .ThenBy(a => a.Name)
                .Select(a => new ProductAttributeInputViewModel
                {
                    DefinitionId = a.Id,
                    Name = a.Name,
                    Code = a.Code,
                    AllowMultipleValues = !forVariant && a.AllowMultipleProductValues,
                    Values = a.Values
                        .Where(v => v.IsActive)
                        .OrderBy(v => v.DisplayOrder)
                        .ThenBy(v => v.Value)
                        .Select(v => new ProductAttributeValueOptionViewModel
                        {
                            Id = v.Id,
                            Value = v.Value
                        })
                        .ToList()
                })
                .ToListAsync();
        }

        public async Task<List<ProductAttributeInputViewModel>> GetInputsForCategoryAsync(int categoryId, bool forVariant)
        {
            bool categoryExists = await _unitOfWork.Categories
                .BuildQuery(c => c.Id == categoryId)
                .AnyAsync();

            if (!categoryExists)
                return new List<ProductAttributeInputViewModel>();

            return await _unitOfWork.CategoryAttributeDefinitions
                .BuildQuery(x =>
                    x.CategoryId == categoryId &&
                    x.AttributeDefinition.IsActive &&
                    (forVariant ? x.UseForVariant : x.UseForProduct))
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.AttributeDefinition.Name)
                .Select(x => new ProductAttributeInputViewModel
                {
                    DefinitionId = x.AttributeDefinitionId,
                    Name = x.AttributeDefinition.Name,
                    Code = x.AttributeDefinition.Code,
                    AllowMultipleValues = !forVariant && x.AttributeDefinition.AllowMultipleProductValues,
                    IsRequired = x.IsRequired,
                    Values = x.AttributeDefinition.Values
                        .Where(v => v.IsActive)
                        .OrderBy(v => v.DisplayOrder)
                        .ThenBy(v => v.Value)
                        .Select(v => new ProductAttributeValueOptionViewModel
                        {
                            Id = v.Id,
                            Value = v.Value
                        })
                        .ToList()
                })
                .ToListAsync();
        }

        public async Task<CategoryAttributeConfigViewModel?> GetCategoryConfigAsync(int categoryId)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);

            if (category == null)
                return null;

            var mappings = await _unitOfWork.CategoryAttributeDefinitions
                .BuildQuery(x => x.CategoryId == categoryId)
                .ToListAsync();

            var mappingByDefinition = mappings.ToDictionary(x => x.AttributeDefinitionId);

            var definitions = await _unitOfWork.ProductAttributeDefinitions
                .BuildQuery(a => a.IsActive)
                .OrderBy(a => a.DisplayOrder)
                .ThenBy(a => a.Name)
                .ToListAsync();

            var model = new CategoryAttributeConfigViewModel
            {
                CategoryId = category.Id,
                CategoryName = category.Name
            };

            foreach (var definition in definitions)
            {
                mappingByDefinition.TryGetValue(definition.Id, out var mapping);

                model.Attributes.Add(new CategoryAttributeMappingViewModel
                {
                    DefinitionId = definition.Id,
                    DefinitionName = definition.Name,
                    DefinitionCode = definition.Code,
                    CanUseForProduct = definition.CanUseForProduct,
                    CanUseForVariant = definition.CanUseForVariant,
                    UseForProduct = mapping?.UseForProduct ?? false,
                    UseForVariant = mapping?.UseForVariant ?? false,
                    IsRequired = mapping?.IsRequired ?? false,
                    DisplayOrder = mapping?.DisplayOrder ?? definition.DisplayOrder
                });
            }

            return model;
        }

        public async Task SaveCategoryConfigAsync(CategoryAttributeConfigViewModel model, int actorUserId, string ipAddress)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(model.CategoryId);

            if (category == null)
                throw new InvalidOperationException("Danh mục không tồn tại.");

            var selectedItems = model.Attributes
                .Where(x => x.UseForProduct || x.UseForVariant)
                .ToList();

            var validatedItems = new List<(CategoryAttributeMappingViewModel Item, ProductAttributeDefinition Definition)>();

            foreach (var item in selectedItems)
            {
                if (item.UseForProduct && item.UseForVariant)
                    throw new InvalidOperationException($"Thuộc tính {item.DefinitionName} chỉ nên dùng ở một vai trò: thông tin chung hoặc tạo biến thể.");

                var definition = await _unitOfWork.ProductAttributeDefinitions.GetByIdAsync(item.DefinitionId);

                if (definition == null || !definition.IsActive)
                    throw new InvalidOperationException("Có thuộc tính không tồn tại hoặc đã ngừng hoạt động.");

                if (item.UseForProduct && !definition.CanUseForProduct)
                    throw new InvalidOperationException($"Thuộc tính {definition.Name} không được cấu hình để dùng cho Product.");

                if (item.UseForVariant && !definition.CanUseForVariant)
                    throw new InvalidOperationException($"Thuộc tính {definition.Name} không được cấu hình để dùng cho Variant.");

                validatedItems.Add((item, definition));
            }

            var currentMappings = await _unitOfWork.CategoryAttributeDefinitions
                .BuildQuery(x => x.CategoryId == model.CategoryId)
                .ToListAsync();

            foreach (var current in currentMappings)
                _unitOfWork.CategoryAttributeDefinitions.Delete(current);

            foreach (var validated in validatedItems)
            {
                await _unitOfWork.CategoryAttributeDefinitions.AddAsync(new CategoryAttributeDefinition
                {
                    CategoryId = model.CategoryId,
                    AttributeDefinitionId = validated.Definition.Id,
                    UseForProduct = validated.Item.UseForProduct,
                    UseForVariant = validated.Item.UseForVariant,
                    IsRequired = validated.Item.IsRequired,
                    DisplayOrder = Math.Max(validated.Item.DisplayOrder, 0)
                });
            }

            await _auditLogService.RecordAsync(
                "UpdateCategoryAttributes",
                "Category",
                category.Id,
                $"Cập nhật bộ thuộc tính cho danh mục: {category.Name}.",
                ipAddress,
                actorUserId);

            await _unitOfWork.SaveChangesAsync();
        }

    }
}
