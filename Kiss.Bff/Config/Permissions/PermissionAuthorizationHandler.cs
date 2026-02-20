using Microsoft.AspNetCore.Authorization;

namespace Kiss.Bff.Config.Permissions
{
    class PermissionAuthorizationHandler(PermissionConfiguration configuration) : AuthorizationHandler<RequirePermissionAttribute>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RequirePermissionAttribute requirement)
        {
            if (configuration.GetPermissions(context.User).Contains(requirement.Permissions))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}