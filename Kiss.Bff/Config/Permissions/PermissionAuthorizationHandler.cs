using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Kiss.Bff.Config.Permissions
{
    class PermissionAuthorizationHandler(PermissionConfiguration configuration) : AuthorizationHandler<RequirePermissionAttribute>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RequirePermissionAttribute requirement)
        {
            var userRoles = context.User.Claims
                                          .Where(c => c.Type == ClaimsIdentity.DefaultRoleClaimType)
                                          .Select(c => c.Value);

            // Transform roles to permissions
            var userPermissions = configuration.GetPermissionsForRoles(userRoles);

            if (requirement.Permissions.Any(userPermissions.Contains))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}