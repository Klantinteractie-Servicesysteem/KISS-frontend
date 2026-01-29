using Kiss.Bff.Config.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

public class PermissionAuthorizationPolicyProvider : IAuthorizationPolicyProvider
{
    public const string Prefix = "Permission:";
    private DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

    public PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    /// <summary>
    /// Checks if policy name starts with the required prefix and transforms the string into RequiredPermissionTo[].
    /// If there are no permissions found the PolicyProvider will revert to the 
    /// </summary>
    /// <param name="policyName">The policy name in the format: prefix:permission1,permission2...</param>
    /// <returns></returns>
    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
        {
            var permissions = policyName[Prefix.Length..]
                .Split(',')
                .Select(Enum.Parse<RequirePermissionTo>)
                .ToArray();

            if (permissions.Length > 0)
            {
                var policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new RequirePermissionAttribute(permissions));
                return Task.FromResult<AuthorizationPolicy?>(policy.Build());
            }
        }
        return FallbackPolicyProvider.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => FallbackPolicyProvider.GetDefaultPolicyAsync();
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => FallbackPolicyProvider.GetFallbackPolicyAsync();
}