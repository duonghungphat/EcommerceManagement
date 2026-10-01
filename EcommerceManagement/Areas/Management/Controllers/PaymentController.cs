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
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public async Task<IActionResult> Index(PaymentStatus? status, DateTime? fromDate, DateTime? toDate)
        {
            var model = await _paymentService.SearchAsync(status, fromDate, toDate);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int orderId)
        {
            var model = await _paymentService.GetCreateModelAsync(orderId);

            if (model == null)
                return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

                await _paymentService.CreateAsync(model, userId, ip);

                TempData["Success"] = "Ghi nhận thanh toán thành công.";

                return RedirectToAction("Details", "Order",
                    new
                    {
                        area = "Management",
                        id = model.OrderId
                    });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return View(model);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refund(int id, int orderId)
        {
            try
            {
                int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

                await _paymentService.RefundAsync(id, userId, ip);

                TempData["Success"] = "Hoàn tiền thành công.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Details", "Order",
                new
                {
                    area = "Management",
                    id = orderId
                });
        }
    }
}