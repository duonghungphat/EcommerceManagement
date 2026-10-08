using EcommerceManagement.Core.ViewModels;
using EcommerceManagement.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommerceManagement.Areas.Management.Controllers
{
    [Area("Management")]
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductAttributeService _productAttributeService;

        public CategoryController(ICategoryService categoryService, IProductAttributeService productAttributeService)
        {
            _categoryService = categoryService;
            _productAttributeService = productAttributeService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllAsync();

            return View(categories);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create()
        {
            var model = new CategoryViewModel();

            await PopulateParentCategoriesAsync(model);

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateParentCategoriesAsync(model);

                return View(model);
            }

            try
            {
                await _categoryService.CreateAsync(model, GetCurrentUserId(), GetIpAddress());

                TempData["Success"] = "Thêm danh mục thành công.";

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                await PopulateParentCategoriesAsync(model);

                return View(model);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _categoryService.GetByIdAsync(id);

            if (model == null)
                return NotFound();

            await PopulateParentCategoriesAsync(model, model.Id);

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateParentCategoriesAsync(model, model.Id);

                return View(model);
            }

            try
            {
                await _categoryService.UpdateAsync(model, GetCurrentUserId(), GetIpAddress());

                TempData["Success"] = "Cập nhật danh mục thành công.";

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                await PopulateParentCategoriesAsync(model, model.Id);

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
                await _categoryService.DeleteAsync(id, GetCurrentUserId(), GetIpAddress());

                TempData["Success"] = "Xóa danh mục thành công.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Attributes(int id)
        {
            var model = await _productAttributeService.GetCategoryConfigAsync(id);

            if (model == null)
                return NotFound();

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Attributes(CategoryAttributeConfigViewModel model)
        {
            try
            {
                await _productAttributeService.SaveCategoryConfigAsync(model, GetCurrentUserId(), GetIpAddress());
                TempData["Success"] = "Cập nhật bộ thuộc tính danh mục thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                var refreshed = await _productAttributeService.GetCategoryConfigAsync(model.CategoryId);

                if (refreshed == null)
                    return NotFound();

                var posted = model.Attributes.ToDictionary(x => x.DefinitionId);

                foreach (var item in refreshed.Attributes)
                {
                    if (!posted.TryGetValue(item.DefinitionId, out var current))
                        continue;

                    item.UseForProduct = current.UseForProduct;
                    item.UseForVariant = current.UseForVariant;
                    item.IsRequired = current.IsRequired;
                    item.DisplayOrder = current.DisplayOrder;
                }

                return View(refreshed);
            }
        }

        private async Task PopulateParentCategoriesAsync(CategoryViewModel model, int? excludeCategoryId = null)
        {
            var categories = await _categoryService.GetAllAsync();

            model.ParentCategories = categories
                .Where(c => c.ParentCategoryId == null && c.Id != excludeCategoryId)
                .OrderBy(c => c.Name)
                .ToList();
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