using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.DTOs.Auths;
using LuongChiHai_QLSV.Server.Entities;
using LuongChiHai_QLSV.Server.Helpers;
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
        private readonly SchoolContext _context; 
        private readonly IConfiguration _config;
        private readonly TokenService _tokenService;

        public AuthController(SchoolContext context, IConfiguration config, TokenService tokenService)
        {
            _context = context;
            _config = config;
            _tokenService = tokenService;
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
                    var newStudent = new Entities.Student
                    {
                        StudentID = request.StudentID!,
                        UserID = newUser.UserID,
                        StudentName = request.StudentName!,
                        Gender = request.Gender ?? null,
                    };
                    _context.Students.Add(newStudent);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Đăng ký tài khoản thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
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

            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserID == user.UserID);

            if (student != null)
            {
                claims.Add(new Claim("StudentID", student.StudentID));
            }

            // 4. Ký và sinh chuỗi Token JWT
            var jwtSettings = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(5),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(token);
            var accessTokenString = tokenHandler.WriteToken(securityToken);

            // ==========================================
            // 5. TẠO REFRESH TOKEN VÀ LƯU VÀO DATABASE
            // ==========================================
            var refreshTokenString = _tokenService.GenerateRefreshToken();

            var userAgent = Request.Headers["User-Agent"].ToString();
            var deviceFriendlyName = DeviceDetector.GetDeviceFriendlyName(userAgent);

            // 3. CHỈ VÔ HIỆU HÓA các token cũ của User này TRÊN CÙNG THIẾT BỊ NÀY
            var existingTokensOnDevice = await _context.UserRefreshTokens
                .Where(t => t.UserID == user.UserID
                         && t.Device == deviceFriendlyName
                         && !t.IsRevoked
                         && t.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();

            foreach (var oldToken in existingTokensOnDevice)
            {
                oldToken.IsRevoked = true;
                oldToken.RevokedAt = DateTime.UtcNow;
            }

            // 4. Lưu Refresh Token mới cùng thông tin thiết bị
            var refreshTokenEntity = new UserRefreshToken
            {
                UserID = user.UserID,
                Token = refreshTokenString,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Device = deviceFriendlyName, // Lưu thiết bị dễ đọc
                UserAgent = userAgent        // Lưu raw user-agent
            };

            _context.UserRefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            // ==========================================
            // 6. THIẾT LẬP COOKIE HTTP-ONLY CHỨA REFRESH TOKEN
            // ==========================================
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,        // Javascript ở Frontend (Angular) không thể đọc => Chống XSS độc hại
                Secure = true,          // Chỉ truyền qua HTTPS (Localhost phát triển vẫn tự động chạy được)
                SameSite = SameSiteMode.Strict, // Chống tấn công giả mạo yêu cầu chéo trang CSRF
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", refreshTokenString, cookieOptions);

            return Ok(new AuthResponseDto
            {
                Token = accessTokenString,
                Username = user.Username
            });
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken()
        {
            // 1. Lấy Refresh Token từ HttpOnly Cookie
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(new { message = "Không tìm thấy Refresh Token!" });
            }

            // 2. Kiểm tra token trong DB kèm thông tin User & Roles
            var tokenInDb = await _context.UserRefreshTokens
                .Include(t => t.User)
                    .ThenInclude(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                .SingleOrDefaultAsync(t => t.Token == refreshToken);

            // 3. Xác thực tính hợp lệ của Token cũ
            if (tokenInDb == null)
            {
                return Unauthorized(new { message = "Refresh Token không tồn tại!" });
            }

            // PHÁT HIỆN GIAN LẬN: Nếu token đã dùng rồi (IsRevoked = true) mà lại gửi lên tiếp
            if (tokenInDb.IsRevoked)
            {
                // Thu hồi TẤT CẢ token đang hoạt động của user này để đảm bảo an toàn
                var activeTokens = await _context.UserRefreshTokens
                    .Where(t => t.UserID == tokenInDb.UserID && !t.IsRevoked)
                    .ToListAsync();

                foreach (var t in activeTokens)
                {
                    t.IsRevoked = true;
                    t.RevokedAt = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();

                return Unauthorized(new { message = "Cảnh báo bảo mật! Vui lòng đăng nhập lại." });
            }

            if (tokenInDb.ExpiresAt < DateTime.UtcNow)
            {
                return Unauthorized(new { message = "Refresh Token đã hết hạn!" });
            }

            // =========================================================
            // 4. XOAY VÒNG TOKEN (ROTATION): Vô hiệu hóa token cũ vừa dùng
            // =========================================================
            tokenInDb.IsRevoked = true;
            tokenInDb.RevokedAt = DateTime.UtcNow;

            // 5. Tái tạo danh sách Claims cho Access Token mới
            var user = tokenInDb.User;
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            foreach (var userRole in user.UserRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole.Role.RoleName));
            }

            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserID == user.UserID);

            if (student != null)
            {
                claims.Add(new Claim("StudentID", student.StudentID));
            }

            // 6. Ký và sinh chuỗi Access Token (JWT) mới
            var jwtSettings = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15), // Hạn Access Token
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var newAccessTokenString = tokenHandler.WriteToken(securityToken);

            // =========================================================
            // 7. TẠO REFRESH TOKEN MỚI & CẬP NHẬT THÔNG TIN THIẾT BỊ
            // =========================================================
            var newRefreshTokenString = _tokenService.GenerateRefreshToken();

            // Lấy thông tin thiết bị tại thời điểm refresh (đề phòng user vừa cập nhật trình duyệt)
            var userAgent = Request.Headers["User-Agent"].ToString();
            var deviceFriendlyName = DeviceDetector.GetDeviceFriendlyName(userAgent);

            var newRefreshTokenEntity = new UserRefreshToken
            {
                UserID = user.UserID,
                Token = newRefreshTokenString, // Đồng nhất sử dụng CHUNG 1 chuỗi token mới
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Device = deviceFriendlyName,
                UserAgent = userAgent
            };

            _context.UserRefreshTokens.Add(newRefreshTokenEntity);
            await _context.SaveChangesAsync();

            // 8. Đè Cookie cũ bằng Cookie chứa Refresh Token MỚI
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", newRefreshTokenString, cookieOptions);

            // 9. Trả Access Token mới về cho Angular
            return Ok(new AuthResponseDto
            {
                Token = newAccessTokenString,
                Username = user.Username
            });
        }

        [HttpPost("logout")]
        [AllowAnonymous] // Nên dùng AllowAnonymous vì khi bấm Logout, Access Token của Client có thể đã hết hạn.
        public async Task<IActionResult> Logout()
        {
            // 1. Lấy Refresh Token từ HttpOnly Cookie do Angular gửi lên (nhờ withCredentials)
            var refreshToken = Request.Cookies["refreshToken"];

            if (!string.IsNullOrEmpty(refreshToken))
            {
                // 2. Tìm đúng Token đó trong Database
                var tokenInDb = await _context.UserRefreshTokens
                    .SingleOrDefaultAsync(t => t.Token == refreshToken);

                // 3. Nếu tìm thấy và token chưa bị hủy, tiến hành thu hồi nó
                if (tokenInDb != null && !tokenInDb.IsRevoked)
                {
                    tokenInDb.IsRevoked = true;
                    tokenInDb.RevokedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                }
            }

            // 4. Đuổi Cookie khỏi trình duyệt Client
            // Việc này sẽ set Expired của cookie về quá khứ, ép trình duyệt tự động xóa bỏ hoàn toàn
            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            return Ok(new { message = "Đăng xuất và xóa phiên làm việc thành công!" });
        }
    }
}
