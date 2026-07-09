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
        public async Task<ActionResult<IEnumerable<CourseSectionDto>>> GetAvailableSections()
        {
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
                    Semester = s.Semester,
                    ClassSection = s.ClassSection,
                    MaxCapacity = s.MaxCapacity,
                    Status = s.Status,
                    CourseName = s.Course != null ? s.Course.CourseName : null,
                    CurrentEnrollment = s.Enrollments.Count,
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

            try
            {
                var result = await _enrollmentService.RegisterCourseAsync(studentId, request);
                return Ok(new
                {
                    Message = "Đăng ký học phần thành công.",
                    result.EnrollmentID
                });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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
            catch (NotFoundException ex)
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
