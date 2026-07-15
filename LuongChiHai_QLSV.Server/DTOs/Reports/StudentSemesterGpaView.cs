using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuongChiHai_QLSV.Server.DTOs.Reports
{
    public class StudentSemesterGpaView
    {
        public string StudentID { get; set; } = string.Empty;

        public int Semester { get; set; }

        public decimal? SemesterGPA { get; set; }

        public int TotalCreditsRegistered { get; set; }

        // --- BỔ SUNG CÁC TRƯỜNG MỚI DƯỚI ĐÂY ---

        /// <summary>
        /// Điểm trung bình tích lũy (CPA) tính đến hết học kỳ này (Thang điểm 10)
        /// </summary>
        public decimal? CumulativeGPA { get; set; }

        /// <summary>
        /// Tổng số tín chỉ tích lũy đạt được (đã có điểm) tính đến hết kỳ này
        /// </summary>
        public int? TotalAccumulatedCredits { get; set; }
    }
}
