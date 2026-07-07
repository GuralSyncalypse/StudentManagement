using Microsoft.AspNetCore.Authorization;

namespace LuongChiHai_QLSV.Server.Security
{
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(string permission) : base(policy: permission)
        {
        }
    }
}
