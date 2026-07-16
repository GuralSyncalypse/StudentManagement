using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.DTOs.Reports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
            [FromQuery] int semester)
        {
            var chiTietMonHoc = await LoadStudentGradesAsync(studentId, semester);

            if (chiTietMonHoc == null || chiTietMonHoc.Count == 0)
            {
                return NotFound($"Không tìm thấy dữ liệu cho sinh viên {studentId} tại học kỳ {semester}");
            }

            // 1. Loại bỏ hoàn toàn chuỗi cứng "Đỗ"/"Trượt", dùng trực tiếp logic từ Smart Enum
            var tongSoMonDaHoc = chiTietMonHoc.Count;
            var soMonDaQua = chiTietMonHoc.Count(x => x.GradeDetails.IsPassed());
            var soMonTruot = chiTietMonHoc.Count(x => x.GradeDetails.IsFailed());

            // 2. Sửa lỗi Logic tính GPA: Chỉ tính các môn ĐÃ CÓ ĐIỂM
            // Môn nào "Chưa có điểm" (Pending) thì không được lôi vào để tính Điểm trung bình học kỳ
            var cacMonDaCoDiem = chiTietMonHoc.Where(x => x.TotalScore.HasValue).ToList();

            var tongTinChiHocKy = cacMonDaCoDiem.Sum(x => x.Credits);
            var tongDiemNhanTinChi = cacMonDaCoDiem.Sum(x => (x.TotalScore ?? 0) * x.Credits);

            var diemTrungBinhHocKy = tongTinChiHocKy > 0
                ? Math.Round(tongDiemNhanTinChi / tongTinChiHocKy, 2)
                : 0;

            var response = new StudentSemesterSummaryDto
            {
                StudentID = studentId,
                Semester = semester,
                TongSoMonDaHoc = tongSoMonDaHoc,
                SoMonDaQua = soMonDaQua,
                SoMonTruot = soMonTruot,
                DiemTrungBinhHocKy = diemTrungBinhHocKy,
                ChiTietMonHoc = chiTietMonHoc // Danh sách trả về vẫn đầy đủ các môn (kể cả môn pending)
            };

            return Ok(response);
        }

        [HttpGet("summary")]
        public async Task<ActionResult<StudentSummaryDto>> GetMySummary([FromQuery] int semester)
        {
            var studentId = User.FindFirst("StudentID")?.Value
                     ?? User.FindFirst(ClaimTypes.Name)?.Value
                     ?? User.FindFirst("sub")?.Value;

            if (studentId == null)
            {
                return Unauthorized();
            }

            var chiTietMonHoc = await LoadStudentGradesAsync(studentId, semester);

            if (chiTietMonHoc.Count == 0)
            {
                return NotFound($"Không tìm thấy dữ liệu cho sinh viên {studentId}");
            }

            var response = new StudentSummaryDto
            {
                StudentID = studentId,
                Semester = semester,
                TotalEnrollment = chiTietMonHoc.Count,
                CoursesDetail = chiTietMonHoc
            };

            return Ok(response);
        }

        private Task<List<StudentCourseGradeDto>> LoadStudentGradesAsync(string studentId, int semester)
        {
            return _context.BangDiemChiTiet
                .AsNoTracking()
                .Where(x => x.StudentID == studentId && x.Semester == semester)
                .ToListAsync();
        }
    }
}
