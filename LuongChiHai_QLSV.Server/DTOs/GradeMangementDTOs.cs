namespace LuongChiHai_QLSV.Server.DTOs
{
    public class EnrollmentDto
    {
        public int EnrollmentID { get; set; }
        public string StudentID { get; set; } = null!;
        public int SectionID { get; set; }
        public DateTime? EnrollDate { get; set; }

        public List<ScoreDto> Scores { get; set; } = new List<ScoreDto>();
    }

    public class ScoreDto
    {
        public int ScoreID { get; set; }
        public string? ScoreType { get; set; }
        public decimal? ScoreValue { get; set; }
        public decimal? Weight { get; set; }
    }
}
