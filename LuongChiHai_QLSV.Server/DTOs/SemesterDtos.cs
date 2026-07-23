using System.ComponentModel.DataAnnotations;

namespace LuongChiHai_QLSV.Server.DTOs
{
    public class CourseSectionBaseDto
    {
        public required int SectionID { get; set; }
        public required string CourseID { get; set; }
    }

    public class ToggleRegistrationDto
    {
        public bool IsRegistrationEnabled { get; set; }
    }

    public class CreateSemesterDto
    {
        [Required(ErrorMessage = "Số học kỳ là bắt buộc")]
        [Range(1, 10, ErrorMessage = "Số học kỳ phải từ 1 đến 10")]
        public byte SemesterNo { get; set; }

        [Required(ErrorMessage = "Năm học bắt đầu là bắt buộc")]
        [Range(2000, 2100, ErrorMessage = "Năm học không hợp lệ")]
        public int StartYear { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu học kỳ là bắt buộc")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc học kỳ là bắt buộc")]
        public DateTime EndDate { get; set; }

        public DateTime? RegistrationStartDate { get; set; }
        public DateTime? RegistrationEndDate { get; set; }
        public bool IsRegistrationEnabled { get; set; } = true;
    }

    // DTO cho Semester
    public class SemesterDto
    {
        public int SemesterID { get; set; }
        public byte SemesterNo { get; set; }
        public int StartYear { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? RegistrationStartDate { get; set; }
        public DateTime? RegistrationEndDate { get; set; }
        public bool IsRegistrationEnabled { get; set; }
        public List<CourseSectionBaseDto> CourseSections { get; set; } = new();
    }
}
