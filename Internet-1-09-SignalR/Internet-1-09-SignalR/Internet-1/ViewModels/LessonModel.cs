using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Internet_1.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Internet_1.ViewModels
{
    public class LessonModel : BaseModel
    {
        [Display(Name = "Ders Adı")]
        [Required(ErrorMessage = "Ders Adı Giriniz!")]
        public string Name { get; set; }

        [Display(Name = "Ders Açıklaması")]
        [Required(ErrorMessage = "Ders Açıklaması Giriniz!")]
        public string Description { get; set; }

        [Display(Name = "Ders Fiyatı")]
        [Required(ErrorMessage = "Ders Fiyatı Giriniz!")]
        public decimal Price { get; set; }

        [Display(Name = "Ders Resmi")]
        [NotMapped] // Bu özelliği veritabanında saklamayacak
        public IFormFile CoverImage { get; set; } // Kapak fotoğrafı
        public string CoverImageUrl { get; set; } // Kapak fotoğrafı URL'si

        // Eğitmen bilgisi
        [Display(Name = "Dersin Eğitmeni")]
        public int InstructorId { get; set; } // Eğitmenin Id'si
        public LessonInstructor Instructor { get; set; } // Navigation property
        public string InstructorFullname { get; set; }

        // Video bilgisi
        [Display(Name = "Video")]
        [Required(ErrorMessage = "Lütfen bir video seçin")]
        public int VideoId { get; set; }
        public string VideoUrl { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true; // Varsayılan olarak aktif

        // Seçilebilir Eğitmenler
        public List<LessonInstructor> AvailableInstructors { get; set; } = new List<LessonInstructor>();

        // Videoların ID'lerini tutan bir liste
        [Display(Name = "Seçilen Videolar")]
        public List<SelectListItem> SelectedVideos { get; set; } = new List<SelectListItem>();

        // Seçilebilir Videolar
        public List<string> VideoIds { get; set; } = new List<string>();
        public List<LessonVideo> AvailableVideos { get; set; } = new List<LessonVideo>();
    }
}
