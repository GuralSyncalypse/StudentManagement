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
