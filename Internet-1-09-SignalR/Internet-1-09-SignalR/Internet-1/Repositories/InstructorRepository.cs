using AutoMapper;
using Internet_1.Models;
using Internet_1.ViewModels;

namespace Internet_1.Repositories
{
    public class InstructorRepository : GenericRepository<LessonInstructor>
    {
        public InstructorRepository(AppDbContext context) : base(context)
        {
        }
    }
}
