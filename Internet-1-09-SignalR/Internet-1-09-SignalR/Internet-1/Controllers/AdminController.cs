using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Internet_1.Models;
using Internet_1.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using Internet_1.Hubs;

namespace Internet_1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly LessonRepository _lessonRepository;
        private readonly VideoRepository _videoRepository;
        private readonly InstructorRepository _instructorRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<AdminController> _logger;
        private readonly IHubContext<GeneralHub> _generalHub;

        public AdminController(
            LessonRepository lessonRepository,
            VideoRepository videoRepository,
            InstructorRepository instructorRepository,
            UserManager<AppUser> userManager,
            ILogger<AdminController> logger,
            IHubContext<GeneralHub> generalHub)
        {
            _lessonRepository = lessonRepository;
            _videoRepository = videoRepository;
            _instructorRepository = instructorRepository;
            _userManager = userManager;
            _logger = logger;
            _generalHub = generalHub;
        }

        public async Task<IActionResult> Index()
        {
            var lessons = await _lessonRepository.GetAllAsync();
            var videos = await _videoRepository.GetAllAsync();
            var instructors = await _instructorRepository.GetAllAsync();
            var users = await _userManager.Users.ToListAsync();

            ViewBag.TotalLessons = lessons.Count();
            ViewBag.TotalVideos = videos.Count();
            ViewBag.TotalInstructors = instructors.Count();
            ViewBag.TotalUsers = users.Count();

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetCounts()
        {
            try
            {
                var lessons = await _lessonRepository.GetAllAsync();
                var videos = await _videoRepository.GetAllAsync();
                var instructors = await _instructorRepository.GetAllAsync();
                var users = await _userManager.Users.ToListAsync();

                var data = new
                {
                    lessonCount = lessons?.Count() ?? 0,
                    videoCount = videos?.Count() ?? 0,
                    instructorCount = instructors?.Count() ?? 0,
                    userCount = users?.Count ?? 0
                };

                _logger.LogInformation($"Counts - Lessons: {data.lessonCount}, Videos: {data.videoCount}, Instructors: {data.instructorCount}, Users: {data.userCount}");
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCounts'da hata oluştu");
                return Json(new { error = ex.Message });
            }
        }
    }
}
