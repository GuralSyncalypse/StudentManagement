using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class StudentController : ControllerBase
{
    private readonly SchoolContext _context;
    public StudentController(SchoolContext context) => _context = context;

    // GET api/Student/me
    [HttpGet("me")]
    public async Task<ActionResult<Student>> GetCurrentStudent()
    {
        var studentId = User.FindFirst(ClaimTypes.Name)?.Value
                     ?? User.FindFirst("sub")?.Value; // fallback for JWT "sub"

        if (string.IsNullOrEmpty(studentId))
            return Unauthorized();

        var student = await _context.Students.FindAsync(studentId);
        if (student == null)
            return Ok($"Not found {studentId}");

        return Ok(student);
    }

    // GET api/Student/{studentid}
    [HttpGet("{studentid}")]
    public async Task<ActionResult<Student>> GetStudent(string studentid)
    {
        var student = await _context.Students.FindAsync(studentid);
        if (student == null) return NotFound();
        return student;
    }
}