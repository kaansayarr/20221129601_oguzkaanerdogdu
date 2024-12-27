using AspNetCoreHero.ToastNotification.Abstractions;
using Internet_1.Models;
using Internet_1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Internet_1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly RoleManager<AppRole> _roleManager;
        private readonly INotyfService _notyf;

        public RoleController(RoleManager<AppRole> roleManager, INotyfService notyf)
        {
            _roleManager = roleManager;
            _notyf = notyf;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var roleViewModels = roles.Select(r => new UserRoleModel
            {
                Id = r.Id,
                RoleName = r.Name
            }).ToList();

            return View(roleViewModels);
        }

        public IActionResult Add()
        {
            return View(new UserRoleModel());
        }

        [HttpPost]
        public async Task<IActionResult> Add(UserRoleModel model)
        {
            if (!ModelState.IsValid)
            {
                _notyf.Error("Lütfen tüm alanları doldurun.");
                return Json(new { success = false });
            }

            try
            {
                if (await _roleManager.RoleExistsAsync(model.Name))
                {
                    _notyf.Error("Bu rol zaten mevcut.");
                    return Json(new { success = false });
                }

                var role = new AppRole { Name = model.Name };
                var result = await _roleManager.CreateAsync(role);

                if (result.Succeeded)
                {
                    _notyf.Success("Rol başarıyla oluşturuldu.");
                    return Json(new { success = true, redirectUrl = Url.Action("Index", "Role") });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                _notyf.Error("Rol oluşturulurken bir hata oluştu.");
                return Json(new { success = false });
            }
            catch (Exception ex)
            {
                _notyf.Error("Bir hata oluştu: " + ex.Message);
                return Json(new { success = false });
            }
        }

        public async Task<IActionResult> Update(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                _notyf.Error("Rol bulunamadı.");
                return RedirectToAction("Index");
            }

            var model = new UserRoleModel
            {
                Id = role.Id,
                RoleName = role.Name
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UserRoleModel model)
        {
            if (!ModelState.IsValid)
            {
                _notyf.Error("Lütfen tüm alanları doldurun.");
                return Json(new { success = false });
            }

            try
            {
                var role = await _roleManager.FindByIdAsync(model.Id);
                if (role == null)
                {
                    _notyf.Error("Rol bulunamadı.");
                    return Json(new { success = false });
                }

                if (role.Name != model.Name)
                {
                    if (await _roleManager.RoleExistsAsync(model.Name))
                    {
                        _notyf.Error("Bu rol adı zaten kullanılıyor.");
                        return Json(new { success = false });
                    }

                    role.Name = model.Name;
                    var result = await _roleManager.UpdateAsync(role);

                    if (result.Succeeded)
                    {
                        _notyf.Success("Rol başarıyla güncellendi.");
                        return Json(new { success = true, redirectUrl = Url.Action("Index", "Role") });
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    _notyf.Error("Rol güncellenirken bir hata oluştu.");
                    return Json(new { success = false });
                }

                return Json(new { success = true, redirectUrl = Url.Action("Index", "Role") });
            }
            catch (Exception ex)
            {
                _notyf.Error("Bir hata oluştu: " + ex.Message);
                return Json(new { success = false });
            }
        }

        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                _notyf.Error("Rol bulunamadı.");
                return RedirectToAction("Index");
            }

            var model = new UserRoleModel
            {
                Id = role.Id,
                Name = role.Name
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(UserRoleModel model)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(model.Id);
                if (role == null)
                {
                    _notyf.Error("Rol bulunamadı.");
                    return RedirectToAction("Index");
                }

                if (role.Name == "Admin")
                {
                    _notyf.Error("Admin rolü silinemez.");
                    return RedirectToAction("Index");
                }

                var result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    _notyf.Success("Rol başarıyla silindi.");
                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    _notyf.Error(error.Description);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _notyf.Error("Bir hata oluştu: " + ex.Message);
                return RedirectToAction("Index");
            }
        }
    }
}