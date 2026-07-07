namespace LuongChiHai_QLSV.Server.DTOs
{
    public class CourseSectionDto
    {
        public required int SectionID { get; set; }
        public required string CourseID { get; set; }
        public int Semester { get; set; }
        public string? ClassSection { get; set; }
        public int MaxCapacity { get; set; }
        public string? Status { get; set; }
        public string? CourseName { get; set; }
        public int CurrentEnrollment { get; set; }
    }

    public class CreateUpdateSectionDto
    {
        public required string CourseID { get; set; }
        public int Semester { get; set; }
        public string? ClassSection { get; set; }
        public int MaxCapacity { get; set; }
        public string Status { get; set; } = "Open";
    }
}
