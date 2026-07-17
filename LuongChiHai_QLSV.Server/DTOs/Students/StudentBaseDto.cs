using System.ComponentModel.DataAnnotations;

namespace LuongChiHai_QLSV.Server.DTOs.Students
{
    public abstract class StudentBaseDto
    {
        [Required(ErrorMessage = "Tên sinh viên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên sinh viên không quá 100 ký tự")]
        public string StudentName { get; set; } = string.Empty;

        public string? Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Ethnicity { get; set; }
        public string? Religion { get; set; }
        public string? Nationality { get; set; }
        public string? BirthPlace { get; set; }

        [StringLength(12, MinimumLength = 9, ErrorMessage = "CMND/CCCD phải từ 9 đến 12 ký tự")]
        public string? CitizenID { get; set; }

        public DateTime? CitizenIDIssueDate { get; set; }
        public string? CitizenIDIssuePlace { get; set; }
        public string? PermanentAddress { get; set; }
        public string? TemporaryAddress { get; set; }
    }
}
