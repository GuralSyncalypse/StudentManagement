using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.DTOs.Auths;
using LuongChiHai_QLSV.Server.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LuongChiHai_QLSV.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SchoolContext _context; // Thay bằng DbContext của bạn
        private readonly IConfiguration _config;

        public AuthController(SchoolContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            // 1. Kiểm tra tài khoản đã tồn tại chưa
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại trong hệ thống!" });
            }

            // 2. Nếu đăng ký quyền Student, kiểm tra tính hợp lệ của mã sinh viên
            if (request.RoleName.Equals("Student", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(request.StudentID) || string.IsNullOrEmpty(request.StudentName))
                {
                    return BadRequest(new { message = "Thông tin Mã sinh viên và Họ tên không được để trống!" });
                }

                if (await _context.Students.AnyAsync(s => s.StudentID == request.StudentID))
                {
                    return BadRequest(new { message = "Mã sinh viên này đã tồn tại!" });
                }
            }

            // 3. Khởi tạo Transaction để bảo vệ toàn vẹn dữ liệu liên kết
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Bước A: Tạo thực thể User mới
                var newUser = new User
                {
                    Username = request.Username,
                    PasswordHash = request.Password, // Thực tế nên dùng BCrypt.Net để HashPassword
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync(); // Lưu trước để sinh ra UserID tự động

                // Bước B: Tìm và gán Role cho User
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == request.RoleName);
                if (role == null)
                {
                    return BadRequest(new { message = $"Quyền hệ thống '{request.RoleName}' không tồn tại!" });
                }

                var userRole = new UserRole { UserID = newUser.UserID, RoleID = role.RoleID };
                _context.UserRoles.Add(userRole);

                // Bước C: Xử lý tạo thực thể Sinh viên nếu quyền là Student
                if (request.RoleName.Equals("Student", StringComparison.OrdinalIgnoreCase))
                {
                    var newStudent = new Student
                    {
                        StudentID = request.StudentID!,
                        UserID = newUser.UserID,
                        StudentName = request.StudentName!,
                        Gender = request.Gender ?? null,
                    };
                    _context.Students.Add(newStudent);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync(); // Xác nhận lưu vĩnh viễn vào DB

                return Ok(new { message = "Đăng ký tài khoản thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Hủy bỏ mọi thay đổi nếu dính lỗi
                return StatusCode(500, new { message = "Có lỗi xảy ra trong quá trình xử lý!", error = ex.Message });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            // 1. Tìm user kèm theo danh sách Roles của họ
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            // 2. Kiểm tra tài khoản và mã hóa password (ở đây viết đơn giản, thực tế nên dùng BCrypt)
            if (user == null || user.PasswordHash != request.Password || !user.IsActive)
            {
                return Unauthorized(new { message = $"Tài khoản hoặc mật khẩu không chính xác!" });
            }

            // 3. Tạo danh sách các Claims quyền hạn
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            // Add tất cả các role của user vào Claim
            foreach (var userRole in user.UserRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole.Role.RoleName));
            }

            // 4. Ký và sinh chuỗi Token JWT
            var jwtSettings = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(4), // Token có giá trị trong 4 tiếng
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(token);

            return Ok(new AuthResponseDto
            {
                Token = tokenHandler.WriteToken(securityToken),
                Username = user.Username
            });
        }

    }
}
