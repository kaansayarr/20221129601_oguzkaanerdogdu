using AutoMapper;
using Internet_1.Models;
using Internet_1.ViewModels;

namespace Internet_1.Repositories
{
    public class VideoRepository : GenericRepository<LessonVideo>
    {
        public VideoRepository(AppDbContext context) : base(context)
        {
        }
    }
}
