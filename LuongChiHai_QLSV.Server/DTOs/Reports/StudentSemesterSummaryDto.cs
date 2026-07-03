namespace LuongChiHai_QLSV.Server.DTOs.Reports
{
    public class StudentSemesterSummaryDto
    {
        public string StudentID { get; set; } = null!;
        public int Semester { get; set; }
        public int TongSoMonDaHoc { get; set; }
        public int SoMonDaQua { get; set; }
        public int SoMonTruot { get; set; }
        public decimal DiemTrungBinhHocKy { get; set; }

        // Gom chung danh sách môn học vào đây để Frontend Angular chỉ cần gọi 1 API duy nhất
        public List<StudentCourseGradeDto> ChiTietMonHoc { get; set; } = new();
    }
}
