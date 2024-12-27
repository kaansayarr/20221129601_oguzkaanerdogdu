using Internet_1.Models;

namespace Internet_1.ViewModels
{
    public class LessonViewModel
    {
        public List<LessonModel> Lessons { get; set; }
        public List<LessonInstructor> Instructors { get; set; }
        public List<LessonVideoModel> Videos { get; set; }

        public LessonViewModel()
        {
            Lessons = new List<LessonModel>();
            Instructors = new List<LessonInstructor>();
            Videos = new List<LessonVideoModel>();
        }
    }
}