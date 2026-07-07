using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.DTOs;
using LuongChiHai_QLSV.Server.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LuongChiHai_QLSV.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentRegistrationsController : ControllerBase
    {
        private readonly SchoolContext _context;
        public StudentRegistrationsController(SchoolContext context)
        {
            _context = context;
        }

        // GET: api/student/course-sections
        // Sinh viên chỉ xem danh sách các lớp ĐANG MỞ (Open) và CHƯA BỊ ĐẦY SĨ SỐ
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseSectionDto>>> GetAvailableSections()
        {
            // 1. Lấy StudentID từ JWT Token của sinh viên đang đăng nhập
            var studentIdClaim = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(studentIdClaim)) return Unauthorized("Không xác thực được sinh viên.");
            string studentId = studentIdClaim;

            // 2. Lấy danh sách lớp và check trạng thái đăng ký của sinh viên này
            var data = await _context.CourseSections
                .Where(s => s.Status == "Open") // Chỉ lấy các lớp đang mở đợt đăng ký
                .Select(s => new CourseSectionDto
                {
                    SectionID = s.SectionID,
                    CourseID = s.CourseID,
                    Semester = s.Semester,
                    ClassSection = s.ClassSection,
                    MaxCapacity = s.MaxCapacity,
                    Status = s.Status,
                    CourseName = s.Course != null ? s.Course.CourseName : null,
                    CurrentEnrollment = s.Enrollments.Count,

                    // Kiểm tra xem sinh viên hiện tại đã nằm trong lớp này chưa
                    IsEnrolled = s.Enrollments.Any(e => e.StudentID == studentId)
                })
                .ToListAsync();

            return Ok(data);
        }

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

            return Ok(new
            {
                Message = "Đăng ký học phần thành công.",
                EnrollmentID = newEnrollment.EnrollmentID
            });
        }

        [HttpDelete("{sectionId}")]
        public async Task<IActionResult> DropCourse(int sectionId)
        {
            // 1. Lấy StudentID từ Token người dùng đăng nhập
            var studentIdClaim = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(studentIdClaim)) return Unauthorized("Không xác thực được sinh viên.");
            string studentId = studentIdClaim;

            // 2. Tìm bản ghi đăng ký trong Database
            var enrollment = await _context.Enrollments
                .Include(e => e.CourseSection)
                .FirstOrDefaultAsync(e => e.SectionID == sectionId && e.StudentID == studentId);

            if (enrollment == null)
            {
                return NotFound("Bạn chưa đăng ký lớp học phần này.");
            }

            // 3. KIỂM TRA NGHIỆP VỤ: Lớp có đang mở cho phép hủy không?
            if (enrollment.CourseSection.Status != "Open")
            {
                return BadRequest("Học phần này đã đóng đợt chỉnh sửa, không thể tự ý hủy.");
            }

            // 4. Tiến hành xóa dữ liệu (Giải phóng 1 suất học)
            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Hủy đăng ký học phần thành công." });
        }
    }
}
