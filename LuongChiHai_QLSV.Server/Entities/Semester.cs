using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuongChiHai_QLSV.Server.Entities
{
    [Table("Semester")]
    public class Semester
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SemesterID { get; set; }

        [Required]
        public byte SemesterNo { get; set; } // TINYINT trong SQL ánh xạ thành byte trong C#

        [Required]
        public int StartYear { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } // Nếu dùng .NET 6+ bạn có thể thay bằng DateOnly

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? RegistrationStartDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime? RegistrationEndDate { get; set; }
        public bool IsRegistrationEnabled { get; set; } = true;

        // Mối quan hệ 1 - Nhiều: Một học kỳ có thể mở nhiều Lớp học phần (CourseSection)
        public virtual ICollection<CourseSection> CourseSections { get; set; } = new List<CourseSection>();
    }
}
