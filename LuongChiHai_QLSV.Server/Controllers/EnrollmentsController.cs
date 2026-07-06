using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LuongChiHai_QLSV.Server.Entities;
using LuongChiHai_QLSV.Server.Data;

[Route("api/[controller]")]
[ApiController]
public class EnrollmentsController : ControllerBase
{
    private readonly SchoolContext _context;
    public EnrollmentsController(SchoolContext context)
    {
        _context = context;
    }

    // GET: api/Enrollment
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetEnrollments()
    {
        var enrollments = await _context.Enrollments
            .Select(e => new EnrollmentDto
            {
                EnrollmentID = e.EnrollmentID,
                StudentID = e.StudentID,
                SectionID = e.SectionID,
                EnrollDate = e.EnrollDate,
                // Mapping danh sách Scores của từng Enrollment sang ScoreDto
                Scores = e.Scores.Select(s => new ScoreDto
                {
                    ScoreID = s.ScoreID,
                    ScoreType = s.ScoreType, // Thay bằng tên trường thực tế trong DB của bạn
                    ScoreValue = s.ScoreValue          // Thay bằng tên trường thực tế trong DB của bạn
                }).ToList()
            })
            .ToListAsync();

        return enrollments;
    }

    // DTO cho bảng Score
    public class ScoreDto
    {
        public int ScoreID { get; set; }
        public string? ScoreType { get; set; } // Ví dụ: Điểm giữa kỳ, điểm cuối kỳ...
        public decimal? ScoreValue { get; set; }     // Điểm số
    }

    // DTO cho bảng Enrollment
    public class EnrollmentDto
    {
        public int EnrollmentID { get; set; }
        public string StudentID { get; set; } = null!;
        public int SectionID { get; set; }
        public DateTime? EnrollDate { get; set; }

        // Thay vì chứa Entity Score, ta chứa List ScoreDto
        public List<ScoreDto> Scores { get; set; } = new List<ScoreDto>();
    }

    // GET: api/Enrollment/5
    [HttpGet("{enrollmentid}")]
    public async Task<ActionResult<EnrollmentDto>> GetEnrollment(int enrollmentid)
    {
        var enrollmentDto = await _context.Enrollments
            .Where(e => e.EnrollmentID == enrollmentid)
            .Select(e => new EnrollmentDto
            {
                EnrollmentID = e.EnrollmentID,
                StudentID = e.StudentID,
                SectionID = e.SectionID,
                EnrollDate = e.EnrollDate,
                // Mapping danh sách Scores sang ScoreDto
                Scores = e.Scores.Select(s => new ScoreDto
                {
                    ScoreID = s.ScoreID,
                    ScoreType = s.ScoreType, // Thay bằng tên trường thực tế trong bảng Score của bạn
                    ScoreValue = s.ScoreValue          // Thay bằng tên trường thực tế trong bảng Score của bạn
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (enrollmentDto == null)
        {
            return NotFound();
        }

        return enrollmentDto; // Trả về DTO thay vì Entity gốc
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

    // POST: api/Enrollment
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Enrollment>> PostEnrollment(Enrollment enrollment)
    {
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetEnrollment", new { enrollmentid = enrollment.EnrollmentID }, enrollment);
    }

    // DELETE: api/Enrollment/5
    [HttpDelete("{enrollmentid}")]
    public async Task<IActionResult> DeleteEnrollment(int? enrollmentid)
    {
        var enrollment = await _context.Enrollments.FindAsync(enrollmentid);
        if (enrollment == null)
        {
            return NotFound();
        }

        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool EnrollmentExists(int? enrollmentid)
    {
        return _context.Enrollments.Any(e => e.EnrollmentID == enrollmentid);
    }
}
