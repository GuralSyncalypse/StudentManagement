using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.DTOs.Reports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

            // 1. Lấy toàn bộ danh sách môn học của SV đó trong học kỳ được chọn từ View
            var chiTietMonHoc = await _context.BangDiemChiTiet
                .Where(x => x.StudentID == studentId && x.Semester == semester)
                .ToListAsync();         

            if (!chiTietMonHoc.Any())
            {
                return NotFound($"Không tìm thấy dữ liệu cho sinh viên {studentId} tại học kỳ {semester}");
            }

            // 2. Thực hiện tính toán thống kê (Logic SQL được xử lý trên Memory cực nhanh sau khi filter)
            int tongSoMonDaHoc = chiTietMonHoc.Count;
            int soMonDaQua = chiTietMonHoc.Count(x => x.Result == "Đỗ");
            int soMonTruot = chiTietMonHoc.Count(x => x.Result == "Trượt");

            // Tính GPA theo trọng số tín chỉ: SUM(TotalScore * Credits) / SUM(Credits)
            decimal tongTinChi = chiTietMonHoc.Sum(x => x.Credits);
            decimal tongDiemNhanTinChi = chiTietMonHoc.Sum(x => (x.TotalScore ?? 0) * x.Credits);

            decimal diemTrungBinhHocKy = tongTinChi > 0
                ? Math.Round(tongDiemNhanTinChi / tongTinChi, 2)
                : 0;

            // 3. Gom tất cả vào DTO tổng hợp để trả về cho Frontend Angular
            var response = new StudentSemesterSummaryDto
            {
                StudentID = studentId,
                Semester = semester,
                TongSoMonDaHoc = tongSoMonDaHoc,
                SoMonDaQua = soMonDaQua,
                SoMonTruot = soMonTruot,
                DiemTrungBinhHocKy = diemTrungBinhHocKy,
                ChiTietMonHoc = chiTietMonHoc
            };

            return Ok(response);
        }
    }
}
