using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuongChiHai_QLSV.Server.Entities
{
    [Table("AcademicProfile")]
    public class AcademicProfile
    {
        [Key]
        [ForeignKey("Student")]
        [Column("StudentID", TypeName = "varchar(15)")]
        public string StudentID { get; set; } = string.Empty;

        public DateTime? AdmissionDate { get; set; } = null!;

        [StringLength(50)]
        public string? ClassName { get; set; } = null!;

        [StringLength(100)]
        public string? CampusName { get; set; } = null!;

        [StringLength(50)]
        public string? EducationLevel { get; set; } = null!;
        [StringLength(50)]
        public string? EducationType { get; set; } = null!;

        [StringLength(100)]
        public string? FacultyName { get; set; } = null!;

        [StringLength(100)]
        public string? MajorName { get; set; } = null!;

        [StringLength(100)]
        public string? SpecializationName { get; set; } = null!;

        public int? AcademicYear { get; set; } = null!;

        public string Status { get; set; } = "Đang học"; // 'Đang học', 'Bảo lưu', 'Đã tốt nghiệp', 'Thôi học'


        // Navigation property ngược lại Student
        public Student Student { get; set; } = null!;
    }
}
