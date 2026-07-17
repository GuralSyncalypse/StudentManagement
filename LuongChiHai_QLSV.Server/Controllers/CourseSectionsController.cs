using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.DTOs;
using LuongChiHai_QLSV.Server.Entities;
using LuongChiHai_QLSV.Server.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class CourseSectionsController : ControllerBase
{
    private readonly SchoolContext _context;
    public CourseSectionsController(SchoolContext context)
    {
        _context = context;
    }

    // GET: api/CourseSection
    // GET: api/CourseSection
    [HttpGet("available")]
    [HasPermission("0702")]
    public async Task<ActionResult<IEnumerable<CourseSectionDto>>> GetAvailableCourseSection()
    {
        var today = DateTime.Today;

        // 1. Tìm học kỳ đang mở đăng ký dựa trên ngày và trạng thái IsRegistrationEnabled
        var openSemester = await _context.Semesters
            .FirstOrDefaultAsync(s => s.IsRegistrationEnabled &&
                                      today >= s.RegistrationStartDate &&
                                      today <= s.RegistrationEndDate);

        // Nếu không tìm thấy kỳ nào đang mở, trả về danh sách rỗng hoặc thông báo
        if (openSemester == null)
        {
            return Ok(new List<CourseSectionDto>());
        }

        // 2. Lọc CourseSections chỉ thuộc học kỳ đó
        var data = await _context.CourseSections
            .AsNoTracking()
            .Where(s => s.Status == "Open" && s.SemesterID == openSemester.SemesterID) // Lọc theo SemesterID
            .Select(s => new CourseSectionDto
            {
                SectionID = s.SectionID,
                CourseID = s.CourseID,
                CourseName = s.Course != null ? s.Course.CourseName : null,
                SemesterID = s.SemesterID,
                SemesterNo = s.Semester.SemesterNo,
                StartYear = s.Semester.StartYear,
                SemesterDisplayName = s.Semester.SemesterNo == 1 ? "Học kỳ I" :
                                      s.Semester.SemesterNo == 2 ? "Học kỳ II" :
                                      s.Semester.SemesterNo == 3 ? "Học kỳ hè" : "Không xác định",
                ClassSection = s.ClassSection,
                MaxCapacity = s.MaxCapacity,
                Status = s.Status,
                CurrentEnrollment = s.Enrollments.Count,
                IsEnrolled = false
            })
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet]
    [HasPermission("0702")]
    public async Task<ActionResult<IEnumerable<CourseSectionDto>>> GetCourseSection()
    {
        // 2. Lọc CourseSections chỉ thuộc học kỳ đó
        var data = await _context.CourseSections
            .AsNoTracking()
            .Where(s => s.Status == "Open")
            .Select(s => new CourseSectionDto
            {
                SectionID = s.SectionID,
                CourseID = s.CourseID,
                CourseName = s.Course != null ? s.Course.CourseName : null,
                SemesterID = s.SemesterID,
                SemesterNo = s.Semester.SemesterNo,
                StartYear = s.Semester.StartYear,
                SemesterDisplayName = s.Semester.SemesterNo == 1 ? "Học kỳ I" :
                                      s.Semester.SemesterNo == 2 ? "Học kỳ II" :
                                      s.Semester.SemesterNo == 3 ? "Học kỳ hè" : "Không xác định",
                ClassSection = s.ClassSection,
                MaxCapacity = s.MaxCapacity,
                Status = s.Status,
                CurrentEnrollment = s.Enrollments.Count,
                IsEnrolled = false
            })
            .ToListAsync();

        return Ok(data);
    }

    // GET: api/CourseSection/5
    [HttpGet("{sectionid}")]
    [HasPermission("0702")]
    public async Task<ActionResult<CourseSectionDto>> GetCourseSection(int sectionid)
    {
        var coursesection = await _context.CourseSections
            .AsNoTracking()
            .Where(s => s.SectionID == sectionid)
            .Select(s => new CourseSectionDto
            {
                SectionID = s.SectionID,
                CourseID = s.CourseID,
                CourseName = s.Course != null ? s.Course.CourseName : null,
                SemesterID = s.SemesterID,
                SemesterNo = s.Semester.SemesterNo,
                StartYear = s.Semester.StartYear,
                SemesterDisplayName = s.Semester.SemesterNo == 1 ? "Học kỳ I" :
                                      s.Semester.SemesterNo == 2 ? "Học kỳ II" :
                                      s.Semester.SemesterNo == 3 ? "Học kỳ hè" : "Không xác định",
                ClassSection = s.ClassSection,
                MaxCapacity = s.MaxCapacity,
                Status = s.Status,
                CurrentEnrollment = s.Enrollments.Count,
                IsEnrolled = false
            })
            .FirstOrDefaultAsync();

        if (coursesection == null)
        {
            return NotFound();
        }

        return Ok(coursesection);
    }

    // PUT: api/CourseSection/5
    [HttpPut("{sectionid}")]
    [HasPermission("0703")]
    public async Task<IActionResult> PutCourseSection(int sectionid, CreateUpdateSectionDto request)
    {
        var coursesection = await _context.CourseSections.FindAsync(sectionid);
        if (coursesection == null)
        {
            return NotFound();
        }

        // Cập nhật các trường dữ liệu theo DTO mới
        coursesection.CourseID = request.CourseID;
        coursesection.SemesterID = request.SemesterID; // Thay đổi từ Semester sang SemesterID
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
    [HttpPost]
    [HasPermission("0701")]
    public async Task<ActionResult<CourseSectionDto>> PostCourseSection(CreateUpdateSectionDto request)
    {
        var coursesection = new CourseSection
        {
            CourseID = request.CourseID,
            SemesterID = request.SemesterID, // Thay đổi từ Semester sang SemesterID
            ClassSection = request.ClassSection ?? "L01",
            MaxCapacity = request.MaxCapacity,
            Status = request.Status
        };

        _context.CourseSections.Add(coursesection);
        await _context.SaveChangesAsync();

        // Lấy lại dữ liệu kèm các bảng liên kết (Course, Semester) để trả về DTO đầy đủ thông tin nhất cho Client
        var response = await _context.CourseSections
            .AsNoTracking()
            .Where(s => s.SectionID == coursesection.SectionID)
            .Select(s => new CourseSectionDto
            {
                SectionID = s.SectionID,
                CourseID = s.CourseID,
                CourseName = s.Course != null ? s.Course.CourseName : null,
                SemesterID = s.SemesterID,
                SemesterNo = s.Semester.SemesterNo,
                StartYear = s.Semester.StartYear,
                SemesterDisplayName = s.Semester.SemesterNo == 1 ? "Học kỳ I" :
                                      s.Semester.SemesterNo == 2 ? "Học kỳ II" :
                                      s.Semester.SemesterNo == 3 ? "Học kỳ hè" : "Không xác định",
                ClassSection = s.ClassSection,
                MaxCapacity = s.MaxCapacity,
                Status = s.Status,
                CurrentEnrollment = 0,
                IsEnrolled = false
            })
            .FirstOrDefaultAsync();

        return CreatedAtAction(nameof(GetCourseSection), new { sectionid = coursesection.SectionID }, response);
    }

    // DELETE: api/CourseSection/5
    [HttpDelete("{sectionid}")]
    [HasPermission("0704")]
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
