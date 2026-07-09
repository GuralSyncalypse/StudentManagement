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
    }
}
