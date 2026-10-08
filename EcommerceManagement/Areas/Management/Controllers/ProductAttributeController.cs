using EcommerceManagement.Core.ViewModels;
using EcommerceManagement.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommerceManagement.Areas.Management.Controllers
{
    [Area("Management")]
    [Authorize(Roles = "Admin,Manager")]
    public class ProductAttributeController : Controller
    {
        private readonly IProductAttributeService _productAttributeService;

        public ProductAttributeController(IProductAttributeService productAttributeService)
        {
            _productAttributeService = productAttributeService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _productAttributeService.GetDefinitionsAsync();
            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new ProductAttributeDefinitionViewModel
            {
                IsActive = true,
                CanUseForProduct = true,
                IsFilterable = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductAttributeDefinitionViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _productAttributeService.CreateDefinitionAsync(model, GetCurrentUserId(), GetIpAddress());
                TempData["Success"] = "Thêm thuộc tính thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _productAttributeService.GetDefinitionByIdAsync(id);

            if (model == null)
                return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductAttributeDefinitionViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _productAttributeService.UpdateDefinitionAsync(model, GetCurrentUserId(), GetIpAddress());
                TempData["Success"] = "Cập nhật thuộc tính thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _productAttributeService.DeleteDefinitionAsync(id, GetCurrentUserId(), GetIpAddress());
                TempData["Success"] = "Xóa thuộc tính thành công.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Values(int id)
        {
            var model = await _productAttributeService.GetValuesPageAsync(id);

            if (model == null)
                return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateValue(ProductAttributeValueViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    throw new InvalidOperationException("Vui lòng nhập giá trị hợp lệ.");

                await _productAttributeService.CreateValueAsync(model, GetCurrentUserId(), GetIpAddress());
                TempData["Success"] = "Thêm giá trị thuộc tính thành công.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Values), new { id = model.AttributeDefinitionId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateValue(ProductAttributeValueViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    throw new InvalidOperationException("Vui lòng nhập giá trị hợp lệ.");

                await _productAttributeService.UpdateValueAsync(model, GetCurrentUserId(), GetIpAddress());
                TempData["Success"] = "Cập nhật giá trị thuộc tính thành công.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Values), new { id = model.AttributeDefinitionId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteValue(int id, int attributeDefinitionId)
        {
            try
            {
                await _productAttributeService.DeleteValueAsync(id, GetCurrentUserId(), GetIpAddress());
                TempData["Success"] = "Xóa giá trị thuộc tính thành công.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Values), new { id = attributeDefinitionId });
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
