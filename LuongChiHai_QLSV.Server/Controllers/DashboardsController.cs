using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

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
            var studentId = User.FindFirst(ClaimTypes.Name)?.Value
                         ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(studentId))
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin sinh viên hợp lệ!" });
            }

            // 1. SỬA ĐỔI: Tìm ID học kỳ mới nhất theo Năm học giảm dần -> Số kỳ giảm dần
            var currentSemesterId = await _context.CourseSections
                .OrderByDescending(cs => cs.Semester.StartYear)
                .ThenByDescending(cs => cs.Semester.SemesterNo)
                .Select(cs => cs.SemesterID)
                .FirstOrDefaultAsync();

            if (currentSemesterId == 0)
            {
                return NotFound(new { message = "Hệ thống chưa thiết lập học kỳ hiện tại!" });
            }

            // Lấy profile sinh viên
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

            // 2. SỬA ĐỔI: Lọc danh sách môn kỳ này theo currentSemesterId
            var currentCourses = await _context.BangDiemChiTiet
                .Where(v => v.StudentID == studentId && v.SemesterID == currentSemesterId)
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

            // 3. SỬA ĐỔI: Sắp xếp lịch sử GPA chuẩn theo trình tự thời gian (Năm học -> Số hiệu kỳ)
            var gpaHistory = await _context.StudentSemesterGpas
                .Where(g => g.StudentID == studentId)
                .OrderBy(g => g.StartYear)
                .ThenBy(g => g.SemesterNo)
                .Select(g => new
                {
                    SemesterID = g.SemesterID,
                    SemesterNo = g.SemesterNo,
                    StartYear = g.StartYear,
                    AcademicYear = $"{g.StartYear}-{g.StartYear + 1}",
                    SemesterDisplayName = g.SemesterNo == 1 ? "HKI" :
                                          g.SemesterNo == 2 ? "HKII" : "HKIII",
                    SemesterGpa = g.SemesterGPA,
                    CumulativeGpa = g.CumulativeGPA
                })
                .ToListAsync();

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