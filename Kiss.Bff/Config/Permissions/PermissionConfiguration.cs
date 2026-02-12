using System.Security.Claims;

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

        public void CreateDefault(string redacteurRole, string beheerderRole, string kcmRole)
        {
            // Redacteur needs skillsread for NieuwsEnWerkBerichten
            var redacteurPermissions = new HashSet<RequirePermissionTo>([
                RequirePermissionTo.skillsread,
                RequirePermissionTo.berichtenread,
                RequirePermissionTo.berichtenbeheer,
                RequirePermissionTo.linksread,
                RequirePermissionTo.linksbeheer,
                RequirePermissionTo.vacsbeheer]);
            _permissionsByRole[redacteurRole] = redacteurPermissions;

            var beheerderPermissions = new HashSet<RequirePermissionTo>([
                RequirePermissionTo.afdelingen,
                RequirePermissionTo.groepen,
                RequirePermissionTo.skillsread,
                RequirePermissionTo.skillsbeheer,
                RequirePermissionTo.gespreksresultatenread,
                RequirePermissionTo.gespreksresultatenbeheer,
                RequirePermissionTo.kanalenread,
                RequirePermissionTo.kanalenbeheer,
                RequirePermissionTo.contactformulierenread,
                RequirePermissionTo.contactformulierengroepenbeheer,
                RequirePermissionTo.contactformulierenafdelingenbeheer]);
            _permissionsByRole[beheerderRole] = beheerderPermissions;

            var kcmPermissions = new HashSet<RequirePermissionTo>([
                RequirePermissionTo.afdelingen,
                RequirePermissionTo.groepen,
                RequirePermissionTo.skillsread,
                RequirePermissionTo.berichtenread,
                RequirePermissionTo.gespreksresultatenread,
                RequirePermissionTo.kanalenread,
                RequirePermissionTo.contactformulierenread,
                RequirePermissionTo.linksread
                ]);
            _permissionsByRole[kcmRole] = kcmPermissions;
        }

        /// <summary>
        /// Retrieves all permissions associated with a specific user.
        /// </summary>
        /// <param name="user">The representation of the user that requests the permissions.</param>
        /// <returns>A set of permissions for the user, or an empty set if the user has no registered permissions.</returns>
        public IEnumerable<RequirePermissionTo> GetPermissions(ClaimsPrincipal user)
        {
            HashSet<RequirePermissionTo> allPermissions = [];

            var roles = user.Identities.SelectMany(id => id.Claims.Where(claim => claim.Type == id.RoleClaimType))
                .Select(role => role.Value)
                .ToList();

            foreach (var roleName in roles)
            {
                var permissions = _permissionsByRole.TryGetValue(roleName, out var _permissions) ? _permissions : [];
                allPermissions.UnionWith(permissions);
            }
            return allPermissions;
        }
    }
}