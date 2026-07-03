namespace LuongChiHai_QLSV.Server.DTOs.Reports
{
    public class StudentCourseGradeDto
    {
        public string StudentID { get; set; } = null!;
        public int EnrollmentID { get; set; }
        public int SectionID { get; set; }
        public int Semester { get; set; }
        public string CourseID { get; set; } = string.Empty;
        public string CourseName { get; set; } = null!;
        public int Credits { get; set; }
        public decimal? TotalScore { get; set; }
        public string Result { get; set; } = null!; 
    }
}
