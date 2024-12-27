using Internet_1.Models;

namespace Internet_1.ViewModels
{
    public class VideoViewModel
    {
        public List<LessonVideoModel> Videos { get; set; }
        public List<LessonModel> Lessons { get; set; }
        public List<LessonInstructor> Instructors { get; set; }
    }
}