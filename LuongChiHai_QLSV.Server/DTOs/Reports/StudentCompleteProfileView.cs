namespace LuongChiHai_QLSV.Server.DTOs.Reports
{
    public class StudentCompleteProfileView
    {
        public string StudentID { get; set; } = string.Empty;

        public string StudentName { get; set; } = string.Empty;

        public string? Gender { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? CitizenID { get; set; }

        public string? PermanentAddress { get; set; }

        public string? ClassName { get; set; }

        public string? FacultyName { get; set; }

        public string? MajorName { get; set; }

        public string? AcademicYear { get; set; }

        public string? AcademicStatus { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public bool IsAccountActive { get; set; }
    }
}
