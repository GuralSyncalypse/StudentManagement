using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.DTOs;
using LuongChiHai_QLSV.Server.Entities;
using LuongChiHai_QLSV.Server.Interfaces;
using LuongChiHai_QLSV.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LuongChiHai_QLSV.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StudentRegistrationsController : ControllerBase
    {
        private readonly SchoolContext _context;
        private readonly IEnrollmentService _enrollmentService;

        public StudentRegistrationsController(SchoolContext context, IEnrollmentService enrollmentService)
        {
            _context = context;
            _enrollmentService = enrollmentService;
        }

        [HttpGet]
        // Bạn có thể thêm [HasPermission("...")] tại đây nếu cần
        public async Task<ActionResult<IEnumerable<CourseSectionDto>>> GetAvailableSections()
        {
            // Lấy mã sinh viên từ Claim của người dùng đang đăng nhập
            var studentIdClaim = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(studentIdClaim))
            {
                return Unauthorized("Không xác thực được sinh viên.");
            }

            var data = await _context.CourseSections
                .AsNoTracking()
                .Where(s => s.Status == "Open")
                .Select(s => new CourseSectionDto
                {
                    SectionID = s.SectionID,
                    CourseID = s.CourseID,
                    CourseName = s.Course != null ? s.Course.CourseName : null,

                    // Map thông tin từ bảng Semester mới
                    SemesterID = s.SemesterID,
                    SemesterNo = s.Semester.SemesterNo,
                    StartYear = s.Semester.StartYear,
                    SemesterDisplayName = s.Semester.SemesterNo == 1 ? "Học kỳ I" :
                                          s.Semester.SemesterNo == 2 ? "Học kỳ II" :
                                          s.Semester.SemesterNo == 3 ? "Học kỳ hè" : "Không xác định",

                    ClassSection = s.ClassSection,
                    MaxCapacity = s.MaxCapacity,
                    Status = s.Status,
                    CurrentEnrollment = s.Enrollments.Count,

                    // Giữ nguyên logic kiểm tra xem sinh viên hiện tại đã đăng ký lớp này chưa
                    IsEnrolled = s.Enrollments.Any(e => e.StudentID == studentIdClaim)
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterCourse([FromBody] StudentRegistrationDto request)
        {
            var studentId = GetStudentId();
            if (studentId == null)
            {
                return Unauthorized("Không xác thực được sinh viên.");
            }

            var result = await _enrollmentService.RegisterCourseAsync(studentId, request);
            return Ok(new
            {
                Message = "Đăng ký học phần thành công.",
                result.EnrollmentID
            });
        }

        [HttpDelete("{sectionId}")]
        public async Task<IActionResult> DropCourse(int sectionId)
        {
            var studentId = GetStudentId();
            if (studentId == null)
            {
                return Unauthorized("Không xác thực được sinh viên.");
            }

            try
            {
                await _enrollmentService.CancelCourseAsync(studentId, sectionId);
                return Ok(new { Message = "Hủy đăng ký học phần thành công." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private string? GetStudentId()
        {
            return User.FindFirst("StudentID")?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value;
        }
    }
}
