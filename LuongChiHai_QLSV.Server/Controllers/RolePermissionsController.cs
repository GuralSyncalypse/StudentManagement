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

            var assignedPermissionIds = await _context.RolePermissions
                .AsNoTracking()
                .Where(rp => rp.RoleID == selectedRoleId)
                .Select(rp => rp.PermissionID)
                .ToListAsync();

            var permissions = await _context.Permissions
                .AsNoTracking()
                .OrderBy(p => p.PermissionKey)
                .ToListAsync();

            var items = permissions
                .Select(permission =>
                {
                    var (resource, action) = SplitPermissionKey(permission.PermissionKey);
                    var normalizedAction = NormalizeAction(action);
                    var inCrudGroup = CrudOrder.Contains(normalizedAction);
                    return new
                    {
                        permission.PermissionID,
                        permission.PermissionKey,
                        permission.Description,
                        Resource = resource,
                        ResourceLabel = ToTitleCase(resource),
                        Action = normalizedAction,
                        IsAssigned = assignedPermissionIds.Contains(permission.PermissionID),
                        InCrudGroup = inCrudGroup
                    };
                })
                .Where(item => item.InCrudGroup)
                .ToList();

            var rows = items
                .GroupBy(item => item.Resource, StringComparer.OrdinalIgnoreCase)
                .Select(group => new RolePermissionMatrixRowDto
                {
                    Resource = group.Key,
                    ResourceLabel = group.First().ResourceLabel,
                    PermissionCount = group.Count(),
                    Cells = group
                        .OrderBy(item => Array.IndexOf(CrudOrder, item.Action))
                        .ThenBy(item => item.Action, StringComparer.OrdinalIgnoreCase)
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

            var permissionByKey = permissions
                .ToDictionary(p => p.PermissionKey, p => p, StringComparer.OrdinalIgnoreCase);

            var desiredKeys = (request.SelectedPermissionKeys ?? new List<string>())
                .Where(key => !string.IsNullOrWhiteSpace(key))
                .Select(key => key.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var invalidKeys = desiredKeys
                .Where(key => !permissionByKey.ContainsKey(key))
                .ToList();

            if (invalidKeys.Count > 0)
            {
                return BadRequest(new
                {
                    message = "Có quyền không hợp lệ trong danh sách gửi lên.",
                    invalidKeys
                });
            }

            var crudPermissionIds = permissions
                .Where(permission =>
                {
                    var (_, action) = SplitPermissionKey(permission.PermissionKey);
                    return CrudOrder.Contains(NormalizeAction(action));
                })
                .Select(permission => permission.PermissionID)
                .ToHashSet();

            var currentAssignments = await _context.RolePermissions
                .Where(rp => rp.RoleID == roleId)
                .ToListAsync();

            var currentAssignmentMap = currentAssignments.ToDictionary(rp => rp.PermissionID);

            foreach (var permission in permissions.Where(p => crudPermissionIds.Contains(p.PermissionID)))
            {
                var shouldBeAssigned = desiredKeys.Contains(permission.PermissionKey);
                var isCurrentlyAssigned = currentAssignmentMap.ContainsKey(permission.PermissionID);

                if (shouldBeAssigned && !isCurrentlyAssigned)
                {
                    _context.RolePermissions.Add(new RolePermission
                    {
                        RoleID = roleId,
                        PermissionID = permission.PermissionID
                    });
                }
                else if (!shouldBeAssigned && isCurrentlyAssigned)
                {
                    _context.RolePermissions.Remove(currentAssignmentMap[permission.PermissionID]);
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static (string Resource, string Action) SplitPermissionKey(string permissionKey)
        {
            if (string.IsNullOrWhiteSpace(permissionKey))
            {
                return (string.Empty, string.Empty);
            }

            var separatorIndex = permissionKey.IndexOf(':');
            if (separatorIndex < 0)
            {
                return (permissionKey.Trim(), string.Empty);
            }

            var resource = permissionKey[..separatorIndex].Trim();
            var action = permissionKey[(separatorIndex + 1)..].Trim();
            return (resource, action);
        }

        private static string NormalizeAction(string action)
        {
            return action.Trim().ToLowerInvariant();
        }

        private static string ToTitleCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Khác";
            }

            var words = value.Replace('_', ' ')
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(word => char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant());

            return string.Join(' ', words);
        }
    }
}
