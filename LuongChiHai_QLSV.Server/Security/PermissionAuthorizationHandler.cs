using LuongChiHai_QLSV.Server.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LuongChiHai_QLSV.Server.Security
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly SchoolContext _context;

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

            var directPermission = await _context.UserPermissions
            .AsNoTracking()
            .FirstOrDefaultAsync(up => up.UserID == userId && up.PermissionID == requirement.Permission);

            if (directPermission != null)
            {
                // NẾU CÓ QUYỀN RIÊNG -> Áp dụng luật ghi đè hoàn toàn, không cần check Role nữa
                if (directPermission.IsAllowed)
                {
                    context.Succeed(requirement); // Cho qua nếu IsAllowed = true
                }
                else
                {
                    // Nếu IsAllowed = false tức là cấm tuyệt đối (Blacklist), return ngay để chặn lại
                    // (Kể cả role của user này có quyền thì quyền riêng false vẫn thắng)
                    return;
                }

                return;
            }

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
