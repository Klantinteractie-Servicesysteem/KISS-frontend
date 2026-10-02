using System.Security.Claims;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Kiss.Bff.Extern.Pabc
{
    /// <summary>
    /// Thin HTTP client for the PABC API. Handles authentication and request/response serialization.
    /// Domain-specific interpretation of the response belongs in the caller.
    /// </summary>
    public class PabcClient(HttpClient httpClient, ILogger<PabcClient> logger)
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<PabcClient> _logger = logger;

        /// <summary>
        /// Calls PABC to get application roles per entity type for the given user's functional roles.
        /// Returns null if the user has no roles or the PABC call fails.
        /// </summary>
        public async Task<GetApplicationRolesResponse?> GetApplicationRolesPerEntityTypeAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default)
        {
            var functionalRoles = GetFunctionalRoles(user);

            if (functionalRoles.Count == 0)
            {
                _logger.LogWarning("User has no functional roles. PABC will deny access.");
                return null;
            }

            var request = new GetApplicationRolesRequest
            {
                FunctionalRoleNames = functionalRoles
            };

            var response = await _httpClient.PostAsJsonAsync("api/v1/application-roles-per-entity-type", request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PABC returned {StatusCode} when fetching application roles per entity type", response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<GetApplicationRolesResponse>(cancellationToken);
        }

        /// <summary>
        /// Calls PABC to determine the KISS application roles (not scoped to any entity type)
        /// that the given user's functional roles map to. Returns an empty set if the user has
        /// no roles, PABC has no unscoped role mappings for KISS, or the PABC call fails.
        /// </summary>
        public async Task<IReadOnlySet<string>> GetKissApplicationRolesAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default)
        {
            var response = await GetApplicationRolesPerEntityTypeAsync(user, cancellationToken);

            var roleNames = response?.Results?
                .Where(result => result.EntityType == null)
                .SelectMany(result => result.ApplicationRoles)
                .Where(role => string.Equals(role.Application, PabcConfig.ApplicationName, StringComparison.OrdinalIgnoreCase))
                .Select(role => role.Name)
                ?? [];

            return new HashSet<string>(roleNames, StringComparer.OrdinalIgnoreCase);
        }

        private static IReadOnlyList<string> GetFunctionalRoles(ClaimsPrincipal user)
        {
            return user.Identities
                .SelectMany(id => id.Claims.Where(claim => claim.Type == id.RoleClaimType))
                .Select(claim => claim.Value)
                .Distinct()
                .ToList();
        }
    }

    #region PABC API Models

    public class GetApplicationRolesRequest
    {
        [JsonPropertyName("functionalRoleNames")]
        public required IReadOnlyList<string> FunctionalRoleNames { get; init; }
    }

    public class GetApplicationRolesResponse
    {
        [JsonPropertyName("results")]
        public List<ApplicationRolesPerEntityTypeResult>? Results { get; set; }
    }

    public class ApplicationRolesPerEntityTypeResult
    {
        [JsonPropertyName("entityType")]
        public EntityTypeModel? EntityType { get; set; }

        [JsonPropertyName("applicationRoles")]
        public List<ApplicationRoleModel> ApplicationRoles { get; set; } = new();
    }

    public class EntityTypeModel
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }

    public class ApplicationRoleModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("application")]
        public string Application { get; set; } = "";
    }

    #endregion
}
