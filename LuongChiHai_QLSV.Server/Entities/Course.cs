using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuongChiHai_QLSV.Server.Entities
{
    [Table("Course")]
    public class Course
    {
        [Key]
        [StringLength(10)]
        public string CourseID { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string CourseName { get; set; } = null!;

        public int Credits { get; set; }

        // Navigation Properties
        public virtual ICollection<CourseSection> CourseSections { get; set; } = new List<CourseSection>();
    }
}
