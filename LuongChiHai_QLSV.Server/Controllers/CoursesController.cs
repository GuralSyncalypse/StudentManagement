using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly SchoolContext _context;
    public CoursesController(SchoolContext context)
    {
        _context = context;
    }

    // POST: api/Course
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    [Authorize(Policy = "0601")]
    public async Task<ActionResult<Course>> PostCourse(Course course)
    {
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetCourse", new { courseid = course.CourseID }, course);
    }

    // GET: api/Course
    [HttpGet]
    [Authorize(Policy = "0602")]
    public async Task<ActionResult<IEnumerable<Course>>> GetCourse()
    {
        return await _context.Courses.AsNoTracking().ToListAsync();
    }

    // GET: api/Course/5
    [HttpGet("{courseid}")]
    [Authorize(Policy = "0602")]
    public async Task<ActionResult<Course>> GetCourse(string courseid)
    {
        var course = await _context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CourseID == courseid);

        if (course == null)
        {
            return NotFound();
        }

        return course;
    }

    // PUT: api/Course/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{courseid}")]
    [Authorize(Policy = "0603")]
    public async Task<IActionResult> PutCourse(string courseid, Course course)
    {
        if (courseid != course.CourseID)
        {
            return BadRequest(course.CourseID);
        }

        _context.Entry(course).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CourseExists(courseid))
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

    // DELETE: api/Course/5
    [HttpDelete("{courseid}")]
    [Authorize(Policy = "0604")]
    public async Task<IActionResult> DeleteCourse(string courseid)
    {
        var course = await _context.Courses.FindAsync(courseid);
        if (course == null)
        {
            return NotFound();
        }

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool CourseExists(string courseid)
    {
        return _context.Courses.Any(e => e.CourseID == courseid);
    }
}
