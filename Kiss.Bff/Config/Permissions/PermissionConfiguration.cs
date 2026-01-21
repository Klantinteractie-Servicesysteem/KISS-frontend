using System.Collections;

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
        public static IServiceCollection AddPermissions(this IServiceCollection services, Action<PermissionConfiguration> configure)
        {
            var transformer = new PermissionConfiguration();
            configure(transformer);
            services.AddSingleton(transformer);
            return services;
        }
    }

    /// <summary>
    /// Configures user roles into permissions by maintaining a mapping between roles and their associated permissions.
    /// </summary>
    public class PermissionConfiguration
    {
        readonly Dictionary<string, HashSet<RequirePermissionTo>> _permissionsByRole = new();

        public void CreateDefault(string redacteurRole, string beheerderRole)
        {
            var redacteurPermissions = new HashSet<RequirePermissionTo>([
                RequirePermissionTo.Beheer,
                RequirePermissionTo.NieuwsEnWerkInstucties,
                RequirePermissionTo.Links,
                RequirePermissionTo.Vacs]);
            _permissionsByRole[redacteurRole] = redacteurPermissions;

            var beheerderPermissions = new HashSet<RequirePermissionTo>([
                RequirePermissionTo.Beheer,
                RequirePermissionTo.Skills,
                RequirePermissionTo.Gespreksresultaten,
                RequirePermissionTo.Kanalen,
                RequirePermissionTo.ContactformulierenGroepen,
                RequirePermissionTo.ContactformulierenAfdelingen]);
            _permissionsByRole[beheerderRole] = beheerderPermissions;
        }

        /// <summary>
        /// Retrieves all permissions associated with specific roles.
        /// </summary>
        /// <param name="roles">The role names to get permissions for.</param>
        /// <returns>An set of permissions for the roles, or an empty set if the roles have no registered permissions.</returns>
        public IEnumerable<RequirePermissionTo> GetPermissionsForRoles(IEnumerable<string> roles)
        {
            HashSet<RequirePermissionTo> allPermissions = new();
            foreach (var roleName in roles)
            {
                var permissions = _permissionsByRole.TryGetValue(roleName, out var _permissions) ? _permissions : [];
                allPermissions.UnionWith(permissions);
            }
            return allPermissions;
        }
    }
}