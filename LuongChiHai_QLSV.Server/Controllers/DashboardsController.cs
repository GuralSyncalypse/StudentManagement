using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LuongChiHai_QLSV.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardsController : ControllerBase
    {
        private readonly SchoolContext _context;
        private readonly IEnrollmentService _enrollmentService;

        public DashboardsController(SchoolContext context, IEnrollmentService enrollmentService)
        {
            _context = context;
            _enrollmentService = enrollmentService;
        }

        [HttpGet("admin")]
        public async Task<IActionResult> GetAdminDashboard()
        {
            var totalSections = await _context.CourseSections.CountAsync();
            var totalStudents = await _context.Students.CountAsync();
            var totalCourses = await _context.Courses.CountAsync();

            return Ok(new
            {
                TotalSections = totalSections,
                TotalStudents = totalStudents,
                TotalCourses = totalCourses
            });
        }

        [HttpGet("student")]
        public async Task<IActionResult> GetStudentDashboard()
        {
            var studentId = User.FindFirst(ClaimTypes.Name)?.Value
                         ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(studentId))
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin sinh viên!" });
            }

            var studentPoints = await _context.BangDiemChiTiet
                .Where(v => v.StudentID == studentId)
                .ToListAsync();

            decimal gpa = 0;
            int totalPassedCredits = 0;

            var gradedPoints = studentPoints.Where(p => p.TotalScore.HasValue).ToList();

            if (gradedPoints.Any())
            {
                decimal totalWeightScore = gradedPoints.Sum(p => (decimal)p.TotalScore!.Value * p.Credits);
                int totalCreditsForGpa = gradedPoints.Sum(p => p.Credits);

                gpa = totalCreditsForGpa > 0 ? Math.Round(totalWeightScore / totalCreditsForGpa, 2) : 0;
                totalPassedCredits = gradedPoints.Where(p => p.GradeDetails.IsPassed()).Sum(p => p.Credits);
            }

            var currentSemester = await _context.CourseSections
                .OrderByDescending(cs => cs.Semester)
                .Select(cs => cs.Semester)
                .FirstOrDefaultAsync();

            var currentSectionsCount = await _enrollmentService.GetCurrentEnrollmentCountAsync(studentId);

            return Ok(new
            {
                GPA = gpa,
                TotalAccumulatedCredits = totalPassedCredits,
                CurrentSectionsCount = currentSectionsCount,
                CurrentSemester = currentSemester
            });
        }
    }
}
