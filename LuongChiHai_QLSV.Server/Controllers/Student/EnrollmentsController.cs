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

        // POST: api/student/enrollments (Sinh viên TỰ ĐĂNG KÝ vào một lớp học phần)
        [HttpPost]
        public async Task<IActionResult> RegisterCourse([FromBody] StudentRegistrationDto request)
        {
            // 1. Lấy mã sinh viên tự động từ Token bảo mật
            var studentIdClaim = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(studentIdClaim)) return Unauthorized();
            string studentId = studentIdClaim;

            // 2. Kiểm tra xem lớp học phần (Section) này có tồn tại không
            var sectionExists = await _context.CourseSections.AnyAsync(s => s.SectionID == request.SectionID);
            if (!sectionExists) return BadRequest("Lớp học phần không tồn tại.");

            // 3. Kiểm tra xem sinh viên đã đăng ký lớp này từ trước chưa (Tránh trùng lặp)
            var alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.StudentID == studentId && e.SectionID == request.SectionID);
            if (alreadyEnrolled) return BadRequest("Bạn đã đăng ký học phần này rồi.");

            // 4. Tiến hành lưu thông tin đăng ký
            var newEnrollment = new Enrollment
            {
                StudentID = studentId,
                SectionID = request.SectionID,
                EnrollDate = DateTime.Now
            };

            _context.Enrollments.Add(newEnrollment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMyEnrollments), new { id = newEnrollment.EnrollmentID }, "Đăng ký học phần thành công.");
        }
    }
}
