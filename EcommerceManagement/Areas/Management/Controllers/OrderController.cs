using EcommerceManagement.Core.Enums;
using EcommerceManagement.Core.ViewModels;
using EcommerceManagement.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommerceManagement.Areas.Management.Controllers
{
    [Area("Management")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;

        public OrderController(IOrderService orderService, ICustomerService customerService, IProductService productService)
        {
            _orderService = orderService;
            _customerService = customerService;
            _productService = productService;
        }

        public async Task<IActionResult> Index(string? searchTerm, OrderStatus? status, DateTime? fromDate, DateTime? toDate)
        {
            var model = await _orderService.SearchAsync(searchTerm, status, fromDate, toDate);

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var model = await _orderService.GetDetailsAsync(id);

            if (model == null)
                return NotFound();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new OrderCreateViewModel();

            await PopulateOptionsAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateOptionsAsync(model);
                return View(model);
            }

            try
            {
                int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Không xác định";

                int orderId = await _orderService.CreateAsync(model, userId, ip);

                TempData["Success"] = "Tạo đơn hàng thành công.";

                return RedirectToAction(nameof(Details), new { id = orderId });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                await PopulateOptionsAsync(model);

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            try
            {
                int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Không xác định";

                await _orderService.UpdateStatusAsync(id, status, userId, ip);

                TempData["Success"] = "Cập nhật trạng thái thành công.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Không xác định";

                await _orderService.CancelAsync(id, userId, ip);

                TempData["Success"] = "Hủy đơn hàng thành công.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task PopulateOptionsAsync(OrderCreateViewModel model)
        {
            model.Customers = await _customerService.GetAllAsync();

            model.Products = (await _productService.GetAllAsync())
                .Where(p => p.Status == ProductStatus.Selling && p.StockQuantity > 0)
                .ToList();
        }
    }
}