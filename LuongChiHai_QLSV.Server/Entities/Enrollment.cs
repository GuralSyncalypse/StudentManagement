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

        // BỔ SUNG THÊM 2 THUỘC TÍNH MỚI ĐỂ PHÙ HỢP VỚI CẤU TRÚC DB CẢI TIẾN
        [Required]
        [StringLength(10)]
        public string CourseID { get; set; } = null!;

        [Required]
        public int SemesterID { get; set; }

        public DateTime? EnrollDate { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey(nameof(StudentID))]
        public virtual Student Student { get; set; } = null!;

        // LƯU Ý: Đã bỏ thuộc tính [ForeignKey(nameof(SectionID))] ở đây
        // Mối quan hệ khóa ngoại tổ hợp (SectionID, CourseID, SemesterID) 
        // sẽ được cấu hình bắt buộc thông qua Fluent API trong DbContext.
        public virtual CourseSection CourseSection { get; set; } = null!;

        public virtual ICollection<Score> Scores { get; set; } = new List<Score>();
    }
}