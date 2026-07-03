using Azure.Core;
using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.DTOs.Auths;
using LuongChiHai_QLSV.Server.DTOs.Students;
using LuongChiHai_QLSV.Server.Entities;
using LuongChiHai_QLSV.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminStudentsController : ControllerBase
{
    private readonly SchoolContext _context;
    private readonly IStudentService _studentService;
    public AdminStudentsController(SchoolContext context, IStudentService studentService)
    {
        _context = context;
        _studentService = studentService;
    }

    // GET: api/Student
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StudentResponseDto>>> GetStudent()
    {
        var response = await _studentService.GetAllAsync();

        if (response == null)
        {
            return NotFound(new { message = "Không có sinh viên nào trong hệ thống!" });
        }

        // Map Entity sang DTO
        return Ok(response);
    }

    // GET: api/Student/5
    [HttpGet("{id}")]
    public async Task<ActionResult<StudentResponseDto>> GetStudent(string id)
    {
        var response = await _studentService.GetByIdAsync(id);

        if (response == null)
            return NotFound($"Không tìm thấy sinh viên có mã số {id}");

        return Ok(response);
    }

    // PUT: api/Student/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutStudent(string? id, StudentRequestDto request)
    {
        // Lớp Validation của ASP.NET Core sẽ tự chạy nhờ các Attribute bạn đã đặt ở DTO
        if (id != request.StudentID)
        {
            return BadRequest("Mã sinh viên trong URL và Body không trùng khớp.");
        }

        // 1. Tìm bản ghi cũ trong Database
        var student = await _context.Students.FindAsync(id);
        if (student == null)
        {
            return NotFound($"Không tìm thấy sinh viên có mã {id}.");
        }

        // 2. Thực hiện mapping (Thay thế toàn bộ dữ liệu cũ bằng dữ liệu từ Request)
        student.StudentName = request.StudentName;

        // Các trường nullable dưới đây sẽ bị ghi đè thành null nếu request không truyền lên
        student.Gender = request.Gender;
        student.BirthDate = request.BirthDate;
        student.Ethnicity = request.Ethnicity;
        student.Religion = request.Religion;
        student.Nationality = request.Nationality;
        student.BirthPlace = request.BirthPlace;
        student.CitizenID = request.CitizenID;
        student.CitizenIDIssueDate = request.CitizenIDIssueDate;
        student.CitizenIDIssuePlace = request.CitizenIDIssuePlace;
        student.PermanentAddress = request.PermanentAddress;
        student.TemporaryAddress = request.TemporaryAddress;

        try
        {
            // EF Core tự nhận biết các trường thay đổi để cập nhật
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!StudentExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent(); // Cập nhật thành công thường trả về 204
    }

    // POST: api/Student
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<StudentResponseDto>> PostStudent(StudentRequestDto request)
    {
        try
        {
            await _studentService.CreateStudentAccountAsync(request);

            var response = new StudentResponseDto
            {
                StudentID = request.StudentID,
                StudentName = request.StudentName,
                Gender = request.Gender,
                Ethnicity = request.Ethnicity,
                PermanentAddress = request.PermanentAddress
            };

            return CreatedAtAction(nameof(GetStudent), new { id = request.StudentID }, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Có lỗi xảy ra",
                error = ex.Message
            });
        }
    }

    // DELETE: api/Student/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStudent(string? id)
    {
        if (id == null)
            return BadRequest("Mã sinh viên không được để trống.");

        await _studentService.DeleteAsync(id);

        return NoContent();
    }

    private bool StudentExists(string? id)
    {
        return _context.Students.Any(e => e.StudentID == id);
    }
}
