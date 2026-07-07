using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.DTOs;
using LuongChiHai_QLSV.Server.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LuongChiHai_QLSV.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BaseController : ControllerBase
    {
        private readonly SchoolContext _context;
        public BaseController(SchoolContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var totalSections = await _context.CourseSections.CountAsync();
            var totalStudents = await _context.Students.CountAsync();
            var totalCourses = await _context.Courses.CountAsync();

            return Ok(new
            {
                TotalSections = totalSections,
                TotalStudents = totalStudents,
                TotalCourses = totalCourses
            });
        }

        
    }
}
