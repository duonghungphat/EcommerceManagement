using EcommerceManagement.Core.ViewModels;
using EcommerceManagement.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceManagement.Areas.Management.Controllers
{
    [Area("Management")]
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _environment;

        public ProductController(IProductService productService, ICategoryService categoryService, IWebHostEnvironment environment)
        {
            _productService = productService;
            _categoryService = categoryService;
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
            var model = new ProductViewModel
            {
                Categories = await _categoryService.GetAllAsync()
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model, IFormFile? imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                ValidateImage(imageFile);
            }

            if (!ModelState.IsValid)
            {
                model.Categories = await _categoryService.GetAllAsync();

                return View(model);
            }

            string? uploadedImagePath = null;

            try
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    uploadedImagePath = await SaveImageAsync(imageFile);

                    model.ImagePath = uploadedImagePath;
                }

                await _productService.CreateAsync(model);

                TempData["Success"] =
                    "Thêm sản phẩm thành công.";

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                if (uploadedImagePath != null)
                {
                    DeleteImage(uploadedImagePath);
                }

                ModelState.AddModelError(string.Empty, ex.Message);

                model.Categories = await _categoryService.GetAllAsync();

                return View(model);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _productService.GetByIdAsync(id);

            if (model == null)
            {
                return NotFound();
            }

            model.Categories = await _categoryService.GetAllAsync();

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductViewModel model, IFormFile? imageFile)
        {
            var existingProduct = await _productService.GetByIdAsync(model.Id);

            if (existingProduct == null)
            {
                return NotFound();
            }

            string? oldImagePath = existingProduct.ImagePath;

            model.ImagePath = oldImagePath;

            if (imageFile != null && imageFile.Length > 0)
            {
                ValidateImage(imageFile);
            }

            if (!ModelState.IsValid)
            {
                model.Categories = await _categoryService.GetAllAsync();

                return View(model);
            }

            string? newImagePath = null;

            try
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    newImagePath = await SaveImageAsync(imageFile);

                    model.ImagePath = newImagePath;
                }

                await _productService.UpdateAsync(model);

                if (newImagePath != null && oldImagePath != null)
                {
                    DeleteImage(oldImagePath);
                }

                TempData["Success"] = "Cập nhật sản phẩm thành công.";

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                if (newImagePath != null)
                {
                    DeleteImage(newImagePath);
                }

                model.ImagePath = oldImagePath;

                ModelState.AddModelError(string.Empty, ex.Message);

                model.Categories = await _categoryService.GetAllAsync();

                return View(model);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var product =
                    await _productService.GetByIdAsync(id);

                if (product == null)
                {
                    return NotFound();
                }

                string? imagePath = product.ImagePath;

                await _productService.DeleteAsync(id);

                if (imagePath != null)
                {
                    DeleteImage(imagePath);
                }

                TempData["Success"] = "Xóa sản phẩm thành công.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private void ValidateImage(IFormFile imageFile)
        {
            const long maxSize = 2 * 1024 * 1024;

            if (imageFile.Length > maxSize)
            {
                ModelState.AddModelError(string.Empty, "Ảnh không được lớn hơn 2MB.");

                return;
            }

            string extension = Path.GetExtension(imageFile.FileName)
                    .ToLowerInvariant();

            string[] allowedExtensions =
            {
                ".jpg",
                ".jpeg",
                ".png"
            };

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(string.Empty, "Chỉ chấp nhận ảnh JPG hoặc PNG.");
            }
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            string uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "products");

            Directory.CreateDirectory(uploadFolder);

            string extension = Path.GetExtension(imageFile.FileName)
                    .ToLowerInvariant();

            string fileName = $"{Guid.NewGuid():N}{extension}";

            string fullPath = Path.Combine(uploadFolder, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);

            await imageFile.CopyToAsync(stream);

            return $"/uploads/products/{fileName}";
        }

        private void DeleteImage(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return;
            }

            string relativePath = imagePath
                    .TrimStart('/')
                    .Replace('/', Path.DirectorySeparatorChar);

            string fullPath = Path.Combine(_environment.WebRootPath, relativePath);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
    }
}