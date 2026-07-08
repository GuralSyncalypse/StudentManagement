using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.DTOs;
using LuongChiHai_QLSV.Server.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        public DashboardsController(SchoolContext context)
        {
            _context = context;
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
            // 1. Lấy StudentID (Dạng chuỗi mã SV, ví dụ: "SV001") từ Token
            var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                         ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(studentId))
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin sinh viên!" });
            }

            // 2. Truy vấn toàn bộ bản ghi từ View v_BangDiemChiTiet
            var studentPoints = await _context.BangDiemChiTiet
                .Where(v => v.StudentID == studentId)
                .ToListAsync();

            decimal gpa = 0;
            int totalPassedCredits = 0;

            // 🔥 GIẢI PHÁP: Chỉ lọc ra những môn ĐÃ CÓ ĐIỂM để tính GPA tích lũy
            var gradedPoints = studentPoints.Where(p => p.TotalScore.HasValue).ToList();

            if (gradedPoints.Any())
            {
                // Công thức chuẩn: Tính tổng điểm hệ số trên các môn ĐÃ CÓ ĐIỂM
                decimal totalWeightScore = gradedPoints.Sum(p => (decimal)p.TotalScore!.Value * p.Credits);
                int totalCreditsForGpa = gradedPoints.Sum(p => p.Credits);

                gpa = totalCreditsForGpa > 0 ? Math.Round(totalWeightScore / totalCreditsForGpa, 2) : 0;

                // Tín chỉ tích lũy (Chỉ tính các môn đã qua và có kết quả là 'Đỗ')
                totalPassedCredits = gradedPoints.Where(p => p.GradeDetails.IsPassed()).Sum(p => p.Credits);
            }

            // 4. Đếm số học phần sinh viên này đang học trong học kỳ hiện tại (Chuỗi)
            var currentSemester = await _context.CourseSections
                .OrderByDescending(cs => cs.Semester)
                .Select(cs => cs.Semester)
                .FirstOrDefaultAsync();

            var currentSectionsCount = await _context.Enrollments
                .CountAsync(e => e.StudentID == studentId);

            // 5. Trả về kết quả sạch sẽ cho Dashboard Angular
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
