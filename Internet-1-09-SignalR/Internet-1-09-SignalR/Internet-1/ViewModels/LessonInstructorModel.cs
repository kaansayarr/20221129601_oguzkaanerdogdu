using System.ComponentModel.DataAnnotations;

namespace Internet_1.ViewModels
{
    public class LessonInstructorModel : BaseModel
    {

        public int Id { get; set; }  // Add Id for the unique identifier
        [Required(ErrorMessage = "Eğitmen Adı Giriniz!")]
        public int InstructorId { get; set; } // Foreign key to Instructor

        public string Fullname { get; set; }

        public int? LessonId { get; set; }

    }
}
