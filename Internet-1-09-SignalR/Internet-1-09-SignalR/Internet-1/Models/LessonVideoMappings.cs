using Internet_1.Models;
using System.ComponentModel.DataAnnotations.Schema;

[Table("LessonVideoMappings")]
public class LessonVideoMapping
{
    public int LessonId { get; set; }
    public Lesson Lesson { get; set; }

    public int VideoId { get; set; }
    public LessonVideo Video { get; set; }
}
