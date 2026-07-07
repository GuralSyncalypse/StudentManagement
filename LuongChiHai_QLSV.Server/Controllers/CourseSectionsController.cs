using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.DTOs;
using LuongChiHai_QLSV.Server.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static System.Collections.Specialized.BitVector32;

[Route("api/[controller]")]
[ApiController]
public class CourseSectionsController : ControllerBase
{
    private readonly SchoolContext _context;
    public CourseSectionsController(SchoolContext context)
    {
        _context = context;
    }

    // GET: api/CourseSection
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CourseSection>>> GetCourseSection()
    {
        var data = await _context.CourseSections
        .Where(s => s.Status == "Open")
        .Select(s => new
        {
            SectionID = s.SectionID,
            CourseID = s.CourseID,
            Semester = s.Semester,
            ClassSection = s.ClassSection,
            MaxCapacity = s.MaxCapacity,
            Status = s.Status,
            Course = s.Course != null ? new
            {
                CourseID = s.Course.CourseID,
                CourseName = s.Course.CourseName
            } : null,
            CurrentEnrollment = s.Enrollments.Count
        })
        .ToListAsync();

        return Ok(data);
    }

    // GET: api/CourseSection/5W
    [HttpGet("{sectionid}")]
    public async Task<ActionResult<CourseSection>> GetCourseSection(int sectionid)
    {
        var coursesection = await _context.CourseSections.FindAsync(sectionid);

        if (coursesection == null)
        {
            return NotFound();
        }

        return coursesection;
    }

    // PUT: api/CourseSection/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{sectionid}")]
    public async Task<IActionResult> PutCourseSection(int? sectionid, CourseSection coursesection)
    {
        if (sectionid != coursesection.SectionID)
        {
            return BadRequest();
        }

        _context.Entry(coursesection).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CourseSectionExists(sectionid))
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

    // POST: api/CourseSection
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<CourseSection>> PostCourseSection(CourseSection coursesection)
    {
        _context.CourseSections.Add(coursesection);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetCourseSection", new { sectionid = coursesection.SectionID }, coursesection);
    }

    // DELETE: api/CourseSection/5
    [HttpDelete("{sectionid}")]
    public async Task<IActionResult> DeleteCourseSection(int? sectionid)
    {
        var coursesection = await _context.CourseSections.FindAsync(sectionid);
        if (coursesection == null)
        {
            return NotFound();
        }

        _context.CourseSections.Remove(coursesection);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool CourseSectionExists(int? sectionid)
    {
        return _context.CourseSections.Any(e => e.SectionID == sectionid);
    }



    
}
