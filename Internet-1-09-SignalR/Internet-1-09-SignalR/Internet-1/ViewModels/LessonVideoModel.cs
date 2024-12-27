using System.ComponentModel.DataAnnotations;

namespace Internet_1.ViewModels
{
    public class LessonVideoModel : BaseModel
    {
        [Display(Name = "Adı")]
        [Required(ErrorMessage = "Video Adı Giriniz!")]
        public string Name { get; set; }

        [Display(Name = "Video Url")]
        public string VideoUrl { get; set; }

        [Display(Name = "Video Dosyası")]
        public IFormFile VideoFile { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; }

        public List<LessonVideoModel> SelectedVideos { get; set; } = new List<LessonVideoModel>();
    }
}
