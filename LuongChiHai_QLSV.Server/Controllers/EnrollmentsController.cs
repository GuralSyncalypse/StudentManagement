using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.DTOs;
using LuongChiHai_QLSV.Server.Entities;
using LuongChiHai_QLSV.Server.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly SchoolContext _context;

    public EnrollmentsController(SchoolContext context)
    {
        _context = context;
    }

    [HttpGet("my-enrollments")]
    [HasPermission("student:view_own_grades")]
    public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetMyEnrollments()
    {
        var studentIdClaim = User.FindFirst("StudentID")?.Value
            ?? User.FindFirst(ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(studentIdClaim)) return Unauthorized("Không tìm thấy thông tin sinh viên.");

        var myEnrollments = await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.StudentID == studentIdClaim)
            .Select(e => new EnrollmentDto
            {
                EnrollmentID = e.EnrollmentID,
                StudentID = e.StudentID,
                SectionID = e.SectionID,
                EnrollDate = e.EnrollDate,
                Scores = e.Scores.Select(s => new ScoreDto
                {
                    ScoreID = s.ScoreID,
                    ScoreType = s.ScoreType,
                    Weight = s.Weight,
                    ScoreValue = s.ScoreValue
                }).ToList()
            })
            .ToListAsync();

        return Ok(myEnrollments);
    }

    // GET: api/admin/enrollments
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetEnrollments()
    {
        var enrollments = await _context.Enrollments
            .AsNoTracking()
            .Select(e => new EnrollmentDto
            {
                EnrollmentID = e.EnrollmentID,
                StudentID = e.StudentID,
                SectionID = e.SectionID,
                EnrollDate = e.EnrollDate,
                Scores = e.Scores.Select(s => new ScoreDto
                {
                    ScoreID = s.ScoreID,
                    ScoreType = s.ScoreType,
                    Weight = s.Weight,
                    ScoreValue = s.ScoreValue
                }).ToList()
            })
            .ToListAsync();

        return Ok(enrollments);
    }

    // GET: api/admin/Enrollment/5
    [HttpGet("{id}")]
    public async Task<ActionResult<EnrollmentDto>> GetEnrollment(int id)
    {
        var enrollmentDto = await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.EnrollmentID == id)
            .Select(e => new EnrollmentDto
            {
                EnrollmentID = e.EnrollmentID,
                StudentID = e.StudentID,
                SectionID = e.SectionID,
                EnrollDate = e.EnrollDate,
                Scores = e.Scores.Select(s => new ScoreDto
                {
                    ScoreID = s.ScoreID,
                    ScoreType = s.ScoreType,
                    Weight = s.Weight,
                    ScoreValue = s.ScoreValue
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (enrollmentDto == null) return NotFound();

        return enrollmentDto;
    }

    // PUT: api/Enrollment/5
    [HttpPut("{enrollmentid}")]
    public async Task<IActionResult> PutEnrollment(int? enrollmentid, Enrollment enrollment)
    {
        if (enrollmentid != enrollment.EnrollmentID)
        {
            return BadRequest();
        }

        _context.Entry(enrollment).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!EnrollmentExists(enrollmentid))
            {
                return NotFound();
            }

            throw;
        }

        return NoContent();
    }

    // POST: api/admin/Enrollment
    [HttpPost]
    public async Task<IActionResult> PostEnrollment([FromBody] AdminRegistrationDto dto)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var section = await _context.CourseSections
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SectionID == dto.SectionID);

            if (section == null)
                return NotFound(new { message = "Lớp học phần không tồn tại." });

            if (!string.Equals(section.Status, "Open", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Lớp học phần này đã đóng." });

            var currentEnrollment = await _context.Enrollments
                .CountAsync(e => e.SectionID == dto.SectionID);

            if (currentEnrollment >= section.MaxCapacity)
                return BadRequest(new { message = "Lớp học phần đã đủ sĩ số tối đa." });

            var studentExists = await _context.Students
                .AsNoTracking()
                .AnyAsync(s => s.StudentID == dto.StudentID);

            if (!studentExists)
                return BadRequest(new { message = "Mã số sinh viên không tồn tại trên hệ thống." });

            var isAlreadyRegistered = await _context.Enrollments
                .AnyAsync(e => e.SectionID == dto.SectionID && e.StudentID == dto.StudentID);

            if (isAlreadyRegistered)
                return BadRequest(new { message = "Sinh viên này đã được xếp vào lớp này rồi." });

            var newEnrollment = new Enrollment
            {
                SectionID = dto.SectionID,
                StudentID = dto.StudentID,
                EnrollDate = DateTime.UtcNow
            };

            _context.Enrollments.Add(newEnrollment);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Có lỗi xảy ra trong quá trình đăng ký.", error = ex.Message });
        }
    }

    // DELETE: api/Enrollment/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEnrollment(int id)
    {
        var enrollment = await _context.Enrollments.FindAsync(id);
        if (enrollment == null) return NotFound();

        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("admin-cancel")]
    public async Task<IActionResult> AdminCancelEnrollment([FromQuery] int sectionID, [FromQuery] string studentID)
    {
        // 1. Kiểm tra dữ liệu đầu vào cơ bản
        if (sectionID <= 0 || string.IsNullOrWhiteSpace(studentID))
        {
            return BadRequest(new { message = "Dữ liệu đầu vào không hợp lệ." });
        }

        try
        {
            // 2. Tìm bản ghi Đăng ký (Enrollment) khớp cả mã lớp và mã sinh viên
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.SectionID == sectionID && e.StudentID == studentID);

            // 3. Nếu không tìm thấy, trả về lỗi 404 NotFound
            if (enrollment == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin xếp lớp của sinh viên này trong học phần đã chọn." });
            }

            // 4. Tiến hành xóa bản ghi khỏi Database
            _context.Enrollments.Remove(enrollment);

            // Nếu bạn có bảng Score (Điểm) liên kết và muốn xóa luôn điểm khi hủy lớp (nếu có), 
            // EF Core sẽ tự động xóa nếu bạn cấu hình Cascade Delete.

            await _context.SaveChangesAsync();

            // 5. Trả về thông báo thành công (HTTP 200 OK)
            return Ok(new { message = "Hủy xếp lớp cho sinh viên thành công!" });
        }
        catch (Exception ex)
        {
            // 6. Ghi log lỗi nếu cần và trả về lỗi hệ thống 500
            // _logger.LogError(ex, "Lỗi khi hủy xếp lớp");
            return StatusCode(500, new { message = "Đã xảy ra lỗi hệ thống khi hủy xếp lớp.", error = ex.Message });
        }
    }   

    private bool EnrollmentExists(int? enrollmentid)
    {
        return _context.Enrollments.Any(e => e.EnrollmentID == enrollmentid);
    }
}
