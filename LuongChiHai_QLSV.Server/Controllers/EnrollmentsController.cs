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
    public async Task<ActionResult<IEnumerable<Enrollment>>> GetEnrollment()
    {
        return await _context.Enrollments.ToListAsync();
    }

    // GET: api/Enrollment/5
    [HttpGet("{enrollmentid}")]
    public async Task<ActionResult<Enrollment>> GetEnrollment(int enrollmentid)
    {
        var enrollment = await _context.Enrollments.FindAsync(enrollmentid);

        if (enrollment == null)
        {
            return NotFound();
        }

        return enrollment;
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
