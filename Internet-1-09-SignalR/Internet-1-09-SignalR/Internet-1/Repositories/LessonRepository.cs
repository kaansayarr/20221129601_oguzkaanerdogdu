using AutoMapper;
using Internet_1.Models;
using Internet_1.ViewModels;

namespace Internet_1.Repositories
{
    public class LessonRepository : GenericRepository<Lesson>
    {
        public LessonRepository(AppDbContext context) : base(context)
        {
        }
    }
}
