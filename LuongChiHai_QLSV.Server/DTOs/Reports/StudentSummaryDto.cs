// StudentSummaryDto.cs
using System.Collections.Generic;

namespace LuongChiHai_QLSV.Server.DTOs.Reports
{
    public class StudentSummaryDto
    {
        public string StudentID { get; set; } = null!;

        // --- ĐIỀU CHỈNH CẤU TRÚC HỌC KỲ MỚI ---
        public int SemesterID { get; set; }
        public byte SemesterNo { get; set; }
        public int StartYear { get; set; }
        // --------------------------------------

        public int TotalEnrollment { get; set; }
        public List<StudentCourseGradeDto> CoursesDetail { get; set; } = new();
    }
}   