using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Kiss.Bff.Config.Permissions
{
    class PermissionAuthorizationHandler(PermissionConfiguration configuration) : AuthorizationHandler<RequirePermissionAttribute>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RequirePermissionAttribute requirement)
        {
            if (requirement.Permissions.Any(configuration.GetPermissions(context.User).Contains))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}