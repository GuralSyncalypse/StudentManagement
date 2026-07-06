namespace LuongChiHai_QLSV.Server.DTOs.Reports
{
    public class StudentSummaryDto
    {
        public string StudentID { get; set; } = null!;
        public int Semester { get; set; }
        public int TotalEnrollment { get; set; }

        public List<StudentCourseGradeDto> CoursesDetail { get; set; } = new();
    }
}
