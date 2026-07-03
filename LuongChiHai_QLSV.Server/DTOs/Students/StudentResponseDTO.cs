namespace LuongChiHai_QLSV.Server.DTOs.Students
{
    public class StudentResponseDto
    {
        public string StudentID { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public string? Ethnicity { get; set; }
        public string? PermanentAddress { get; set; }

        // Thông tin liên quan
        public AcademicProfileDto? AcademicProfile { get; set; }
    }

    public class AcademicProfileDto
    {
        public string? ClassName { get; set; } = string.Empty;
        public string? FacultyName { get; set; } = string.Empty;
        public string? MajorName { get; set; } = string.Empty;
    }
}
