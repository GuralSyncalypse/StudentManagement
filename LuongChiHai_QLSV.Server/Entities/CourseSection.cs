using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuongChiHai_QLSV.Server.Entities
{
    [Table("CourseSection")]
    public class CourseSection
    {
        [Key]
        public int SectionID { get; set; }

        [Required]
        [StringLength(10)]
        public string CourseID { get; set; } = null!;

        public int Semester { get; set; }

        [Required]
        [StringLength(10)]
        public string ClassSection { get; set; } = "L01";

        public int MaxCapacity { get; set; }

        [StringLength(20)]
        public string? Status { get; set; } = "Open";

        // Navigation Properties
        [ForeignKey(nameof(CourseID))]
        public virtual Course Course { get; set; } = null!;

        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
