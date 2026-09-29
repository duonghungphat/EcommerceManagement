using EcommerceManagement.Core.ViewModels;
using EcommerceManagement.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommerceManagement.Areas.Management.Controllers
{
    [Area("Management")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IAccountService _accountService;

        public UserController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _accountService.GetAllAsync();

            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new UserCreateViewModel
            {
                Roles = await _accountService.GetRolesAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Roles = await _accountService.GetRolesAsync();

                return View(model);
            }

            try
            {
                await _accountService.CreateAsync(model);

                TempData["Success"] = "Tạo tài khoản thành công.";

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                model.Roles = await _accountService.GetRolesAsync();

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditRole(int id)
        {
            var user = await _accountService.GetByIdAsync(id);

            if (user == null)
                return NotFound();

            var model = new UserRoleViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                RoleId = user.RoleId,
                Roles = await _accountService.GetRolesAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(
            UserRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Roles = await _accountService.GetRolesAsync();

                return View(model);
            }

            try
            {
                int actorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

                await _accountService.UpdateRoleAsync(model.Id, model.RoleId, actorUserId, ipAddress);

                TempData["Success"] = "Cập nhật vai trò thành công.";

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                model.Roles = await _accountService.GetRolesAsync();

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id, bool isActive)
        {
            try
            {
                await _accountService.SetActiveAsync(id, isActive);

                TempData["Success"] = isActive ? "Mở khóa tài khoản thành công." : "Khóa tài khoản thành công.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var user = await _accountService.GetByIdAsync(id);

            if (user == null)
                return NotFound();

            return View(new ResetPasswordViewModel
            {
                UserId = user.Id,
                FullName = user.FullName
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _accountService.ResetPasswordAsync(model.UserId, model.NewPassword);

                TempData["Success"] = "Đặt lại mật khẩu thành công.";

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return View(model);
            }
        }
    }
}