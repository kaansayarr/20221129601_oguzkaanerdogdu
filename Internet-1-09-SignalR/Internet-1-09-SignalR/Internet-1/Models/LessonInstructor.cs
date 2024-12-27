using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Internet_1.Models
{
    [Table("Instructors")] // Veritabanında kullanılan tablonun adı

    public class LessonInstructor : BaseEntity
    {
        public string FullName { get; set; }

    
    }
}
