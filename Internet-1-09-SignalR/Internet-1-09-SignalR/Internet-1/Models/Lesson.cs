using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Internet_1.Models
{



    [Table("Lessons")] // Veritabanında kullanılan tablonun adı
    public class Lesson : BaseEntity
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
        public string CoverImageUrl { get; set; } // Kapak fotoğrafı URL'si

        [Required]
        public int InstructorId { get; set; }
        public LessonInstructor Instructor { get; set; }

        [Required]
        public int VideoId { get; set; }
        public LessonVideo Video { get; set; }
        public string VideoUrl { get; set; }

        public ICollection<LessonInstructor> LessonInstructors { get; set; }
        public ICollection<LessonVideo> Videos { get; set; }  // One-to-many relationship with Videos
    }
}
