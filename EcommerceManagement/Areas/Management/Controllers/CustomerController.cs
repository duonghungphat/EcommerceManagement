using EcommerceManagement.Core.ViewModels;
using EcommerceManagement.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceManagement.Areas.Management.Controllers
{
    [Area("Management")]
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;

        public CustomerController(
            ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _customerService.GetAllAsync();

            return View(customers);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CustomerViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CustomerViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _customerService.CreateAsync(model);

                TempData["Success"] = "Thêm khách hàng thành công.";

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _customerService.GetByIdAsync(id);

            if (model == null)
                return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            CustomerViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _customerService.UpdateAsync(model);

                TempData["Success"] = "Cập nhật khách hàng thành công.";

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _customerService.DeleteAsync(id);

                TempData["Success"] = "Xóa khách hàng thành công.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}