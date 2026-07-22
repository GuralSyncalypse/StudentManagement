using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.DTOs;
using LuongChiHai_QLSV.Server.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class SemestersController : ControllerBase
{
    private readonly SchoolContext _context;
    public SemestersController(SchoolContext context)
    {
        _context = context;
    }

    // GET: api/Semester
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SemesterDto>>> GetSemester()
    {
        var semesters = await _context.Semesters
            .Include(s => s.CourseSections)
            .Select(s => new SemesterDto
            {
                SemesterID = s.SemesterID,
                SemesterNo = s.SemesterNo,
                StartYear = s.StartYear,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                RegistrationStartDate = s.RegistrationStartDate,
                RegistrationEndDate = s.RegistrationEndDate,
                IsRegistrationEnabled = s.IsRegistrationEnabled,
                CourseSections = s.CourseSections.Select(cs => new CourseSectionBaseDto
                {
                    // Map các thuộc tính của CourseSection ở đây
                    SectionID = cs.SectionID,
                    CourseID = cs.CourseID
                }).ToList()     
            })
            .ToListAsync();

        return Ok(semesters);
    }

    /// <summary>
    /// GET: api/semesters/{semesterId}/courses/{courseId}/sections
    /// Lấy danh sách lớp học phần của một môn học trong học kỳ
    /// </summary>
    [HttpGet("{semesterId}/courses/{courseId}/sections")]
    public async Task<IActionResult> GetSectionsByCourse(int semesterId, string courseId)
    {
        // Truy vấn danh sách lớp học phần theo SemesterId và CourseId
        var sections = await _context.CourseSections
            .Where(s => s.SemesterID == semesterId && s.CourseID == courseId)
            .Select(s => new
            {
                sectionID = s.SectionID,
                courseID = s.CourseID,
                courseName = s.Course != null ? s.Course.CourseName : "Tên môn học",
                sectionName = s.ClassSection, // Ví dụ: "Nhóm 1", "Nhóm 2"
                credits = s.Course != null ? s.Course.Credits : 3,
                currentEnrollment = s.Enrollments.Count(),
                maxCapacity = s.MaxCapacity,
                status = s.Status
            })
            .ToListAsync();

        // Lưu ý: Nên trả về 200 OK kèm mảng rỗng [] nếu không tìm thấy dữ liệu,
        // tránh trả về 404 làm văng lỗi Console ở Frontend Angular.
        return Ok(sections);
    }

    [HttpPatch("{id}/toggle-registration")]
    public async Task<IActionResult> ToggleRegistration(int id, [FromBody] ToggleRegistrationDto request)
    {
        var semester = await _context.Semesters.FindAsync(id);

        if (semester == null)
        {
            return NotFound(new { message = $"Không tìm thấy học kỳ với ID: {id}" });
        }

        // Cập nhật trạng thái
        semester.IsRegistrationEnabled = request.IsRegistrationEnabled;

        await _context.SaveChangesAsync();

        return Ok(semester);
    }

    // GET: api/Semester/5
    [HttpGet("{semesterid}")]
    public async Task<ActionResult<Semester>> GetSemester(int semesterid)
    {
        var semester = await _context.Semesters.FindAsync(semesterid);

        if (semester == null)
        {
            return NotFound();
        }

        return semester;
    }

    // PUT: api/Semester/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{semesterid}")]
    public async Task<IActionResult> PutSemester(int? semesterid, Semester semester)
    {
        if (semesterid != semester.SemesterID)
        {
            return BadRequest();
        }

        _context.Entry(semester).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!SemesterExists(semesterid))
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

    // POST: api/Semester
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    // POST: api/Semesters
    [HttpPost]
    public async Task<ActionResult<SemesterDto>> PostSemester([FromBody] CreateSemesterDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var semester = new Semester
        {
            SemesterNo = dto.SemesterNo,
            StartYear = dto.StartYear,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            RegistrationStartDate = dto.RegistrationStartDate,
            RegistrationEndDate = dto.RegistrationEndDate,
            IsRegistrationEnabled = dto.IsRegistrationEnabled
        };

        _context.Semesters.Add(semester);
        await _context.SaveChangesAsync();

        var resultDto = new SemesterDto
        {
            SemesterID = semester.SemesterID,
            SemesterNo = semester.SemesterNo,
            StartYear = semester.StartYear,
            StartDate = semester.StartDate,
            EndDate = semester.EndDate,
            RegistrationStartDate = semester.RegistrationStartDate,
            RegistrationEndDate = semester.RegistrationEndDate,
            IsRegistrationEnabled = semester.IsRegistrationEnabled,
            CourseSections = new List<CourseSectionBaseDto>()
        };

        return CreatedAtAction(nameof(GetSemester), new { semesterid = semester.SemesterID }, resultDto);
    }

    // DELETE: api/Semester/5
    [HttpDelete("{semesterid}")]
    public async Task<IActionResult> DeleteSemester(int? semesterid)
    {
        var semester = await _context.Semesters.FindAsync(semesterid);
        if (semester == null)
        {
            return NotFound();
        }

        _context.Semesters.Remove(semester);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool SemesterExists(int? semesterid)
    {
        return _context.Semesters.Any(e => e.SemesterID == semesterid);
    }
}
