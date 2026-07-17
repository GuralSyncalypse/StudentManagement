using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuongChiHai_QLSV.Server.Entities
{
    [Table("CourseSection")]
    public class CourseSection
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SectionID { get; set; }

        [Required]
        [StringLength(10)]
        public string CourseID { get; set; } = null!;

        [Required] // Khóa ngoại liên kết bắt buộc
        public int SemesterID { get; set; }

        [Required]
        [StringLength(10)]
        public string ClassSection { get; set; } = "L01";

        // Chuyển thành int? (nullable) vì trong SQL cột này không có ràng buộc NOT NULL
        public int? MaxCapacity { get; set; }

        [StringLength(20)]
        public string? Status { get; set; } = "Open";


        [ForeignKey(nameof(CourseID))]
        public virtual Course Course { get; set; } = null!;

        // Bổ sung: Liên kết ngược về bảng Semester để dễ dàng truy vấn thông tin học kỳ
        [ForeignKey(nameof(SemesterID))]
        public virtual Semester Semester { get; set; } = null!;

        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
