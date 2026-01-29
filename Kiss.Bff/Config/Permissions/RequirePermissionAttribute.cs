using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;

namespace Kiss.Bff.Config.Permissions
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RequirePermissionTo
    {
        skillsread,
        skillsbeheer,
        kanalenread,
        kanalenbeheer,
        gespreksresultatenread,
        gespreksresultatenbeheer,
        contactformulierenread,
        contactformulierenafdelingenbeheer,
        contactformulierengroepenbeheer,
        berichtenread,
        berichtenbeheer,
        linksbeheer,
        vacsbeheer,
    }

    /// <summary>
    /// Authorization attribute that restricts access based on one or more permissions.
    /// Can be applied to controllers or action methods to enforce permission-based authorization.
    /// </summary>
    public sealed class RequirePermissionAttribute(params RequirePermissionTo[] permissions) : AuthorizeAttribute, IAuthorizationRequirement, IAuthorizationRequirementData
    {
        public RequirePermissionTo[] Permissions => permissions;
        public IEnumerable<IAuthorizationRequirement> GetRequirements() => [this];
        public static string? GetPermissionsStringValue(RequirePermissionTo[]? permissions)
        {
            var prefix = "Permission:";
            return permissions?.Length > 0 ? prefix + string.Join(",", permissions) : null;
        }
    }
}
