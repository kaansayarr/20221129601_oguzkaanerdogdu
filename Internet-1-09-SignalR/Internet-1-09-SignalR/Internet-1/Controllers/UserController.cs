using AspNetCoreHero.ToastNotification.Abstractions;
using AspNetCoreHero.ToastNotification.Notyf.Models;
using AutoMapper;
using Internet_1.Hubs;
using Internet_1.Models;
using Internet_1.Repositories;
using Internet_1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using NETCore.Encrypt.Extensions;
using System.Collections.Specialized;

namespace Internet_1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {

        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly INotyfService _notyf;
        private readonly IFileProvider _fileProvider;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly ILogger<UserController> _logger;
        private readonly IHubContext<GeneralHub> _generalHub;
        public UserController(IMapper mapper, IConfiguration config, INotyfService notyf, IFileProvider fileProvider, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, ILogger<UserController> logger, IHubContext<GeneralHub> generalHub)
        {

            _mapper = mapper;
            _config = config;
            _notyf = notyf;
            _fileProvider = fileProvider;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _generalHub = generalHub;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userModels = new List<UserModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userModel = _mapper.Map<UserModel>(user);
                userModel.Role = roles.ToList();
                userModels.Add(userModel);
            }

            return View(userModels);
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(UserModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _notyf.Error("Lütfen tüm alanları doldurun.");
                    return View(model);
                }

                if (string.IsNullOrEmpty(model.SelectedRole))
                {
                    _notyf.Error("Lütfen bir rol seçin.");
                    return View(model);
                }

                var user = new AppUser
                {
                    FullName = model.FullName,
                    UserName = model.UserName,
                    Email = model.Email,
                    PhotoUrl = "no-img.png"
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                        _notyf.Error(error.Description);
                    }
                    return View(model);
                }

                // Seçilen rolü ata
                await _userManager.AddToRoleAsync(user, model.SelectedRole);
                await _generalHub.Clients.All.SendAsync("onUserAdd"); // SignalR bildirimi
                _notyf.Success("Kullanıcı başarıyla oluşturuldu.");
                return RedirectToAction("Index", "User");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı eklenirken bir hata oluştu");
                _notyf.Error("Kullanıcı eklenirken bir hata oluştu: " + ex.Message);
                return View(model);
            }
        }
        public async Task<IActionResult> Update(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var userModel = _mapper.Map<UserModel>(user);
            return View(userModel);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UserModel model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    _notyf.Error("Kullanıcı bulunamadı.");
                    return RedirectToAction("Index", "User");
                }

                // Admin kullanıcısının rolünü değiştirmeye çalışıyorsa engelle
                if (user.UserName == "admin" && model.SelectedRole != "Admin")
                {
                    _notyf.Error("Admin kullanıcısının rolü değiştirilemez!");
                    return RedirectToAction("Index", "User");
                }

                user.FullName = model.FullName;
                user.UserName = model.UserName;
                user.Email = model.Email;

                // Şifre değişikliği varsa
                if (!string.IsNullOrEmpty(model.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var passwordResult = await _userManager.ResetPasswordAsync(user, token, model.Password);
                    if (!passwordResult.Succeeded)
                    {
                        foreach (var error in passwordResult.Errors)
                        {
                            _notyf.Error(error.Description);
                        }
                        return View(model);
                    }
                }

                // Rol güncelleme
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.FirstOrDefault() != model.SelectedRole)
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRoleAsync(user, model.SelectedRole);
                }

                var result = await _userManager.UpdateAsync(user);
                await _generalHub.Clients.All.SendAsync("onUserUpdate");

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        _notyf.Error(error.Description);
                    }
                    return View(model);
                }

                _notyf.Success("Kullanıcı başarıyla güncellendi.");
                return RedirectToAction("Index", "User");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı güncellenirken bir hata oluştu");
                _notyf.Error("Kullanıcı güncellenirken bir hata oluştu: " + ex.Message);
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var userModel = _mapper.Map<UserModel>(user);
            return View(userModel);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(UserModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                _notyf.Error("Yönetici Üye Silinemez!");
                return RedirectToAction("Index", "User");
            }

            await _userManager.DeleteAsync(user);
            await _generalHub.Clients.All.SendAsync("onUserDelete"); // Bildirim gönder
            _notyf.Success("Kullanıcı başarıyla silindi.");
            return RedirectToAction("Index", "User");
        }

        public async Task<IActionResult> Profile()
        {
            var userName = User.Identity.Name;
            var user = await _userManager.Users.Where(s => s.UserName == userName).FirstOrDefaultAsync();
            var userModel = _mapper.Map<RegisterModel>(user);
            return View(userModel);
        }
        [HttpPost]
        public async Task<IActionResult> Profile(RegisterModel model)
        {
            var userName = User.Identity.Name;
            var user = await _userManager.Users.Where(s => s.UserName == userName).FirstOrDefaultAsync();

            if (model.Password != model.PasswordConfirm)
            {
                _notyf.Error("Parola Tekrarı Tutarsız!");
                return RedirectToAction("Profile");
            }

            user.FullName = model.FullName;
            user.UserName = model.UserName;
            user.Email = model.Email;


            var rootFolder = _fileProvider.GetDirectoryContents("wwwroot");
            var photoUrl = "no-img.png";
            if (model.PhotoFile != null)
            {
                var filename = Guid.NewGuid().ToString() + Path.GetExtension(model.PhotoFile.FileName);
                var photoPath = Path.Combine(rootFolder.First(x => x.Name == "userPhotos").PhysicalPath, filename);
                using var stream = new FileStream(photoPath, FileMode.Create);
                model.PhotoFile.CopyTo(stream);
                photoUrl = filename;

            }

            user.PhotoUrl = photoUrl;


            await _userManager.UpdateAsync(user);
            _notyf.Success("Kullanıcı Bilgileri Güncellendi");

            return RedirectToAction("Index", "Admin");

        }
    }
}