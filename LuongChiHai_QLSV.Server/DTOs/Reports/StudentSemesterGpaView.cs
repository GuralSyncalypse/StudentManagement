using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuongChiHai_QLSV.Server.DTOs.Reports
{
    public class StudentSemesterGpaView
    {
        public string StudentID { get; set; } = string.Empty;

        // --- ĐIỀU CHỈNH CẤU TRÚC HỌC KỲ MỚI ---
        public int SemesterID { get; set; }
        public byte SemesterNo { get; set; }
        public int StartYear { get; set; }
        // --------------------------------------

        public decimal? SemesterGPA { get; set; }
        public int TotalCreditsRegistered { get; set; }
        public decimal? CumulativeGPA { get; set; }
        public int? TotalAccumulatedCredits { get; set; }
    }
}