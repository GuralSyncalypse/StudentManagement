namespace LuongChiHai_QLSV.Server.DTOs
{
    public class CourseSectionDto
    {
        public required int SectionID { get; set; }
        public required string CourseID { get; set; }
        public string? CourseName { get; set; }

        // Thay đổi: Dùng SemesterID thay cho Semester kiểu INT cũ
        public required int SemesterID { get; set; }

        // Bổ sung thêm các trường bổ trợ để Client (UI) hiển thị trực quan
        public byte SemesterNo { get; set; } // Ví dụ: 1, 2, 3
        public int StartYear { get; set; }    // Ví dụ: 2026

        // Thuộc tính tự động tính toán Niên khóa (Ví dụ: "2026-2027")
        public string AcademicYear => $"{StartYear}-{StartYear + 1}";

        // Tên hiển thị thân thiện (Ví dụ: "Học kỳ I" hoặc "Học kỳ hè")
        public string? SemesterDisplayName { get; set; }

        public string? ClassSection { get; set; }

        // Thay đổi: int? để đồng bộ với Database và Entity (cho phép rỗng)
        public int? MaxCapacity { get; set; }

        public string? Status { get; set; }
        public int CurrentEnrollment { get; set; }

        // Thuộc tính kiểm tra sinh viên hiện tại đã đăng ký lớp này chưa
        public bool IsEnrolled { get; set; }
    }

    public class CreateUpdateSectionDto
    {
        public required string CourseID { get; set; }

        // Thay đổi: Nhận vào ID của học kỳ để tạo lớp
        public required int SemesterID { get; set; }

        public string? ClassSection { get; set; } = "L01";

        // Thay đổi: int? để đồng bộ
        public int? MaxCapacity { get; set; }

        public string Status { get; set; } = "Open";
    }
}