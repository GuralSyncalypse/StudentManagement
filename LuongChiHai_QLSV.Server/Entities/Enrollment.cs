using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuongChiHai_QLSV.Server.Entities
{
    [Table("Enrollment")]
    public class Enrollment
    {
        [Key]
        public int EnrollmentID { get; set; }

        [Required]
        [StringLength(15)]
        public string StudentID { get; set; } = null!;

        public int SectionID { get; set; }

        public DateTime? EnrollDate { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey(nameof(StudentID))]
        public virtual Student Student { get; set; } = null!;

        [ForeignKey(nameof(SectionID))]
        public virtual CourseSection CourseSection { get; set; } = null!;

        public virtual ICollection<Score> Scores { get; set; } = new List<Score>();
    }
}
