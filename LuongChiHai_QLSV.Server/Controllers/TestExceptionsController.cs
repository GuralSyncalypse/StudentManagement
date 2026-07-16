using Microsoft.AspNetCore.Mvc;

namespace LuongChiHai_QLSV.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestExceptionController : ControllerBase
    {
        // 1. Test lỗi 404 (KeyNotFoundException)
        [HttpGet("not-found")]
        public IActionResult GetNotFound()
        {
            throw new KeyNotFoundException("Không tìm thấy Sinh viên có mã số SV999.");
        }

        // 2. Test lỗi 401 (UnauthorizedAccessException)
        [HttpGet("unauthorized")]
        public IActionResult GetUnauthorized()
        {
            throw new UnauthorizedAccessException();
        }

        // 3. Test lỗi 500 (Lỗi hệ thống bất kỳ)
        [HttpGet("server-error")]
        public IActionResult GetServerError()
        {
            // Cố tình chia cho 0 để gây ra lỗi hệ thống chung
            int a = 10;
            int b = 0;
            int result = a / b;
            return Ok(result);
        }
    }
}