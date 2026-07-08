using System.Text.Json.Serialization;

namespace LuongChiHai_QLSV.Server.DTOs.Reports
{
    public class GradeEnum
    {
        // Định nghĩa các trạng thái dựa trên mã số từ Database
        public static readonly GradeEnum A = new GradeEnum(5, "A", 4.0m, "Đạt", isGraded: true);
        public static readonly GradeEnum B = new GradeEnum(4, "B", 3.0m, "Đạt", isGraded: true);
        public static readonly GradeEnum C = new GradeEnum(3, "C", 2.0m, "Đạt", isGraded: true);
        public static readonly GradeEnum D = new GradeEnum(2, "D", 1.0m, "Đạt", isGraded: true);
        public static readonly GradeEnum F = new GradeEnum(1, "F", 0.0m, "Trượt", isGraded: true);
        public static readonly GradeEnum Pending = new GradeEnum(-1, "-", 0.0m, "Chưa có điểm", false);

        // Đăng ký Dictionary để tra cứu O(1) bằng mã số
        private static readonly Dictionary<int, GradeEnum> Registry = new()
        {
            { A.Id, A }, { B.Id, B }, { C.Id, C }, { D.Id, D }, { F.Id, F }
        };

            public int Id { get; }            // Khớp với GradeCode từ View
            public string Code { get; }       // A, B, C, D, F
            public decimal Point4 { get; }    // Điểm hệ 4 để tính GPA
            public string Result { get; }     // Đạt / Trượt
            public bool IsGraded { get; }

            private GradeEnum(int id, string code, decimal point4, string result, bool isGraded)
            {
                Id = id;
                Code = code;
                Point4 = point4;
                Result = result;
                IsGraded = isGraded;
            }

            // Hàm chuyển đổi O(1) từ mã số sang thực thể Rich Object
            public static GradeEnum FromCode(int? gradeCode)
            {
                if (gradeCode == null) return Pending;
                return Registry.TryGetValue(gradeCode.Value, out var grade) ? grade : Pending;
            }

            public bool IsPassed() => Id >= 2; // Từ điểm D (mã 2) trở lên là Đạt
            public bool IsFailed() => Id == 1; // Mã 1 là Trượt
        }

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

        // Hứng trực tiếp mã số (1, 2, 3, 4, 5 hoặc null) từ SQL View
        [JsonIgnore]
        public int? GradeCode { get; set; }

        // Các thuộc tính tính toán tự động map từ Smart Enum ra cho Frontend dùng
        public string Grade => GradeDetails.Code;    // Trả về: "A", "B", "C",...
        public string Result => GradeDetails.Result;  // Trả về: "Đạt", "Trượt", "Chưa có điểm"

        // Xử lý logic nội bộ ở Backend
        private GradeEnum? _gradeDetails;

        [JsonIgnore]
        public GradeEnum GradeDetails => _gradeDetails ?? GradeEnum.FromCode(this.GradeCode);
    }
}
