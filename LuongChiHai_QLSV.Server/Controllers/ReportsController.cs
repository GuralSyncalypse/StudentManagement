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
        public async Task<ActionResult<StudentSemesterSummaryDto>> GetSemesterSummary(
            [FromQuery] string studentId,
            [FromQuery] int semesterId) // Thay đổi: Nhận vào mã ID học kỳ
        {
            var chiTietMonHoc = await LoadStudentGradesAsync(studentId, semesterId);

            if (chiTietMonHoc == null || chiTietMonHoc.Count == 0)
            {
                return NotFound($"Không tìm thấy dữ liệu cho sinh viên {studentId} tại học kỳ có mã ID {semesterId}");
            }

            var tongSoMonDaHoc = chiTietMonHoc.Count;
            var soMonDaQua = chiTietMonHoc.Count(x => x.GradeDetails.IsPassed());
            var soMonTruot = chiTietMonHoc.Count(x => x.GradeDetails.IsFailed());

            // Tính toán GPA học kỳ cho các môn đã có điểm
            var cacMonDaCoDiem = chiTietMonHoc.Where(x => x.TotalScore.HasValue).ToList();
            var tongTinChiHocKy = cacMonDaCoDiem.Sum(x => x.Credits);
            var tongDiemNhanTinChi = cacMonDaCoDiem.Sum(x => (x.TotalScore ?? 0) * x.Credits);

            var diemTrungBinhHocKy = tongTinChiHocKy > 0
                ? Math.Round(tongDiemNhanTinChi / tongTinChiHocKy, 2)
                : 0;

            // Lấy thông tin học kỳ từ phần tử đầu tiên trong danh sách kết quả
            var firstItem = chiTietMonHoc.First();

            var response = new StudentSemesterSummaryDto
            {
                StudentID = studentId,
                // Thay đổi: Gán bộ nhận diện học kỳ mới vào DTO
                SemesterID = firstItem.SemesterID,
                SemesterNo = firstItem.SemesterNo,
                StartYear = firstItem.StartYear,

                TongSoMonDaHoc = tongSoMonDaHoc,
                SoMonDaQua = soMonDaQua,
                SoMonTruot = soMonTruot,
                DiemTrungBinhHocKy = (decimal)diemTrungBinhHocKy,
                ChiTietMonHoc = chiTietMonHoc
            };

            return Ok(response);
        }

        [HttpGet("summary")]
        public async Task<ActionResult<StudentSummaryDto>> GetMySummary([FromQuery] int semesterId) // Thay đổi: Nhận vào mã ID học kỳ
        {
            var studentId = User.FindFirst("StudentID")?.Value
                     ?? User.FindFirst(ClaimTypes.Name)?.Value
                     ?? User.FindFirst("sub")?.Value;

            if (studentId == null)
            {
                return Unauthorized();
            }

            var chiTietMonHoc = await LoadStudentGradesAsync(studentId, semesterId);

            if (chiTietMonHoc.Count == 0)
            {
                return NotFound($"Không tìm thấy dữ liệu học tập cho sinh viên {studentId} tại học kỳ này");
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

        private async Task<List<StudentCourseGradeDto>> LoadStudentGradesAsync(string studentId, int semesterId)
        {
            // Thay đổi: Điều kiện lọc theo SemesterID và Explicit Select để ánh xạ chuẩn xác dữ liệu từ View
            return await _context.BangDiemChiTiet
                .AsNoTracking()
                .Where(x => x.StudentID == studentId && x.SemesterID == semesterId)
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