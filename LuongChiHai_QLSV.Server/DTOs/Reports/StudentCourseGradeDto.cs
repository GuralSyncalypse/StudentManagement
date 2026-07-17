using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LuongChiHai_QLSV.Server.DTOs.Reports
{
    public class GradeEnum
    {
        public static readonly GradeEnum A = new GradeEnum(5, "A", 4.0m, "Đạt", isGraded: true);
        public static readonly GradeEnum B = new GradeEnum(4, "B", 3.0m, "Đạt", isGraded: true);
        public static readonly GradeEnum C = new GradeEnum(3, "C", 2.0m, "Đạt", isGraded: true);
        public static readonly GradeEnum D = new GradeEnum(2, "D", 1.0m, "Đạt", isGraded: true);
        public static readonly GradeEnum F = new GradeEnum(1, "F", 0.0m, "Trượt", isGraded: true);
        public static readonly GradeEnum Pending = new GradeEnum(-1, "-", 0.0m, "Chưa có điểm", false);

        private static readonly Dictionary<int, GradeEnum> Registry = new()
        {
            { A.Id, A }, { B.Id, B }, { C.Id, C }, { D.Id, D }, { F.Id, F }
        };

        public int Id { get; }
        public string Code { get; }
        public decimal Point4 { get; }
        public string Result { get; }
        public bool IsGraded { get; }

        private GradeEnum(int id, string code, decimal point4, string result, bool isGraded)
        {
            Id = id;
            Code = code;
            Point4 = point4;
            Result = result;
            IsGraded = isGraded;
        }

        public static GradeEnum FromCode(int? gradeCode)
        {
            if (gradeCode == null) return Pending;
            return Registry.TryGetValue(gradeCode.Value, out var grade) ? grade : Pending;
        }

        public bool IsPassed() => Id >= 2;
        public bool IsFailed() => Id == 1;
    }

    public class StudentCourseGradeDto
    {
        public string StudentID { get; set; } = null!;
        public int EnrollmentID { get; set; }
        public int SectionID { get; set; }

        // --- ĐIỀU CHỈNH CẤU TRÚC HỌC KỲ MỚI ---
        public int SemesterID { get; set; }
        public byte SemesterNo { get; set; }
        public int StartYear { get; set; }
        public string AcademicYear => $"{StartYear}-{StartYear + 1}";
        // --------------------------------------

        public string CourseID { get; set; } = string.Empty;
        public string CourseName { get; set; } = null!;
        public int Credits { get; set; }
        public decimal? TotalScore { get; set; }

        [JsonIgnore]
        public int? GradeCode { get; set; }

        public string Grade => GradeDetails.Code;
        public string Result => GradeDetails.Result;

        private GradeEnum? _gradeDetails;

        [JsonIgnore]
        public GradeEnum GradeDetails => _gradeDetails ?? GradeEnum.FromCode(this.GradeCode);
    }
}