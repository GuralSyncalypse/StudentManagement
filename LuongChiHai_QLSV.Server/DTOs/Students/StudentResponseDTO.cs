namespace LuongChiHai_QLSV.Server.DTOs.Students
{
    public class StudentListDto
    {
        public string StudentID { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public string? Ethnicity { get; set; }
        public string? PermanentAddress { get; set; }
        public AcademicProfileResponseDto? AcademicProfile { get; set; }
    }

    public class StudentDetailDto : StudentBaseDto
    {
        public string StudentID { get; set; } = string.Empty;
        public int UserID { get; set; } = 0;
        public AcademicProfileResponseDto? AcademicProfile { get; set; }
    }

    public class AcademicProfileResponseDto
    {
        public string? ClassName { get; set; } = string.Empty;
        public string? FacultyName { get; set; } = string.Empty;
        public string? MajorName { get; set; } = string.Empty;
    }
}
