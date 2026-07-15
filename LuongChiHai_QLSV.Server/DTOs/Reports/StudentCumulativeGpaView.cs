using System.ComponentModel.DataAnnotations;

namespace LuongChiHai_QLSV.Server.DTOs.Reports
{
    public class StudentCumulativeGpaView
    {
        public string StudentID { get; set; } = string.Empty;

        public decimal? CumulativeGPA { get; set; }

        public int TotalAccumulatedCredits { get; set; }
    }
}
