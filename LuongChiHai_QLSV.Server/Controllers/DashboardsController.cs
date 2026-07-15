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


        public class KpiDataDto
        {
            public decimal CumulativeGpa { get; set; }
            public int TotalAccumulatedCredits { get; set; }
            public string AcademicStanding { get; set; } = "";
        }

        [HttpGet("student")]
        public async Task<IActionResult> GetStudentDashboard()
        {
            // 1. Lấy StudentID từ Token JWT
            var studentId = User.FindFirst(ClaimTypes.Name)?.Value
                         ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(studentId))
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin sinh viên hợp lệ!" });
            }

            // 2. Lấy học kỳ hiện tại đang mở (Lấy kỳ lớn nhất trong hệ thống)
            var currentSemester = await _context.CourseSections
                .OrderByDescending(cs => cs.Semester)
                .Select(cs => cs.Semester)
                .FirstOrDefaultAsync();

            if (currentSemester == 0)
            {
                return NotFound(new { message = "Hệ thống chưa thiết lập học kỳ hiện tại!" });
            }

            // Thẻ thông tin cá nhân
            var profileData = await _context.StudentCompleteProfiles
                .Where(p => p.StudentID == studentId)
                .Select(p => new
                {
                    StudentId = p.StudentID,
                    StudentName = p.StudentName,
                    ClassName = p.ClassName,
                    MajorName = p.MajorName,
                    FacultyName = p.FacultyName,
                    AcademicStatus = p.AcademicStatus,
                    Email = p.Email,
                    PhoneNumber = p.PhoneNumber
                })
                .FirstOrDefaultAsync();

            if (profileData == null)
            {
                return NotFound(new { message = "Hồ sơ sinh viên không tồn tại!" });
            }

            // Thẻ KPI tổng kết (Dùng View Cumulative GPA)
            var kpiData = await _context.StudentCumulativeGpas
                .Where(k => k.StudentID == studentId)
                .Select(k => new KpiDataDto
                {
                    CumulativeGpa = k.CumulativeGPA ?? 0,
                    TotalAccumulatedCredits = k.TotalAccumulatedCredits,
                    AcademicStanding =
                        k.CumulativeGPA >= 8.5m ? "Xuất sắc" :
                        k.CumulativeGPA >= 7.0m ? "Giỏi" :
                        k.CumulativeGPA >= 5.5m ? "Khá" :
                        k.CumulativeGPA >= 4.0m ? "Trung bình" :
                        "Yếu/Kém"
                })
                .FirstOrDefaultAsync()
                ?? new KpiDataDto
                {
                    CumulativeGpa = 0,
                    TotalAccumulatedCredits = 0,
                    AcademicStanding = "Chưa có dữ liệu"
                };
                

            // Danh sách môn học kỳ này (Lọc theo StudentID và Kỳ hiện tại)
            var currentCourses = await _context.BangDiemChiTiet
                .Where(v => v.StudentID == studentId && v.Semester == currentSemester)
                .Select(v => new
                {
                    CourseId = v.CourseID,
                    CourseName = v.CourseName,
                    Credits = v.Credits,
                    CurrentScore = v.TotalScore.HasValue ? v.TotalScore.Value.ToString("0.0") : "N/A",
                    LetterGrade = v.GradeCode == 5 ? "A" :
                                  v.GradeCode == 4 ? "B" :
                                  v.GradeCode == 3 ? "C" :
                                  v.GradeCode == 2 ? "D" :
                                  v.GradeCode == 1 ? "F" : "Chưa có"
                })
                .ToListAsync();

            // Lịch sử GPA từng kỳ phục vụ vẽ biểu đồ đường (Sort theo kỳ tăng dần từ cũ đến mới)
            var gpaHistory = await _context.StudentSemesterGpas
                .Where(g => g.StudentID == studentId)
                .OrderBy(g => g.Semester)
                .Select(g => new
                {
                    Semester = g.Semester,
                    SemesterGpa = g.SemesterGPA,
                    CumulativeGpa = g.CumulativeGPA
                })
                .ToListAsync();

            // 4. Trả về cấu trúc object "phẳng" bọc gọn gàng, khớp hoàn toàn Model Angular frontend
            return Ok(new
            {
                Profile = profileData,
                Kpis = kpiData,
                Courses = currentCourses,
                GpaHistory = gpaHistory
            });
        }
    }
}
