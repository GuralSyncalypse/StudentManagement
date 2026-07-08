using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.DTOs;
using LuongChiHai_QLSV.Server.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public async Task<ActionResult<IEnumerable<CourseSectionDto>>> GetCourseSection()
    {
        var data = await _context.CourseSections
        .AsNoTracking()
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
    public async Task<ActionResult<CourseSectionDto>> GetCourseSection(int sectionid)
    {
        var coursesection = await _context.CourseSections
            .AsNoTracking()
            .Where(s => s.SectionID == sectionid)
            .Select(s => new CourseSectionDto
            {
                SectionID = s.SectionID,
                CourseID = s.CourseID,
                Semester = s.Semester,
                ClassSection = s.ClassSection,
                MaxCapacity = s.MaxCapacity,
                Status = s.Status,
                CourseName = s.Course.CourseName,
                CurrentEnrollment = s.Enrollments.Count,
                IsEnrolled = false
            })
            .FirstOrDefaultAsync();

        if (coursesection == null)
        {
            return NotFound();
        }

        return coursesection;
    }

    // PUT: api/CourseSection/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{sectionid}")]
    public async Task<IActionResult> PutCourseSection(int? sectionid, CreateUpdateSectionDto request)
    {
        if (sectionid == null)
        {
            return BadRequest();
        }

        var coursesection = await _context.CourseSections.FindAsync(sectionid);
        if (coursesection == null)
        {
            return NotFound();
        }

        coursesection.CourseID = request.CourseID;
        coursesection.Semester = request.Semester;
        coursesection.ClassSection = request.ClassSection ?? coursesection.ClassSection;
        coursesection.MaxCapacity = request.MaxCapacity;
        coursesection.Status = request.Status;

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
    public async Task<ActionResult<CourseSectionDto>> PostCourseSection(CreateUpdateSectionDto request)
    {
        var coursesection = new CourseSection
        {
            CourseID = request.CourseID,
            Semester = request.Semester,
            ClassSection = request.ClassSection ?? "L01",
            MaxCapacity = request.MaxCapacity,
            Status = request.Status
        };

        _context.CourseSections.Add(coursesection);
        await _context.SaveChangesAsync();

        var response = new CourseSectionDto
        {
            SectionID = coursesection.SectionID,
            CourseID = coursesection.CourseID,
            Semester = coursesection.Semester,
            ClassSection = coursesection.ClassSection,
            MaxCapacity = coursesection.MaxCapacity,
            Status = coursesection.Status,
            CourseName = null,
            CurrentEnrollment = 0,
            IsEnrolled = false
        };

        return CreatedAtAction(nameof(GetCourseSection), new { sectionid = coursesection.SectionID }, response);
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
