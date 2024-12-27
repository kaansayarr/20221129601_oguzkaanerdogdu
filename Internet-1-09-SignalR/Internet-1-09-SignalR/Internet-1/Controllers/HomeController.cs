using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using Internet_1.Hubs;
using Internet_1.Models;
using Internet_1.Repositories;
using Internet_1.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

namespace Internet_1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly LessonRepository _lessonRepository;
        private readonly InstructorRepository _instructorRepository;
        private readonly VideoRepository _videoRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly INotyfService _notyf;
        private readonly IHubContext<GeneralHub> _generalHub;

        public HomeController(
            ILogger<HomeController> logger,
            LessonRepository lessonRepository,
            VideoRepository videoRepository,
            InstructorRepository instructorRepository,
            IMapper mapper,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<AppRole> roleManager,
            INotyfService notyf,
            IHubContext<GeneralHub> generalHub)
        {
            _logger = logger;
            _lessonRepository = lessonRepository;
            _videoRepository = videoRepository;
            _instructorRepository = instructorRepository;
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _notyf = notyf;
            _generalHub = generalHub;
        }

        public async Task<IActionResult> Index()
        {
            try
            {

                var lessons = await _lessonRepository.GetAllAsync();
                var instructors = await _instructorRepository.GetAllAsync();
                var videos = await _videoRepository.GetAllAsync();

                var viewModel = new LessonViewModel
                {
                    Lessons = _mapper.Map<List<LessonModel>>(lessons.OrderByDescending(l => l.Created)),
                    Instructors = instructors,
                    Videos = _mapper.Map<List<LessonVideoModel>>(videos)
                };
                foreach (var lesson in viewModel.Lessons)
                {
                    var instructor = viewModel.Instructors.FirstOrDefault(i => i.Id == lesson.InstructorId);
                    if (instructor != null)
                    {
                        lesson.InstructorFullname = instructor.FullName;
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ders listesi yüklenirken bir hata oluştu");
                _notyf.Error("Ders listesi yüklenirken bir hata oluştu.");
                return View(new LessonViewModel());
            }
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            var identityResult = await _userManager.CreateAsync(new() { UserName = model.UserName, Email = model.Email, FullName = model.FullName, PhotoUrl = "no-img.png" }, model.Password);

            if (!identityResult.Succeeded)
            {
                foreach (var item in identityResult.Errors)
                {
                    ModelState.AddModelError("", item.Description);


                    _notyf.Error(item.Description);
                }

                return View(model);
            }

            // default olarak Uye rolü ekleme
            var user = await _userManager.FindByNameAsync(model.UserName);
            var roleExist = await _roleManager.RoleExistsAsync("Uye");
            if (!roleExist)
            {
                var role = new AppRole { Name = "Uye" };
                await _roleManager.CreateAsync(role);
            }

            await _userManager.AddToRoleAsync(user, "Uye");
            await _generalHub.Clients.All.SendAsync("onUserAdd");
            _notyf.Success("Üye Kaydı Yapılmıştır. Oturum Açınız");
            return RedirectToAction("Login");
        }

        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                _notyf.Error("Lütfen tüm alanları doldurun.");
                return View(model);
            }

            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                _notyf.Error("Kullanıcı adı veya şifre hatalı.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, true);
            if (result.Succeeded)
            {
                _notyf.Success("Giriş başarılı.");

                // Admin rolündeyse Admin paneline, değilse ana sayfaya yönlendir
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }

                // Normal kullanıcı için ana sayfaya veya returnUrl'e yönlendir
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                _notyf.Error("Hesabınız kilitlendi. Lütfen daha sonra tekrar deneyin.");
                return View(model);
            }

            _notyf.Error("Kullanıcı adı veya şifre hatalı.");
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _notyf.Success("Çıkış yapıldı.");
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult AccessDenied(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                _signInManager.SignOutAsync().Wait();
                _notyf.Warning("Bu sayfaya erişim yetkiniz bulunmamaktadır. Lütfen uygun yetkiye sahip bir hesapla giriş yapın.");
            }
            return View();
        }

        [HttpGet("ders/{name}")]
        public async Task<IActionResult> LessonDetail(string name)
        {
            try
            {
                var lesson = await _lessonRepository.GetAllAsync();
                var targetLesson = lesson.FirstOrDefault(l => l.Name.ToLower().Replace(" ", "-") == name.ToLower());

                if (targetLesson == null)
                {
                    _notyf.Error("Ders bulunamadı.");
                    return RedirectToAction("Index");
                }

                var instructor = await _instructorRepository.GetByIdAsync(targetLesson.InstructorId);
                var video = await _videoRepository.GetByIdAsync(targetLesson.VideoId);

                var lessonModel = _mapper.Map<LessonModel>(targetLesson);
                lessonModel.InstructorFullname = instructor?.FullName;
                lessonModel.VideoUrl = video?.VideoUrl;

                return View(lessonModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ders detay sayfası yüklenirken bir hata oluştu");
                _notyf.Error("Bir hata oluştu. Lütfen tekrar deneyin.");
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Courses()
        {
            try
            {
                var lessons = await _lessonRepository.GetAllAsync();
                var instructors = await _instructorRepository.GetAllAsync();
                var videos = await _videoRepository.GetAllAsync();

                var viewModel = new LessonViewModel
                {
                    Lessons = _mapper.Map<List<LessonModel>>(lessons.Where(l => l.IsActive).OrderByDescending(l => l.Created)),
                    Instructors = instructors,
                    Videos = _mapper.Map<List<LessonVideoModel>>(videos)
                };

                foreach (var lesson in viewModel.Lessons)
                {
                    var instructor = instructors.FirstOrDefault(i => i.Id == lesson.InstructorId);
                    if (instructor != null)
                    {
                        lesson.InstructorFullname = instructor.FullName;
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ders listesi yüklenirken bir hata oluştu");
                _notyf.Error("Ders listesi yüklenirken bir hata oluştu.");
                return View(new LessonViewModel());
            }
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpGet("kullanici/{username}")]
        public async Task<IActionResult> UserProfile(string username)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(username);
                if (user == null)
                {
                    _notyf.Error("Kullanıcı bulunamadı.");
                    return RedirectToAction("Index");
                }

                var model = new UserProfileViewModel
                {
                    UserName = user.UserName,
                    FullName = user.FullName,
                    PhotoUrl = user.PhotoUrl ?? "no-image.jpg",
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı profili yüklenirken bir hata oluştu");
                _notyf.Error("Bir hata oluştu. Lütfen tekrar deneyin.");
                return RedirectToAction("Index");
            }
        }

        [Authorize]
        public async Task<IActionResult> MyProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _notyf.Error("Kullanıcı bulunamadı.");
                return RedirectToAction("Index");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var model = new UserProfileViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                PhotoUrl = user.PhotoUrl ?? "no-image.jpg",
                Role = roles.FirstOrDefault()
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(UserProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _notyf.Error("Lütfen tüm alanları doldurun.");
                return View("MyProfile", model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _notyf.Error("Kullanıcı bulunamadı.");
                return RedirectToAction("Index");
            }

            user.FullName = model.FullName;
            user.Email = model.Email;

            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (string.IsNullOrEmpty(model.CurrentPassword))
                {
                    ModelState.AddModelError("CurrentPassword", "Mevcut şifrenizi girmelisiniz.");
                    return View("MyProfile", model);
                }

                var passwordCheck = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
                if (!passwordCheck)
                {
                    ModelState.AddModelError("CurrentPassword", "Mevcut şifreniz hatalı.");
                    return View("MyProfile", model);
                }

                var passwordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View("MyProfile", model);
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                _notyf.Success("Profil başarıyla güncellendi.");
                return RedirectToAction("MyProfile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View("MyProfile", model);
        }
    }
}
