using Microsoft.AspNetCore.Authorization;

namespace LuongChiHai_QLSV.Server.Security
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }
        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }
}
