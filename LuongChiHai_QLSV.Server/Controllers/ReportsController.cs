using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.DTOs.Reports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LuongChiHai_QLSV.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly SchoolContext _context;

        public ReportsController(SchoolContext context)
        {
            _context = context;
        }

        [HttpGet("semester-summary")]
        public async Task<IActionResult> GetSemesterSummary(
            [FromQuery] string studentId,
            [FromQuery] int semesterId,
            [FromQuery] int year)
        {
            var chiTietMonHoc = await LoadStudentGradesAsync(studentId, semesterId, year);

            if (chiTietMonHoc == null || !chiTietMonHoc.Any())
            {
                throw new KeyNotFoundException($"Không tìm thấy dữ liệu cho sinh viên {studentId} tại học kỳ có mã ID {semesterId}");
            }

            var firstItem = chiTietMonHoc.First();

            var response = new
            {
                studentID = studentId,
                semesterID = firstItem.SemesterID,
                semesterNo = firstItem.SemesterNo,
                startYear = firstItem.StartYear,
                totalEnrollment = chiTietMonHoc.Count,
                coursesDetail = chiTietMonHoc.Select(x => new
                {
                    studentID = x.StudentID,
                    enrollmentID = x.EnrollmentID,
                    sectionID = x.SectionID,
                    semesterID = x.SemesterID,
                    semesterNo = x.SemesterNo,
                    startYear = x.StartYear,
                    academicYear = x.AcademicYear,
                    courseID = x.CourseID,
                    courseName = x.CourseName,
                    credits = x.Credits,
                    totalScore = x.TotalScore ?? 0,
                    grade = x.Grade,
                    result = x.Result
                })
            };

            return Ok(response);
        }

        [HttpGet("summary")]
        public async Task<ActionResult<StudentSummaryDto>> GetMySummary([FromQuery] int semesterNo, [FromQuery] int year)
        {
            var studentId = User.FindFirst("StudentID")?.Value
                     ?? User.FindFirst(ClaimTypes.Name)?.Value
                     ?? User.FindFirst("sub")?.Value;

            if (studentId == null)
            {
                throw new UnauthorizedAccessException("Người dùng chưa đăng nhập");
            }

            var chiTietMonHoc = await LoadStudentGradesAsync(studentId, semesterNo, year);

            if (chiTietMonHoc.Count == 0)
            {
                throw new KeyNotFoundException($"Không tìm thấy dữ liệu học tập cho sinh viên {studentId} tại học kỳ này");
            }

            var firstItem = chiTietMonHoc.First();

            var response = new StudentSummaryDto
            {
                StudentID = studentId,
                // Thay đổi: Gán bộ nhận diện học kỳ mới vào DTO
                SemesterID = firstItem.SemesterID,
                SemesterNo = firstItem.SemesterNo,
                StartYear = firstItem.StartYear,

                TotalEnrollment = chiTietMonHoc.Count,
                CoursesDetail = chiTietMonHoc
            };

            return Ok(response);
        }

        private async Task<List<StudentCourseGradeDto>> LoadStudentGradesAsync(string studentId, int semesterNo, int year)
        {
            return await _context.BangDiemChiTiet
                .AsNoTracking()
                .Where(x => x.StudentID == studentId && x.SemesterNo == semesterNo && x.StartYear == year)
                .Select(x => new StudentCourseGradeDto
                {
                    StudentID = x.StudentID,
                    EnrollmentID = x.EnrollmentID,
                    SectionID = x.SectionID,
                    SemesterID = x.SemesterID,
                    SemesterNo = x.SemesterNo,
                    StartYear = x.StartYear,
                    CourseID = x.CourseID,
                    CourseName = x.CourseName,
                    Credits = x.Credits,
                    TotalScore = x.TotalScore,
                    GradeCode = x.GradeCode
                })
                .ToListAsync();
        }
    }
}