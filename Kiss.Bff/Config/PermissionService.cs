using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Kiss.Bff.Config.Permissions
{
    /// <summary>
    /// Extension methods for configuring permission-based authorization in the service collection.
    /// </summary>
    public static class PermissionsExtensions
    {
        /// <summary>
        /// Adds permission-based authorization services to the service collection.
        /// </summary>
        /// <param name="services">The service collection to add permissions to.</param>
        /// <param name="configure">An action to configure the permission transformer with role-to-permission mappings.</param>
        /// <returns>The service collection for method chaining.</returns>
        public static IServiceCollection AddPermissions(this IServiceCollection services, Action<PermissionTransformer> configure)
        {
            var transformer = new PermissionTransformer();
            configure(transformer);
            services.AddSingleton(transformer);
            return services;
        }
    }

    public enum HasPermissionTo
    {
        Skills,
        Beheer
    }

    /// <summary>
    /// Authorization attribute that restricts access based on one or more permissions.
    /// Can be applied to controllers or action methods to enforce permission-based authorization.
    /// </summary>
    public sealed class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(params HasPermissionTo[] permissions) : base(policy: string.Join(",", permissions.Select(p => p.ToString())))
        {
        }
    }

    /// <summary>
    /// Custom authorization policy provider that dynamically creates policies for permission-based authorization.
    /// </summary>
    public class PermissionAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
            : base(options) { }

        /// <summary>
        /// Gets the authorization policy with the specified name, or creates a new permission-based policy if none exists.
        /// </summary>
        /// <param name="policyName">The name of the policy to retrieve. For permission policies, this is a comma-separated list of permission names.</param>
        /// <returns>The authorization policy, or null if no policy could be created.</returns>
        public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            var policy = await base.GetPolicyAsync(policyName);

            if (policy is not null)
                return policy;

            var permissions = policyName.Split(',');

            return new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(permissions))
                .Build();
        }
    }

    /// <summary>
    /// Authorization handler that evaluates permission requirements based on user roles.
    /// </summary>
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly PermissionTransformer _permissionTransformer;

        public PermissionAuthorizationHandler(PermissionTransformer permissionTransformer)
        {
            _permissionTransformer = permissionTransformer;
        }

        /// <summary>
        /// Handles the authorization requirement by checking if the user has any of the required permissions.
        /// </summary>
        /// <param name="context">The authorization context containing user claims.</param>
        /// <param name="requirement">The permission requirement to evaluate.</param>
        /// <returns>A completed task.</returns>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var userRoles = context.User.Claims
                                          .Where(c => c.Type == ClaimsIdentity.DefaultRoleClaimType)
                                          .Select(c => c.Value);

            // Transform roles to permissions
            var userPermissions = userRoles
                .SelectMany(_permissionTransformer.GetPermissionsByRole)
                .Select(p => p.ToString());

            if (requirement.Permissions.Any(permission => userPermissions.Contains(permission)))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Represents an authorization requirement that specifies one or more required permissions.
    /// The user must have at least one of the specified permissions to satisfy this requirement.
    /// </summary>
    public class PermissionRequirement(IEnumerable<string> permissions) : IAuthorizationRequirement
    {
        /// <summary>
        /// Gets the collection of permission names required to satisfy this requirement.
        /// </summary>
        public IEnumerable<string> Permissions { get; } = permissions;
    }

    /// <summary>
    /// Transforms user roles into permissions by maintaining a mapping between roles and their associated permissions.
    /// </summary>
    public class PermissionTransformer
    {
        readonly Dictionary<string, HasPermissionTo[]> _permissionsToRole = [];

        /// <summary>
        /// Registers permissions for a specific role.
        /// </summary>
        /// <param name="permissions">The permissions to grant to the role.</param>
        /// <param name="role">The role name to register permissions for.</param>
        /// <returns>True if the registration was successful.</returns>
        public void RegisterPermissionsByRole(HasPermissionTo[] permissions, string role)
        {
            if (!_permissionsToRole.TryAdd(role, permissions))
            {
                _permissionsToRole.GetValueOrDefault(role)?.Concat(permissions);
            }
        }

        /// <summary>
        /// Retrieves all permissions associated with a specific role.
        /// </summary>
        /// <param name="role">The role name to get permissions for.</param>
        /// <returns>An array of permissions for the role, or an empty array if the role has no registered permissions.</returns>
        public HasPermissionTo[] GetPermissionsByRole(string role)
        {
            return !_permissionsToRole.ContainsKey(role) ? [] : _permissionsToRole.GetValueOrDefault(role) ?? [];
        }
    }
}