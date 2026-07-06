using LuongChiHai_QLSV.Server.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuongChiHai_QLSV.Server.Entities
{
    public class Student
    {
        public string StudentID { get; set; } = null!;
        public int UserID { get; set; }
        public string StudentName { get; set; } = null!;
        public string? Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Ethnicity { get; set; }
        public string? Religion { get; set; }
        public string? Nationality { get; set; }
        public string? BirthPlace { get; set; }
        public string? CitizenID { get; set; }
        public DateTime? CitizenIDIssueDate { get; set; }
        public string? CitizenIDIssuePlace { get; set; }
        public string? PermanentAddress { get; set; }
        public string? TemporaryAddress { get; set; }

        // Navigation Property
        public User User { get; set; } = null!;
        public AcademicProfile? AcademicProfile { get; set; } = null!; // 1-1
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
