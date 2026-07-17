using LuongChiHai_QLSV.Server.DTOs;
using LuongChiHai_QLSV.Server.Entities;
using LuongChiHai_QLSV.Server.Interfaces;
using LuongChiHai_QLSV.Server.Security;
using LuongChiHai_QLSV.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpGet("my-enrollments")]
    [HasPermission("student:view_own_grades")]
    public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetMyEnrollments()
    {
        var studentId = GetStudentId();
        if (studentId == null)
        {
            return Unauthorized("Student information was not found.");
        }

        var result = await _enrollmentService.GetMyEnrollmentsAsync(studentId);
        return Ok(result);
    }

    [HttpGet]
    [HasPermission("0802")]
    public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetEnrollments()
    {
        var result = await _enrollmentService.GetAllEnrollmentsAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [HasPermission("0802")]
    public async Task<ActionResult<EnrollmentDto>> GetEnrollment(int id)
    {
        var result = await _enrollmentService.GetEnrollmentAsync(id);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPut("{id}")]
    [HasPermission("0803")]
    public async Task<IActionResult> PutEnrollment(int? id, Enrollment enrollment)
    {
        if (id == null || id != enrollment.EnrollmentID)
        {
            return BadRequest();
        }

        try
        {
            var result = await _enrollmentService.UpdateEnrollmentAsync(id.Value, enrollment);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    [HasPermission("0801")]
    public async Task<IActionResult> RegisterForStudent([FromBody] AdminRegistrationDto dto)
    {
        try
        {
            var result = await _enrollmentService.RegisterForStudentAsync(dto);
            return CreatedAtAction(nameof(GetEnrollment), new { id = result.EnrollmentID }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [HasPermission("0804")]
    public async Task<IActionResult> DeleteEnrollment(int id)
    {
        try
        {
            await _enrollmentService.DeleteEnrollmentAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete]
    [Authorize(Roles = "Admin")]
    [HasPermission("0804")]
    public async Task<IActionResult> DeleteEnrollmentByAdmin([FromQuery][Required] int sectionID, [FromQuery][Required] string studentID)
    {
        try
        {
            await _enrollmentService.AdminCancelEnrollmentAsync(sectionID, studentID);
            return Ok(new { message = "Enrollment was cancelled successfully." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private string? GetStudentId()
    {
        return User.FindFirst("StudentID")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
    }
}
