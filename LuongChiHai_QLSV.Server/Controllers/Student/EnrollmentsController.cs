using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.DTOs;
using LuongChiHai_QLSV.Server.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LuongChiHai_QLSV.Server.Controllers.Student
{
    [Route("api/student/[controller]")]
    [ApiController]
    [Authorize(Roles = "Student")]
    public class StudentEnrollmentsController : ControllerBase
    {
        private readonly SchoolContext _context;

        public StudentEnrollmentsController(SchoolContext context)
        {
            _context = context;
        }

        // GET: api/student/enrollments (Sinh viên chỉ xem được các lớp MÌNH đã đăng ký)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetMyEnrollments()
        {
            // Lấy StudentID từ JWT Token đã đăng nhập
            var studentIdClaim = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(studentIdClaim)) return Unauthorized("Không tìm thấy thông tin sinh viên.");

            string studentId = studentIdClaim;

            var myEnrollments = await _context.Enrollments
                .Where(e => e.StudentID == studentId)
                .Select(e => new EnrollmentDto
                {
                    EnrollmentID = e.EnrollmentID,
                    StudentID = e.StudentID,
                    SectionID = e.SectionID,
                    EnrollDate = e.EnrollDate,
                    Scores = e.Scores.Select(s => new ScoreDto
                    {
                        ScoreID = s.ScoreID,
                        ScoreType = s.ScoreType,
                        ScoreValue = s.ScoreValue
                    }).ToList()
                }).ToListAsync();

            return Ok(myEnrollments);
        }
    }
}
