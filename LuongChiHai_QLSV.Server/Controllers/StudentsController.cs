using Azure.Core;
using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.DTOs.Auths;
using LuongChiHai_QLSV.Server.DTOs.Students;
using LuongChiHai_QLSV.Server.Entities;
using LuongChiHai_QLSV.Server.Interfaces;
using LuongChiHai_QLSV.Server.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


[Route("api/[controller]")]
[ApiController]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;
    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    // GET: api/Student
    [HttpGet]
    [HasPermission("0402")]
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
    [HasPermission("0402")]
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
    [HasPermission("0403")]
    public async Task<IActionResult> PutStudent(string id, StudentRequestDto request)
    {
        if (id != request.StudentID)
            return BadRequest("Mã cấu trúc không khớp.");

        var isUpdated = await _studentService.UpdateAsync(id, request);

        if (!isUpdated)
            return NotFound($"Không tìm thấy sinh viên có mã {id}.");

        return NoContent();
    }

    // POST: api/Student
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    [HasPermission("0401")]
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

    [HttpGet("me")]
    [HasPermission("0402")]
    public async Task<ActionResult<StudentResponseDto>> GetCurrentStudent()
    {
        var studentId = User.FindFirst("StudentID")?.Value;

        if (string.IsNullOrEmpty(studentId))
            return Unauthorized();

        // Service sẽ chịu trách nhiệm tìm và map sang DTO
        var studentDto = await _studentService.GetByIdAsync(studentId);
        if (studentDto == null)
            return NotFound();

        return Ok(studentDto);
    }

    // DELETE: api/Student/5
    [HttpDelete("{id}")]
    [HasPermission("0404")]
    public async Task<IActionResult> DeleteStudent(string? id)
    {
        if (id == null)
            return BadRequest("Mã sinh viên không được để trống.");

        await _studentService.DeleteAsync(id);

        return NoContent();
    }

}
