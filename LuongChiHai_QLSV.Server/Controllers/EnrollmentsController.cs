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
        // Lấy StudentID từ JWT Token đã đăng nhập
        var studentIdClaim = User.FindFirst(ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(studentIdClaim)) return Unauthorized("Không tìm thấy thông tin sinh viên.");

        string studentId = studentIdClaim;

        var myEnrollments = await _context.Enrollments
            .Where(e => e.StudentID == studentId)
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
                    ScoreValue = s.ScoreValue
                }).ToList()
            }).ToListAsync();

        return Ok(myEnrollments);
    }

    // GET: api/admin/enrollments
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetEnrollments()
    {
        return await _context.Enrollments
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
                    ScoreValue = s.ScoreValue
                }).ToList()
            })
            .ToListAsync();
    }

    // GET: api/admin/Enrollment/5
    [HttpGet("{id}")]
    public async Task<ActionResult<EnrollmentDto>> GetEnrollment(int id)
    {
        var enrollmentDto = await _context.Enrollments
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
                    ScoreValue = s.ScoreValue
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (enrollmentDto == null) return NotFound();

        return enrollmentDto;
    }

    // PUT: api/Enrollment/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/admin/Enrollment
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<IActionResult> PostEnrollment([FromBody] AdminRegistrationDto dto)
    {
        // Bước 1: Kiểm tra xem lớp học phần (SectionID) có tồn tại và đang mở không
        var section = await _context.CourseSections.FindAsync(dto.SectionID);
        if (section == null) return NotFound(new { message = "Lớp học phần không tồn tại." });
        if (section.Status != "Open") return BadRequest(new { message = "Lớp học phần này đã đóng." });

        // Bước 2: Kiểm tra sĩ số xem đã đầy chưa
        if (section.Enrollments.Count >= section.MaxCapacity)
        {
            return BadRequest(new { message = "Lớp học phần đã đủ sĩ số tối đa." });
        }

        // Bước 3: Kiểm tra Mã sinh viên (StudentID) xem có hợp lệ trong hệ thống không
        var studentExists = await _context.Students.AnyAsync(s => s.StudentID == dto.StudentID);
        if (!studentExists) return BadRequest(new { message = "Mã số sinh viên không tồn tại trên hệ thống." });

        // Bước 4: Kiểm tra xem sinh viên này đã đăng ký lớp này chưa (tránh trùng lặp)
        var isAlreadyRegistered = await _context.Enrollments
            .AnyAsync(e => e.SectionID == dto.SectionID && e.StudentID == dto.StudentID);
        if (isAlreadyRegistered) return BadRequest(new { message = "Sinh viên này đã được xếp vào lớp này rồi." });

        // Bước 5: Thêm bản ghi đăng ký mới & tăng sĩ số lớp lên 1
        var newEnrollment = new Enrollment
        {
            SectionID = dto.SectionID,
            StudentID = dto.StudentID,
            EnrollDate = DateTime.Now
        };
        _context.Enrollments.Add(newEnrollment);

        await _context.SaveChangesAsync();
        return Ok();
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

    private bool EnrollmentExists(int? enrollmentid)
    {
        return _context.Enrollments.Any(e => e.EnrollmentID == enrollmentid);
    }
}
