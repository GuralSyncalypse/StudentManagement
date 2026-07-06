namespace LuongChiHai_QLSV.Server.DTOs
{
    public class AdminRegistrationDto
    {
        public int SectionID { get; set; }
        public required string StudentID { get; set; }
    }

    public class StudentRegistrationDto
    {
        public int SectionID { get; set; }
    }
}
