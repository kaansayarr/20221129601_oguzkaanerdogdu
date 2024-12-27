using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Internet_1.Models
{
    [Table("Videos")] // Veritabanında kullanılan tablonun adı
    public class LessonVideo : BaseEntity
    {
        public int Id { get; set; } 
        public string Name { get; set; }
        public string VideoUrl { get; set; }

    }
}
