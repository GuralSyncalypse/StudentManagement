// StudentSemesterSummaryDto.cs
using System.Collections.Generic;

namespace LuongChiHai_QLSV.Server.DTOs.Reports
{
    public class StudentSemesterSummaryDto
    {
        public string StudentID { get; set; } = null!;
        
        // --- ĐIỀU CHỈNH CẤU TRÚC HỌC KỲ MỚI ---
        public int SemesterID { get; set; }
        public byte SemesterNo { get; set; }
        public int StartYear { get; set; }
        public string AcademicYear => $"{StartYear}-{StartYear + 1}";
        // --------------------------------------

        public int TongSoMonDaHoc { get; set; }
        public int SoMonDaQua { get; set; }
        public int SoMonTruot { get; set; }
        public decimal DiemTrungBinhHocKy { get; set; }

        public List<StudentCourseGradeDto> ChiTietMonHoc { get; set; } = new();
    }
}