using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.DTOs.Users;
using LuongChiHai_QLSV.Server.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LuongChiHai_QLSV.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly SchoolContext _context;

        public UsersController(SchoolContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserListDto>>> GetUsers(
            [FromQuery] string? search = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? role = null)
        {
            var query = _context.Users
                .AsNoTracking()
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalizedSearch = search.Trim().ToLower();
                query = query.Where(u =>
                    u.Username.ToLower().Contains(normalizedSearch) ||
                    u.Email.ToLower().Contains(normalizedSearch) ||
                    u.PhoneNumber.ToLower().Contains(normalizedSearch) ||
                    u.UserRoles.Any(ur => ur.Role.RoleName.ToLower().Contains(normalizedSearch)));
            }

            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(role) && !role.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(u => u.UserRoles.Any(ur => ur.Role.RoleName == role));
            }

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return Ok(users.Select(MapToListDto));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserDetailDto>> GetUser(int id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserID == id);

            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản." });
            }

            return Ok(MapToDetailDto(user));
        }

        [HttpPost]
        public async Task<ActionResult<UserDetailDto>> CreateUser([FromBody] CreateUserRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.PhoneNumber) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.RoleName))
            {
                return BadRequest(new { message = "Vui lòng nhập đầy đủ thông tin tài khoản." });
            }

            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại." });
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == request.RoleName);
            if (role == null)
            {
                return BadRequest(new { message = $"Vai trò '{request.RoleName}' không tồn tại." });
            }

            var user = new User
            {
                Username = request.Username.Trim(),
                Email = request.Email.Trim(),
                PhoneNumber = request.PhoneNumber.Trim(),
                PasswordHash = request.Password,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.UserRoles.Add(new UserRole
            {
                UserID = user.UserID,
                RoleID = role.RoleID
            });

            await _context.SaveChangesAsync();

            var createdUser = await _context.Users
                .AsNoTracking()
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstAsync(u => u.UserID == user.UserID);

            return CreatedAtAction(nameof(GetUser), new { id = user.UserID }, MapToDetailDto(createdUser));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.PhoneNumber) ||
                string.IsNullOrWhiteSpace(request.RoleName))
            {
                return BadRequest(new { message = "Vui lòng nhập đầy đủ thông tin tài khoản." });
            }

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.UserID == id);

            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản." });
            }

            if (await _context.Users.AnyAsync(u => u.Username == request.Username && u.UserID != id))
            {
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại." });
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == request.RoleName);
            if (role == null)
            {
                return BadRequest(new { message = $"Vai trò '{request.RoleName}' không tồn tại." });
            }

            user.Username = request.Username.Trim();
            user.Email = request.Email.Trim();
            user.PhoneNumber = request.PhoneNumber.Trim();
            user.IsActive = request.IsActive;

            _context.UserRoles.RemoveRange(user.UserRoles);
            _context.UserRoles.Add(new UserRole
            {
                UserID = user.UserID,
                RoleID = role.RoleID
            });

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (GetCurrentUserId() == id)
            {
                return BadRequest(new { message = "Bạn không thể xóa tài khoản đang đăng nhập." });
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản." });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UserStatusRequestDto request)
        {
            if (GetCurrentUserId() == id)
            {
                return BadRequest(new { message = "Bạn không thể tự khóa chính tài khoản đang đăng nhập." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == id);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản." });
            }

            user.IsActive = request.IsActive;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id:int}/change-password")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.CurrentPassword) ||
                string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new { message = "Mật khẩu hiện tại và mật khẩu mới không được để trống." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == id);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản." });
            }

            if (user.PasswordHash != request.CurrentPassword)
            {
                return BadRequest(new { message = "Mật khẩu hiện tại không chính xác." });
            }

            user.PasswordHash = request.NewPassword;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id:int}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new { message = "Mật khẩu mới không được để trống." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == id);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản." });
            }

            user.PasswordHash = request.NewPassword;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private int? GetCurrentUserId()
        {
            var rawId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(rawId, out var id) ? id : null;
        }

        private static UserListDto MapToListDto(User user)
        {
            return new UserListDto
            {
                UserID = user.UserID,
                Username = user.Username,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                RoleNames = user.UserRoles
                    .Select(ur => ur.Role.RoleName)
                    .Distinct()
                    .OrderBy(roleName => roleName)
                    .ToList()
            };
        }

        private static UserDetailDto MapToDetailDto(User user)
        {
            var dto = MapToListDto(user);

            return new UserDetailDto
            {
                UserID = dto.UserID,
                Username = dto.Username,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                IsActive = dto.IsActive,
                CreatedAt = dto.CreatedAt,
                RoleNames = dto.RoleNames
            };
        }
    }
} 
