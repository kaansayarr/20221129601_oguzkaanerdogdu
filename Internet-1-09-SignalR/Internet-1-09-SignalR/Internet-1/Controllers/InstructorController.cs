using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using Internet_1.Hubs;
using Internet_1.Models;
using Internet_1.Repositories;
using Internet_1.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


public class InstructorController : Controller
{
    private readonly InstructorRepository _instructorRepository;
    private readonly ILogger<InstructorController> _logger;
    public readonly AppDbContext _context;
    private readonly LessonRepository _lessonRepository;
    private readonly INotyfService _notyf;
    private readonly IMapper _mapper;
    private readonly IHubContext<GeneralHub> _generalHub;


    public InstructorController(AppDbContext contex, ILogger<InstructorController> logger, InstructorRepository instructorRepository, LessonRepository lessonRepository, IHubContext<GeneralHub> generalHub, INotyfService notyf, IMapper mapper)
    {
        _instructorRepository = instructorRepository;
        _notyf = notyf;
        _lessonRepository = lessonRepository;
        _mapper = mapper;
        _generalHub = generalHub;
        _logger = logger;

    }
    [HttpGet]
    public async Task<IActionResult> GetAllInstructors()
    {
        var instructors = await _context.Instructors.ToListAsync();
        return Ok(instructors); // JSON olarak verileri döndürüyoruz
    }
    public async Task<IActionResult> Index()
    {
        var instructors = await _instructorRepository.GetAllAsync();
        var instructorModels = _mapper.Map<List<LessonInstructorModel>>(instructors);
        return View(instructorModels);
    }

    public IActionResult Add()
    {
        var model = new LessonInstructorModel();
        return View(model); // Boş bir model gönnder
    }

    [HttpPost]
    public async Task<IActionResult> Add(LessonInstructorModel model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Lütfen tüm alanları doldurun." });
        }

        try
        {
            var instructor = new LessonInstructor
            {
                FullName = model.Fullname,
                Created = DateTime.Now,
                Updated = DateTime.Now,
                IsActive = model.IsActive
            };

            await _instructorRepository.AddAsync(instructor);
            await _generalHub.Clients.All.SendAsync("onInstructorAdd");
            _notyf.Success("Eğitmen başarıyla eklendi.");

            // Notyf kaldırıldı, sadece başarılı yanıt dönüyoruz
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eğitmen eklenirken bir hata oluştu");
            return Json(new { success = false, message = "Eğitmen eklenirken bir hata oluştu: " + ex.Message });
        }
    }


    public async Task<IActionResult> Update(int id)
    {
        var ınstructor = await _instructorRepository.GetByIdAsync(id);
        var ınstructorModel = _mapper.Map<LessonInstructorModel>(ınstructor);
        return View(ınstructorModel);
    }


    [HttpPost]
    public async Task<IActionResult> Update(LessonInstructorModel model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Lütfen tüm alanları doldurun." });
        }

        try
        {
            var instructor = await _instructorRepository.GetByIdAsync(model.Id);
            if (instructor == null)
            {
                return Json(new { success = false, message = "Eğitmen bulunamadı." });
            }

            instructor.FullName = model.Fullname;
            instructor.IsActive = model.IsActive;
            instructor.Updated = DateTime.Now;


            await _instructorRepository.UpdateAsync(instructor);
            await _generalHub.Clients.All.SendAsync("OnInstructorUpdate");
            _notyf.Success("Eğitmen Güncellendi...");
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eğitmen güncellenirken bir hata oluştu");
            return Json(new { success = false, message = "Eğitmen güncellenirken bir hata oluştu: " + ex.Message });
        }
    }
    public async Task<IActionResult> Delete(int id)
    {
        var instructor = await _instructorRepository.GetByIdAsync(id);
        if (instructor == null)
        {
            _notyf.Error("Eğitmen bulunamadı.");
            return RedirectToAction("Index");  // Redirect to the list page if instructor not found
        }

        var model = new LessonInstructorModel
        {
            Id = instructor.Id,
            Fullname = instructor.FullName
        };

        return View(model);  // Pass the instructor details to the view for confirmation
    }

    // POST: /Instructor/Delete
    [HttpPost]
    public async Task<IActionResult> Delete(LessonInstructorModel model)
    {
        var lessons = await _lessonRepository.GetAllAsync();
        if (lessons.Any(c => c.InstructorId == model.Id))  // Ensure no lessons are linked to the instructor
        {
            _notyf.Error("Üzerinde Ders Kayıtlı Olan Eğitmen Silinemez!");
            return RedirectToAction("Index");
        }

        // Proceed to delete the instructor
        await _instructorRepository.DeleteAsync(model.Id);
        await _generalHub.Clients.All.SendAsync("onInstructorDelete");
        _notyf.Success("Eğitmen başarıyla silindi.");
        return RedirectToAction("Index");
    }
}


