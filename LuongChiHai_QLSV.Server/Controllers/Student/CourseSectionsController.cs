using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LuongChiHai_QLSV.Server.Controllers.Student
{
    [Route("api/student/course-sections")]
    [ApiController]
    [Authorize(Roles = "Student")]
    public class CourseSectionsController : ControllerBase
    {
        private readonly SchoolContext _context;

        public CourseSectionsController(SchoolContext context)
        {
            _context = context;
        }

        // GET: api/student/course-sections
        // Sinh viên chỉ xem danh sách các lớp ĐANG MỞ (Open) và CHƯA BỊ ĐẦY SĨ SỐ
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseSectionDto>>> GetAvailableSections()
        {
            var data = await _context.CourseSections
                .Where(s => s.Status == "Open" && s.Enrollments.Count < s.MaxCapacity)
                .Select(s => new CourseSectionDto
                {
                    SectionID = s.SectionID,
                    CourseID = s.CourseID,
                    Semester = s.Semester,
                    ClassSection = s.ClassSection,
                    MaxCapacity = s.MaxCapacity,
                    Status = s.Status,
                    CourseName = s.Course != null ? s.Course.CourseName : null,
                    CurrentEnrollment = s.Enrollments.Count
                }).ToListAsync();

            return Ok(data);
        }
    }
}
