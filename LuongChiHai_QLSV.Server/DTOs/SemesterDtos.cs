namespace LuongChiHai_QLSV.Server.DTOs
{
    public class CourseSectionBaseDto
    {
        public required int SectionID { get; set; }
        public required string CourseID { get; set; }
    }

    public class ToggleRegistrationDto
    {
        public bool IsRegistrationEnabled { get; set; }
    }

    // DTO cho Semester
    public class SemesterDto
    {
        public int SemesterID { get; set; }
        public byte SemesterNo { get; set; }
        public int StartYear { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? RegistrationStartDate { get; set; }
        public DateTime? RegistrationEndDate { get; set; }
        public bool IsRegistrationEnabled { get; set; }
        public List<CourseSectionBaseDto> CourseSections { get; set; } = new();
    }
}
