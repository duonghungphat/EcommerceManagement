using EcommerceManagement.Core.ViewModels;
using EcommerceManagement.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommerceManagement.Areas.Management.Controllers
{
    [Area("Management")]
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IWebHostEnvironment _environment;

        public ProductController(
            IProductService productService,
            ICategoryService categoryService,
            IProductAttributeService productAttributeService,
            IWebHostEnvironment environment)
        {
            _productService = productService;
            _categoryService = categoryService;
            _productAttributeService = productAttributeService;
            _environment = environment;
        }

        public async Task<IActionResult> Index(string? searchTerm, int? categoryId, int page = 1)
        {
            const int pageSize = 10;

            var model = await _productService.GetPagedAsync(searchTerm, categoryId, page, pageSize);
            model.Categories = await _categoryService.GetAllAsync();

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create()
        {
            var model = new ProductViewModel();
            await PopulateOptionsAsync(model);

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model, IFormFile? imageFile)
        {
            ValidateImageIfPresent(imageFile);
            ValidateVariantImages(model);

            if (!ModelState.IsValid)
            {
                await PopulateOptionsAsync(model);
                return View(model);
            }

            var uploadedPaths = new List<string>();

            try
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    model.ImagePath = await SaveImageAsync(imageFile);
                    uploadedPaths.Add(model.ImagePath);
                }

                for (int i = 0; i < model.Variants.Count; i++)
                {
                    var variantImage = Request.Form.Files.GetFile($"variantImages[{i}]");

                    if (variantImage == null || variantImage.Length == 0)
                        continue;

                    model.Variants[i].ImagePath = await SaveImageAsync(variantImage);
                    uploadedPaths.Add(model.Variants[i].ImagePath!);
                }

                await _productService.CreateAsync(model, GetCurrentUserId(), GetIpAddress());

                TempData["Success"] = "Thêm sản phẩm thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                foreach (var path in uploadedPaths)
                    DeleteImage(path);

                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateOptionsAsync(model);

                return View(model);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _productService.GetByIdAsync(id);

            if (model == null)
                return NotFound();

            await PopulateOptionsAsync(model);

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductViewModel model, IFormFile? imageFile)
        {
            var existingProduct = await _productService.GetByIdAsync(model.Id);

            if (existingProduct == null)
                return NotFound();

            string? oldProductImagePath = existingProduct.ImagePath;
            model.ImagePath = oldProductImagePath;

            var oldVariantImages = existingProduct.Variants
                .Where(v => v.Id > 0)
                .ToDictionary(v => v.Id, v => v.ImagePath);

            foreach (var variant in model.Variants)
            {
                if (variant.Id > 0 && oldVariantImages.TryGetValue(variant.Id, out var oldPath))
                    variant.ImagePath = oldPath;
            }

            ValidateImageIfPresent(imageFile);
            ValidateVariantImages(model);

            if (!ModelState.IsValid)
            {
                await PopulateOptionsAsync(model);
                return View(model);
            }

            var uploadedPaths = new List<string>();
            var replacedOldPaths = new List<string>();

            try
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    model.ImagePath = await SaveImageAsync(imageFile);
                    uploadedPaths.Add(model.ImagePath);

                    if (!string.IsNullOrWhiteSpace(oldProductImagePath))
                        replacedOldPaths.Add(oldProductImagePath);
                }

                for (int i = 0; i < model.Variants.Count; i++)
                {
                    var variantImage = Request.Form.Files.GetFile($"variantImages[{i}]");

                    if (variantImage == null || variantImage.Length == 0)
                        continue;

                    string newPath = await SaveImageAsync(variantImage);
                    uploadedPaths.Add(newPath);

                    if (model.Variants[i].Id > 0 &&
                        oldVariantImages.TryGetValue(model.Variants[i].Id, out var oldVariantPath) &&
                        !string.IsNullOrWhiteSpace(oldVariantPath))
                    {
                        replacedOldPaths.Add(oldVariantPath);
                    }

                    model.Variants[i].ImagePath = newPath;
                }

                var postedVariantIds = model.Variants
                    .Where(v => v.Id > 0)
                    .Select(v => v.Id)
                    .ToHashSet();

                var removedVariantImagePaths = existingProduct.Variants
                    .Where(v => v.Id > 0 && !postedVariantIds.Contains(v.Id) && !string.IsNullOrWhiteSpace(v.ImagePath))
                    .Select(v => v.ImagePath!)
                    .ToList();

                await _productService.UpdateAsync(model, GetCurrentUserId(), GetIpAddress());

                foreach (var path in replacedOldPaths.Concat(removedVariantImagePaths).Distinct())
                    DeleteImage(path);

                TempData["Success"] = "Cập nhật sản phẩm thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                foreach (var path in uploadedPaths)
                    DeleteImage(path);

                model.ImagePath = oldProductImagePath;

                foreach (var variant in model.Variants)
                {
                    if (variant.Id > 0 && oldVariantImages.TryGetValue(variant.Id, out var oldPath))
                        variant.ImagePath = oldPath;
                }

                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateOptionsAsync(model);

                return View(model);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id, string? searchTerm, int? categoryId, int page = 1)
        {
            try
            {
                await _productService.ToggleStatusAsync(id, GetCurrentUserId(), GetIpAddress());
                TempData["Success"] = "Đổi trạng thái sản phẩm thành công.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index), new { searchTerm, categoryId, page });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);

                if (product == null)
                    return NotFound();

                var imagePaths = new List<string>();

                if (!string.IsNullOrWhiteSpace(product.ImagePath))
                    imagePaths.Add(product.ImagePath);

                imagePaths.AddRange(product.Variants
                    .Where(v => !string.IsNullOrWhiteSpace(v.ImagePath))
                    .Select(v => v.ImagePath!));

                await _productService.DeleteAsync(id, GetCurrentUserId(), GetIpAddress());

                foreach (var path in imagePaths.Distinct())
                    DeleteImage(path);

                TempData["Success"] = "Xóa sản phẩm thành công.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetCategoryAttributes(int categoryId)
        {
            if (categoryId <= 0)
            {
                return Json(new
                {
                    productAttributes = Array.Empty<object>(),
                    variantAttributes = Array.Empty<object>()
                });
            }

            var productAttributes = await _productAttributeService.GetInputsForCategoryAsync(categoryId, false);
            var variantAttributes = await _productAttributeService.GetInputsForCategoryAsync(categoryId, true);

            return Json(new
            {
                productAttributes = productAttributes.Select(a => new
                {
                    definitionId = a.DefinitionId,
                    name = a.Name,
                    code = a.Code,
                    allowMultipleValues = a.AllowMultipleValues,
                    isRequired = a.IsRequired,
                    values = a.Values.Select(v => new
                    {
                        id = v.Id,
                        value = v.Value
                    })
                }),
                variantAttributes = variantAttributes.Select(a => new
                {
                    definitionId = a.DefinitionId,
                    name = a.Name,
                    code = a.Code,
                    isRequired = a.IsRequired,
                    values = a.Values.Select(v => new
                    {
                        id = v.Id,
                        value = v.Value
                    })
                })
            });
        }

        private async Task PopulateOptionsAsync(ProductViewModel model)
        {
            model.Categories = await _categoryService.GetAllAsync();

            var postedProductSelections = model.ProductAttributes
                .GroupBy(x => x.DefinitionId)
                .ToDictionary(g => g.Key, g => g.SelectMany(x => x.SelectedValueIds).Distinct().ToList());

            var postedVariantSelections = model.VariantAttributes
                .GroupBy(x => x.DefinitionId)
                .ToDictionary(g => g.Key, g => g.SelectMany(x => x.SelectedValueIds).Distinct().ToList());

            if (model.CategoryId <= 0)
            {
                model.ProductAttributes = new List<ProductAttributeInputViewModel>();
                model.VariantAttributes = new List<ProductAttributeInputViewModel>();
                return;
            }

            var productAttributes = await _productAttributeService.GetInputsForCategoryAsync(model.CategoryId, false);
            var variantAttributes = await _productAttributeService.GetInputsForCategoryAsync(model.CategoryId, true);

            foreach (var input in productAttributes)
            {
                if (postedProductSelections.TryGetValue(input.DefinitionId, out var selected))
                    input.SelectedValueIds = selected;
            }

            foreach (var input in variantAttributes)
            {
                if (postedVariantSelections.TryGetValue(input.DefinitionId, out var selected))
                    input.SelectedValueIds = selected;
            }

            model.ProductAttributes = productAttributes;
            model.VariantAttributes = variantAttributes;
        }

        private void ValidateVariantImages(ProductViewModel model)
        {
            for (int i = 0; i < model.Variants.Count; i++)
            {
                var file = Request.Form.Files.GetFile($"variantImages[{i}]");

                if (file != null && file.Length > 0)
                    ValidateImage(file);
            }
        }

        private void ValidateImageIfPresent(IFormFile? imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
                ValidateImage(imageFile);
        }

        private void ValidateImage(IFormFile imageFile)
        {
            const long maxSize = 2 * 1024 * 1024;

            if (imageFile.Length > maxSize)
            {
                ModelState.AddModelError(string.Empty, "Ảnh không được lớn hơn 2MB.");
                return;
            }

            string extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };

            if (!allowedExtensions.Contains(extension))
                ModelState.AddModelError(string.Empty, "Chỉ chấp nhận ảnh JPG hoặc PNG.");
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            string uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "products");
            Directory.CreateDirectory(uploadFolder);

            string extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            string fileName = $"{Guid.NewGuid():N}{extension}";
            string fullPath = Path.Combine(uploadFolder, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await imageFile.CopyToAsync(stream);

            return $"/uploads/products/{fileName}";
        }

        private void DeleteImage(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return;

            string relativePath = imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            string fullPath = Path.Combine(_environment.WebRootPath, relativePath);

            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        private string GetIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Không xác định";
        }
    }
}
