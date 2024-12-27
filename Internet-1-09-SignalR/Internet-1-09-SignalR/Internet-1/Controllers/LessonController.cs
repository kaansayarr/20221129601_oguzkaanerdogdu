using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using Internet_1.Models;
using Internet_1.Repositories;
using Internet_1.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using Internet_1.Hubs;

public class LessonController : Controller
{
    private readonly ILogger<LessonController> _logger;
    private readonly LessonRepository _lessonRepository;
    private readonly InstructorRepository _instructorRepository;
    private readonly VideoRepository _videoRepository;
    private readonly string _imageUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "images");
    private readonly string _videoUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "videos");
    private readonly IMapper _mapper;
    private readonly INotyfService _notyf;
    private readonly IWebHostEnvironment _environment;
    private readonly IHubContext<GeneralHub> _generalHub;

    public LessonController(ILogger<LessonController> logger, LessonRepository lessonRepository, InstructorRepository instructorRepository, VideoRepository videoRepository, IMapper mapper, INotyfService notyf, IWebHostEnvironment environment, IHubContext<GeneralHub> generalHub)

    {
        _lessonRepository = lessonRepository;
        _instructorRepository = instructorRepository;
        _videoRepository = videoRepository;
        _mapper = mapper;
        _notyf = notyf;
        _environment = environment;
        _logger = logger;
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

            // Eğitmen bilgilerini doldur
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
    public async Task<IActionResult> Add()
    {
        try
        {
            var instructors = await _instructorRepository.GetAllAsync();
            var videos = await _videoRepository.GetAllAsync();

            if (instructors == null || !instructors.Any())
            {
                _notyf.Warning("Henüz hiç eğitmen eklenmemiş.");
                return RedirectToAction("Index");
            }

            if (videos == null || !videos.Any())
            {
                _notyf.Warning("Henüz hiç video eklenmemiş.");
                return RedirectToAction("Index");
            }

            ViewBag.Videos = videos.Select(v => new
            {
                Id = v.Id,
                Name = v.Name,
                Url = v.VideoUrl
            }).ToList();

            ViewBag.Instructors = new SelectList(instructors, "Id", "FullName");

            return View(new LessonModel());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ders ekleme sayfası yüklenirken bir hata oluştu");
            _notyf.Error("Bir hata oluştu. Lütfen tekrar deneyin.");
            return RedirectToAction("Index");
        }
    }
    [HttpPost]
    public async Task<IActionResult> Add(LessonModel model)
    {
        if (!ModelState.IsValid)
        {
            _notyf.Error("Lütfen tüm alanları doldurun.");
            return Json(new { success = false });
        }

        try
        {
            if (model.CoverImage != null && model.CoverImage.Length > 0)
            {
                var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "images");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(model.CoverImage.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    _notyf.Error("Geçersiz resim formatı.");
                    return Json(new { success = false });
                }

                var fileName = Path.GetFileName(model.CoverImage.FileName);
                var safeFileName = fileName.Replace(" ", "_").ToLower();
                var filePath = Path.Combine(uploadFolder, safeFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.CoverImage.CopyToAsync(stream);
                }

                model.CoverImageUrl = "/uploads/images/" + safeFileName;
            }
            else
            {
                _notyf.Error("Lütfen bir resim yükleyin.");
                return Json(new { success = false });
            }

            // Instructor kontrolü
            var instructor = await _instructorRepository.GetByIdAsync(model.InstructorId);
            if (instructor == null)
            {
                _notyf.Error("Seçilen eğitmen bulunamadı.");
                return Json(new { success = false });
            }

            // Video kontrolü
            var video = await _videoRepository.GetByIdAsync(model.VideoId);
            if (video == null)
            {
                _notyf.Error("Seçilen video bulunamadı.");
                return Json(new { success = false });
            }

            var lesson = new Lesson
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                InstructorId = model.InstructorId,
                VideoId = model.VideoId,
                VideoUrl = video.VideoUrl,
                CoverImageUrl = model.CoverImageUrl,
                Created = DateTime.Now,
                Updated = DateTime.Now,
                IsActive = model.IsActive
            };

            await _lessonRepository.AddAsync(lesson);
            await _generalHub.Clients.All.SendAsync("onLessonAdd");
            _notyf.Success("Ders başarıyla eklendi.");
            var lessonCount = _lessonRepository.Where(l => l.IsActive).Count();
            await _generalHub.Clients.All.SendAsync("onLessonAdd", lessonCount);
            return Json(new { success = true, redirectUrl = Url.Action("Index", "Lesson") });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ders eklenirken bir hata oluştu");
            _notyf.Error("Ders eklenirken bir hata oluştu: " + ex.Message);
            return Json(new { success = false });
        }
    }

    public async Task<IActionResult> Update(int id)
    {
        try
        {
            var lesson = await _lessonRepository.GetByIdAsync(id);
            if (lesson == null)
            {
                _notyf.Error("Ders bulunamadı.");
                return RedirectToAction("Index");
            }

            var instructors = await _instructorRepository.GetAllAsync();
            var videos = await _videoRepository.GetAllAsync();

            ViewBag.Videos = videos.Select(v => new
            {
                Id = v.Id,
                Name = v.Name,
                Url = v.VideoUrl
            }).ToList();

            ViewBag.Instructors = new SelectList(instructors, "Id", "FullName");

            var lessonModel = _mapper.Map<LessonModel>(lesson);
            return View(lessonModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ders güncelleme sayfası yüklenirken bir hata oluştu");
            _notyf.Error("Bir hata oluştu. Lütfen tekrar deneyin.");
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Update(LessonModel model)
    {
        if (ModelState.ContainsKey("CoverImage"))
        {
            ModelState.Remove("CoverImage");
        }

        if (!ModelState.IsValid)
        {
            _notyf.Error("Lütfen tüm zorunlu alanları doldurun.");
            return Json(new { success = false });
        }

        try
        {
            var lesson = await _lessonRepository.GetByIdAsync(model.Id);
            if (lesson == null)
            {
                _notyf.Error("Ders bulunamadı.");
                return Json(new { success = false });
            }

            // Yeni resim yüklendiyse
            if (model.CoverImage != null && model.CoverImage.Length > 0)
            {
                var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "images");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(model.CoverImage.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    _notyf.Error("Geçersiz resim formatı.");
                    return Json(new { success = false });
                }

                // Eski resmi sil
                if (!string.IsNullOrEmpty(lesson.CoverImageUrl))
                {
                    var oldFilePath = Path.Combine(_environment.WebRootPath, lesson.CoverImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                var fileName = Path.GetFileName(model.CoverImage.FileName);
                var safeFileName = fileName.Replace(" ", "_").ToLower();
                var filePath = Path.Combine(uploadFolder, safeFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.CoverImage.CopyToAsync(stream);
                }

                lesson.CoverImageUrl = "/uploads/images/" + safeFileName;
            }

            // Instructor kontrolü
            var instructor = await _instructorRepository.GetByIdAsync(model.InstructorId);
            if (instructor == null)
            {
                _notyf.Error("Seçilen eğitmen bulunamadı.");
                return Json(new { success = false });
            }

            // Video kontrolü
            var video = await _videoRepository.GetByIdAsync(model.VideoId);
            if (video == null)
            {
                _notyf.Error("Seçilen video bulunamadı.");
                return Json(new { success = false });
            }

            lesson.Name = model.Name;
            lesson.Description = model.Description;
            lesson.Price = model.Price;
            lesson.InstructorId = model.InstructorId;
            lesson.VideoId = model.VideoId;
            lesson.IsActive = model.IsActive;
            lesson.Updated = DateTime.Now;

            await _lessonRepository.UpdateAsync(lesson);
            await _generalHub.Clients.All.SendAsync("onLessonUpdate");
            _notyf.Success("Ders başarıyla güncellendi.");

            return Json(new { success = true, redirectUrl = Url.Action("Index", "Lesson") });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ders güncellenirken bir hata oluştu");
            _notyf.Error("Ders güncellenirken bir hata oluştu: " + ex.Message);
            return Json(new { success = false });
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        var lesson = await _lessonRepository.GetByIdAsync(id);
        if (lesson == null)
        {
            _notyf.Error("Ders bulunamadı.");
            return RedirectToAction("Index");
        }

        var lessonModel = _mapper.Map<LessonModel>(lesson);
        return View(lessonModel);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(LessonModel model)
    {
        try
        {
            var lesson = await _lessonRepository.GetByIdAsync(model.Id);
            if (lesson == null)
            {
                _notyf.Error("Ders bulunamadı.");
                return RedirectToAction("Index");
            }

            // Resmi sil
            if (!string.IsNullOrEmpty(lesson.CoverImageUrl))
            {
                var imagePath = Path.Combine(_environment.WebRootPath, lesson.CoverImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            await _lessonRepository.DeleteAsync(model.Id);
            await _generalHub.Clients.All.SendAsync("onLessonDelete");
            _notyf.Success("Ders başarıyla silindi.");
            var lessonCount = _lessonRepository.Where(l => l.IsActive).Count();
            await _generalHub.Clients.All.SendAsync("onLessonDelete", lessonCount);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ders silinirken bir hata oluştu");
            _notyf.Error("Ders silinirken bir hata oluştu: " + ex.Message);
            return RedirectToAction("Index");
        }
    }

}



