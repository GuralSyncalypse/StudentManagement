using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.DTOs.Students;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LuongChiHai_QLSV.Server.Interfaces;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Student")]
public class StudentController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentController(IStudentService studentService)
        => _studentService = studentService;

    // GET api/Student/me
    [HttpGet("me")]
    public async Task<ActionResult<StudentResponseDto>> GetCurrentStudent()
    {
        var studentId = User.FindFirst(ClaimTypes.Name)?.Value
                     ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(studentId))
            return Unauthorized();

        // Service sẽ chịu trách nhiệm tìm và map sang DTO
        var studentDto = await _studentService.GetByIdAsync(studentId);
        if (studentDto == null)
            return NotFound();

        return Ok(studentDto);
    }
}