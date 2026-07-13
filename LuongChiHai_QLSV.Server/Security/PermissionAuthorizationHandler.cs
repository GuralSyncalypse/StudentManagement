using LuongChiHai_QLSV.Server.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LuongChiHai_QLSV.Server.Security
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly SchoolContext _context; // Inject DbContext của bạn vào đây

        public PermissionAuthorizationHandler(SchoolContext context)
        {
            _context = context;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            // 1. Lấy UserID từ JWT Claims
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return;

            if (!int.TryParse(userIdClaim.Value, out int userId)) return;

            // 2. Kiểm tra DB xem User có Permission này không thông qua RolePermission.
            var hasPermission = await _context.UserRoles
                .Where(ur => ur.UserID == userId)
                .AnyAsync(ur => ur.Role.RolePermissions.Any(rp => rp.Permission.PermissionID == requirement.Permission));

            // 3. Nếu hợp lệ thì cho qua
            if (hasPermission)
            {
                context.Succeed(requirement);
            }
        }
    }
}
