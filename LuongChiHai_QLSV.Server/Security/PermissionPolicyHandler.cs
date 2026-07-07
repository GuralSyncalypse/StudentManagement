using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace LuongChiHai_QLSV.Server.Security
{
    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => FallbackPolicyProvider.GetDefaultPolicyAsync();
        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => FallbackPolicyProvider.GetFallbackPolicyAsync();

        // Hệ thống sẽ tự động nhảy vào hàm này khi gặp [Authorize(Policy = "xxx")]
        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            // Tạo động một Policy yêu cầu PermissionRequirement truyền vào chính tên Policy đó
            var policy = new AuthorizationPolicyBuilder();
            policy.AddRequirements(new PermissionRequirement(policyName));
            return Task.FromResult<AuthorizationPolicy?>(policy.Build());
        }
    }
}
