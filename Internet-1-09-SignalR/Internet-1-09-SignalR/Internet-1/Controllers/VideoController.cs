using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using Internet_1.Hubs;
using Internet_1.Models;
using Internet_1.Repositories;
using Internet_1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;


namespace Internet_1.Controllers
{
    [Authorize]
    public class videoController : Controller
    {
        private readonly VideoRepository _videoRepository;
        private readonly LessonRepository _lessonRepository;
        private readonly InstructorRepository _instructorRepository;
        private readonly ILogger<LessonController> _logger;
        private readonly INotyfService _notyf;
        private readonly IMapper _mapper;
        private readonly IHubContext<GeneralHub> _generalHub;
        private readonly IWebHostEnvironment _environment;


        public videoController(
            ILogger<LessonController> logger,
            VideoRepository videoRepository,
            LessonRepository lessonRepository,
            InstructorRepository instructorRepository,
            INotyfService notyf,
            IMapper mapper,
            IHubContext<GeneralHub> generalHub,
            IWebHostEnvironment environment)
        {
            _videoRepository = videoRepository;
            _lessonRepository = lessonRepository;
            _instructorRepository = instructorRepository;
            _notyf = notyf;
            _mapper = mapper;
            _generalHub = generalHub;
            _logger = logger;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var videos = await _videoRepository.GetAllAsync();
                var lessons = await _lessonRepository.GetAllAsync();
                var instructors = await _instructorRepository.GetAllAsync();

                var viewModel = new VideoViewModel
                {
                    Videos = _mapper.Map<List<LessonVideoModel>>(videos.OrderByDescending(v => v.Created)),
                    Lessons = _mapper.Map<List<LessonModel>>(lessons),
                    Instructors = instructors
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Video listesi yüklenirken bir hata oluştu");
                _notyf.Error("Video listesi yüklenirken bir hata oluştu.");
                return View(new VideoViewModel());
            }
        }

        public IActionResult Add()
        {
            var model = new LessonVideoModel
            {
                // Initialize any collections to avoid null references
                SelectedVideos = new List<LessonVideoModel>() // Ensure this is initialized
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Add(LessonVideoModel model)
        {
            if (!ModelState.IsValid)
            {
                _notyf.Error("Lütfen tüm alanları doldurun.");
                return View(model);
            }

            try
            {
                if (model.VideoFile != null && model.VideoFile.Length > 0)
                {
                    var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "videos");

                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    var fileName = Path.GetFileName(model.VideoFile.FileName);
                    var safeFileName = fileName.Replace(" ", "_").ToLower();
                    var filePath = Path.Combine(uploadFolder, safeFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.VideoFile.CopyToAsync(stream);
                    }

                    var lessonVideo = new LessonVideo
                    {
                        Name = model.Name,
                        VideoUrl = "/uploads/videos/" + safeFileName,
                        Created = DateTime.Now,
                        Updated = DateTime.Now,
                        IsActive = model.IsActive
                    };

                    await _videoRepository.AddAsync(lessonVideo);
                    await _generalHub.Clients.All.SendAsync("onVideoAdd");
                    _notyf.Success("Video başarıyla eklendi.");

                    return RedirectToAction("Index");
                }
                else
                {
                    _notyf.Error("Lütfen bir video dosyası seçin.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Video eklenirken bir hata oluştu");
                _notyf.Error("Video eklenirken bir hata oluştu: " + ex.Message);
                return View(model);
            }
        }
        public async Task<IActionResult> Update(int id)
        {
            var videos = await _videoRepository.GetByIdAsync(id);
            var videoModels = _mapper.Map<LessonVideoModel>(videos);
            return View(videoModels);
        }

        [HttpPost]
        public async Task<IActionResult> Update(LessonVideoModel model)
        {
            try
            {
                var video = await _videoRepository.GetByIdAsync(model.Id);
                if (video == null)
                {
                    _notyf.Error("Video bulunamadı.");
                    return RedirectToAction("Index");
                }

                // Video dosyası yüklendiyse
                if (model.VideoFile != null && model.VideoFile.Length > 0)
                {
                    // Eski videoyu sil
                    if (!string.IsNullOrEmpty(video.VideoUrl))
                    {
                        var oldVideoPath = Path.Combine(_environment.WebRootPath, video.VideoUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldVideoPath))
                        {
                            System.IO.File.Delete(oldVideoPath);
                        }
                    }

                    // Yeni videoyu yükle
                    var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "videos");
                    var fileName = Path.GetFileName(model.VideoFile.FileName);
                    var safeFileName = fileName.Replace(" ", "_").ToLower();
                    var filePath = Path.Combine(uploadFolder, safeFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.VideoFile.CopyToAsync(stream);
                    }

                    video.VideoUrl = "/uploads/videos/" + safeFileName;
                }

                // Diğer bilgileri güncelle
                video.Name = model.Name;
                video.IsActive = model.IsActive;
                video.Updated = DateTime.Now;

                await _videoRepository.UpdateAsync(video);
                await _generalHub.Clients.All.SendAsync("onVideoUpdate"); // SignalR bildirimi
                _notyf.Success("Video başarıyla güncellendi.");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Video güncellenirken bir hata oluştu");
                _notyf.Error("Video güncellenirken bir hata oluştu: " + ex.Message);
                return View(model);
            }
        }


        private async Task<string> SaveVideoFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            var fileName = Path.GetFileName(file.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "videos", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return filePath;
        }

        public async Task<IActionResult> Delete(int id)
        {

            var video = await _videoRepository.GetByIdAsync(id);
            if (video == null)
            {
                _notyf.Error("Video bulunamadı.");
                return RedirectToAction("Index");
            }

            var model = new LessonVideoModel
            {
                Id = video.Id,
                Name = video.Name,
                VideoUrl = video.VideoUrl  // Video URL'yi de modele ekliyoruz
            };

            return View(model);  // Video bilgilerini onay için view'e gönderiyoruz
        }
        [HttpPost]
        public async Task<IActionResult> Delete(LessonVideoModel model)
        {
            try
            {
                // Önce videoyu kontrol et
                var video = await _videoRepository.GetByIdAsync(model.Id);
                if (video == null)
                {
                    _notyf.Error("Video bulunamadı.");
                    return RedirectToAction("Index");
                }

                // Video herhangi bir derse bağlı mı kontrol et
                var lessons = await _lessonRepository.GetAllAsync();
                if (lessons != null && lessons.Any(l => l.VideoId == model.Id))
                {
                    _notyf.Error("Bu video bir veya daha fazla derse bağlı olduğu için silinemez!");
                    return RedirectToAction("Index");
                }

                // Video dosyasını sil
                if (!string.IsNullOrEmpty(video.VideoUrl))
                {
                    var videoPath = Path.Combine(_environment.WebRootPath, video.VideoUrl.TrimStart('/'));
                    if (System.IO.File.Exists(videoPath))
                    {
                        System.IO.File.Delete(videoPath);
                    }
                }

                // Videoyu veritabanından sil
                await _videoRepository.DeleteAsync(model.Id);
                await _generalHub.Clients.All.SendAsync("onVideoDelete");
                _notyf.Success("Video başarıyla silindi.");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Video silinirken bir hata oluştu");
                _notyf.Error("Video silinirken bir hata oluştu: " + ex.Message);
                return RedirectToAction("Index");
            }
        }

    }
}
