using Internet_1.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<LessonInstructor> Instructors { get; set; }
    public DbSet<LessonVideo> Videos { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Todo> Todos { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    //protected override void OnModelCreating(ModelBuilder modelBuilder)
    //{ }

}
