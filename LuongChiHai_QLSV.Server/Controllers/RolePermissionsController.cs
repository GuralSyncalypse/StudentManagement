using LuongChiHai_QLSV.Server.Data;
using LuongChiHai_QLSV.Server.DTOs.Permissions;
using LuongChiHai_QLSV.Server.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LuongChiHai_QLSV.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RolePermissionsController : ControllerBase
    {
        private static readonly string[] CrudOrder = ["create", "read", "update", "delete"];

        // Định nghĩa bộ danh mục Resource dựa theo 2 ký tự đầu của PermissionID
        private static readonly Dictionary<string, (string ResourceName, string ResourceLabel)> ResourceMapping = new()
    {
        { "01", ("user", "Tài khoản") },
        { "02", ("role", "Vai trò") },
        { "03", ("permission", "Quyền hạn") },
        { "04", ("student", "Sinh viên") },
        { "05", ("academic_profile", "Hồ sơ học thuật") },
        { "06", ("course", "Khóa học") },
        { "07", ("course_section", "Lớp học phần") },
        { "08", ("enrollment", "Đăng ký học phần") },
        { "09", ("score", "Điểm số") }
    };

        // Định nghĩa bộ Action tương ứng với 2 ký tự cuối của PermissionID
        private static readonly Dictionary<string, string> ActionMapping = new()
    {
        { "01", "create" },
        { "02", "read" },
        { "03", "update" },
        { "04", "delete" }
    };

        private readonly SchoolContext _context;

        public RolePermissionsController(SchoolContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<RolePermissionMatrixResponseDto>> GetMatrix([FromQuery] int? roleId = null)
        {
            var roles = await _context.Roles
                .AsNoTracking()
                .OrderBy(r => r.RoleName)
                .Select(r => new RolePermissionRoleOptionDto
                {
                    RoleID = r.RoleID,
                    RoleName = r.RoleName
                })
                .ToListAsync();

            if (roles.Count == 0)
            {
                return Ok(new RolePermissionMatrixResponseDto());
            }

            var selectedRoleId = roleId ?? roles.First().RoleID;

            var selectedRole = await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RoleID == selectedRoleId);

            if (selectedRole == null)
            {
                return NotFound(new { message = "Không tìm thấy role được chọn." });
            }

            // Lấy danh sách PermissionID đã gán cho Role hiện tại
            var assignedPermissionIds = await _context.RolePermissions
                .AsNoTracking()
                .Where(rp => rp.RoleID == selectedRoleId)
                .Select(rp => rp.PermissionID)
                .ToListAsync();

            // Lấy toàn bộ danh sách Quyền từ Database
            var permissions = await _context.Permissions
                .AsNoTracking()
                .OrderBy(p => p.PermissionID)
                .ToListAsync();

            var items = permissions
                .Select(permission =>
                {
                    var (resource, label, action) = ParsePermissionId(permission.PermissionID);
                    var inCrudGroup = CrudOrder.Contains(action);

                    return new
                    {
                        permission.PermissionID,
                        // Giữ lại sinh PermissionKey tự động (dạng "user:create") để tránh lỗi giao diện Frontend (nếu có)
                        PermissionKey = string.IsNullOrEmpty(resource) ? permission.PermissionID : $"{resource}:{action}",
                        permission.Description,
                        Resource = resource ?? "unknown",
                        ResourceLabel = label ?? "Khác",
                        Action = action,
                        IsAssigned = assignedPermissionIds.Contains(permission.PermissionID),
                        InCrudGroup = inCrudGroup
                    };
                })
                .Where(item => item.InCrudGroup)
                .ToList();

            // Nhóm theo nhóm chức năng (Resource) để xuất ra ma trận dòng x cột
            var rows = items
                .GroupBy(item => item.Resource, StringComparer.OrdinalIgnoreCase)
                .Select(group => new RolePermissionMatrixRowDto
                {
                    Resource = group.Key,
                    ResourceLabel = group.First().ResourceLabel,
                    PermissionCount = group.Count(),
                    Cells = group
                        .OrderBy(item => Array.IndexOf(CrudOrder, item.Action))
                        .Select(item => new RolePermissionMatrixCellDto
                        {
                            PermissionID = item.PermissionID,
                            PermissionKey = item.PermissionKey,
                            Action = item.Action,
                            Description = item.Description,
                            IsAssigned = item.IsAssigned
                        })
                        .ToList()
                })
                .OrderBy(row => row.ResourceLabel, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return Ok(new RolePermissionMatrixResponseDto
            {
                Roles = roles,
                SelectedRoleID = selectedRoleId,
                Rows = rows
            });
        }

        [HttpGet("exclusive")]
        public async Task<ActionResult<UserPermissionMatrixResponseDto>> GetExclusiveMatrix([FromQuery] int? userId = null)
        {
            // 1. Lấy danh sách người dùng để làm Option cho dropdown ở Frontend
            var users = await _context.Users
                .AsNoTracking()
                .OrderBy(u => u.Username)
                .Select(u => new UserPermissionUserOptionDto
                {
                    UserID = u.UserID,
                    Username = u.Username
                })
                .ToListAsync();

            if (users.Count == 0)
            {
                return Ok(new UserPermissionMatrixResponseDto
                {
                    Users = new(),
                    Rows = new()
                });
            }

            // 2. Xác định User được chọn (mặc định lấy User đầu tiên nếu không truyền userId)
            var selectedUserId = userId ?? users.First().UserID;

            var selectedUserExists = await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.UserID == selectedUserId);

            if (!selectedUserExists)
            {
                return NotFound(new { message = "Không tìm thấy user được chọn." });
            }

            // 3. Lấy danh sách quyền riêng biệt đã gán cho User hiện tại
            var assignedPermissions = await _context.UserPermissions
                .AsNoTracking()
                .Where(up => up.UserID == selectedUserId)
                .Select(up => new UserPermissionDto
                {
                    UserId = up.UserID,
                    PermissionId = up.PermissionID,
                    IsAllowed = up.IsAllowed
                })
                .ToListAsync();

            // Chuyển sang Dictionary tối ưu hóa việc tìm kiếm O(1)
            var assignedPermissionDict = assignedPermissions
                .ToDictionary(ap => ap.PermissionId, ap => ap.IsAllowed);

            // 4. Lấy toàn bộ danh sách Quyền gốc có trong hệ thống
            var permissions = await _context.Permissions
                .AsNoTracking()
                .OrderBy(p => p.PermissionID)
                .ToListAsync();

            // 5. Khớp dữ liệu (Map) ra danh sách hiển thị
            var rows = permissions
                .Select(permission =>
                {
                    bool isAssigned = assignedPermissionDict.ContainsKey(permission.PermissionID);
                    bool isAllowed = isAssigned && assignedPermissionDict[permission.PermissionID];

                    return new ExclusivePermissionRowDto
                    {
                        PermissionID = permission.PermissionID,
                        Description = permission.Description,
                        IsAssigned = isAssigned,
                        IsAllowed = isAllowed
                    };
                })
                .ToList();

            // 6. Trả về Response DTO chuẩn hóa
            return Ok(new UserPermissionMatrixResponseDto
            {
                Users = users,
                SelectedUserID = selectedUserId,
                Rows = rows
            });
        }

        [HttpPut("{roleId:int}")]
        public async Task<IActionResult> SaveMatrix(int roleId, [FromBody] SaveRolePermissionMatrixRequestDto request)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleID == roleId);
            if (role == null)
            {
                return NotFound(new { message = "Không tìm thấy role được chọn." });
            }

            var permissions = await _context.Permissions
                .AsNoTracking()
                .ToListAsync();

            // Lưu ý: Đổi logic kiểm tra từ PermissionKey sang nhận diện trực tiếp bằng PermissionID (Chuỗi số 4 ký tự)
            var desiredIds = (request.SelectedPermissionIds ?? new List<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct()
                .ToHashSet();

            var validPermissionIds = permissions.Select(p => p.PermissionID).ToHashSet();
            var invalidIds = desiredIds.Where(id => !validPermissionIds.Contains(id)).ToList();

            if (invalidIds.Count > 0)
            {
                return BadRequest(new
                {
                    message = "Có mã quyền (PermissionID) không hợp lệ trong danh sách gửi lên.",
                    invalidIds
                });
            }

            // Lấy danh sách các PermissionID thuộc phạm vi quản lý CRUD để xử lý đồng bộ gán/gỡ gán
            var crudPermissionIds = permissions
                .Where(p =>
                {
                    var (_, _, action) = ParsePermissionId(p.PermissionID);
                    return CrudOrder.Contains(action);
                })
                .Select(p => p.PermissionID)
                .ToHashSet();

            var currentAssignments = await _context.RolePermissions
                .Where(rp => rp.RoleID == roleId)
                .ToListAsync();

            var currentAssignmentMap = currentAssignments.ToDictionary(rp => rp.PermissionID);

            foreach (var pId in crudPermissionIds)
            {
                var shouldBeAssigned = desiredIds.Contains(pId);
                var isCurrentlyAssigned = currentAssignmentMap.ContainsKey(pId);

                if (shouldBeAssigned && !isCurrentlyAssigned)
                {
                    _context.RolePermissions.Add(new RolePermission
                    {
                        RoleID = roleId,
                        PermissionID = pId
                    });
                }
                else if (!shouldBeAssigned && isCurrentlyAssigned)
                {
                    _context.RolePermissions.Remove(currentAssignmentMap[pId]);
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }


        [HttpPost("save-permissions")]
        public async Task<IActionResult> SaveUserPermissions([FromBody] UpdateUserPermissionsDto model)
        {
            if (model == null || model.UserID <= 0)
            {
                return BadRequest(new { message = "Dữ liệu yêu cầu không hợp lệ." });
            }

            // 1. Kiểm tra sự tồn tại của User
            var userExists = await _context.Users.AnyAsync(u => u.UserID == model.UserID);
            if (!userExists)
            {
                return NotFound(new { message = "Không tìm thấy người dùng trong hệ thống." });
            }

            // 2. Lấy toàn bộ quyền hiện tại của User này từ Database ra để so sánh
            var existingUserPermissions = await _context.UserPermissions
                .Where(up => up.UserID == model.UserID)
                .ToListAsync();

            // Chuyển danh sách hiện tại thành Dictionary để tra cứu nhanh theo PermissionID
            var existingDict = existingUserPermissions.ToDictionary(up => up.PermissionID);

            // 3. Duyệt qua danh sách thay đổi gửi lên từ Frontend
            foreach (var item in model.Permissions)
            {
                bool exists = existingDict.TryGetValue(item.PermissionID, out var existingPermission);

                if (item.IsAssigned)
                {
                    if (exists)
                    {
                        // THỘP: Đã tồn tại -> Cập nhật lại thuộc tính IsAllowed nếu có thay đổi
                        if (existingPermission!.IsAllowed != item.IsAllowed)
                        {
                            existingPermission.IsAllowed = item.IsAllowed;
                            _context.Entry(existingPermission).State = EntityState.Modified;
                        }
                    }
                    else
                    {
                        // THÊM MỚI: Chưa tồn tại và được gán -> Thêm mới bản ghi vào bảng trung gian
                        var newPermission = new UserPermission
                        {
                            UserID = model.UserID,
                            PermissionID = item.PermissionID,
                            IsAllowed = item.IsAllowed
                        };
                        await _context.UserPermissions.AddAsync(newPermission);
                    }
                }
                else
                {
                    // XÓA: Nếu tồn tại trong DB nhưng Frontend gửi lên IsAssigned = false -> Xóa khỏi DB
                    if (exists)
                    {
                        _context.UserPermissions.Remove(existingPermission!);
                    }
                }
            }

            // 4. Lưu tất cả thay đổi vào Database (bọc trong Transaction ngầm định của SaveChangesAsync)
            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Cập nhật ma trận phân quyền thành công!" });
            }
            catch (DbUpdateException ex)
            {
                // Log lỗi tại đây (ví dụ: _logger.LogError(ex, "..."))
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lưu dữ liệu phân quyền.", detail = ex.Message });
            }
        }

        /// <summary>
        /// Hàm helper hỗ trợ bóc tách PermissionID (4 ký tự) thành Resource và Action chuẩn chỉ.
        /// </summary>
        private static (string? Resource, string? Label, string Action) ParsePermissionId(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || id.Length != 4)
            {
                return (null, null, "unknown");
            }

            var prefix = id.Substring(0, 2);
            var suffix = id.Substring(2, 2);

            ResourceMapping.TryGetValue(prefix, out var resourceInfo);
            ActionMapping.TryGetValue(suffix, out var action);

            return (resourceInfo.ResourceName, resourceInfo.ResourceLabel, action ?? "unknown");
        }
    }
}
