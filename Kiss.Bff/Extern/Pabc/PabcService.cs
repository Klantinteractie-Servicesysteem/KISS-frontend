using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kiss.Bff.Extern.Pabc
{
    public class PabcService(HttpClient httpClient, PabcConfig config, ILogger<PabcService> logger)
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly PabcConfig _config = config;
        private readonly ILogger<PabcService> _logger = logger;

        /// <summary>
        /// Gets the allowed zaaktype IDs for the given user based on their functional roles.
        /// Returns an empty set when no zaaktypes are allowed (including when PABC returns no matching application role).
        /// Note: caching per user/roles would be a performance improvement for future consideration.
        /// </summary>
        public async Task<IReadOnlySet<string>?> GetAllowedZaaktypenAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default)
        {
            var functionalRoles = GetFunctionalRoles(user);

            if (functionalRoles.Count == 0)
            {
                _logger.LogWarning("User has no functional roles. PABC will deny access to all zaaktypes.");
                return new HashSet<string>();
            }

            return await FetchAllowedZaaktypenFromPabc(functionalRoles, cancellationToken);
        }

        private async Task<IReadOnlySet<string>> FetchAllowedZaaktypenFromPabc(IReadOnlyList<string> functionalRoles, CancellationToken cancellationToken)
        {
            var request = new GetApplicationRolesRequest
            {
                FunctionalRoleNames = functionalRoles
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "api/v1/application-roles-per-entity-type")
            {
                Content = content
            };
            requestMessage.Headers.Add("X-API-KEY", _config.ApiKey);

            var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PABC returned {StatusCode} when fetching application roles per entity type", response.StatusCode);
                // On error, deny all access (safe default)
                return new HashSet<string>();
            }

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            var pabcResponse = JsonSerializer.Deserialize<GetApplicationRolesResponse>(responseBody);

            if (pabcResponse?.Results == null)
            {
                return new HashSet<string>();
            }

            // Matching against catalogi omschrijving is intentionally case-sensitive:
            // the entityType.id in PABC must exactly match the zaaktype omschrijving from catalogi.
            var allowedZaaktypen = new HashSet<string>();

            foreach (var result in pabcResponse.Results)
            {
                if (result.EntityType?.Type?.Equals("zaaktype", StringComparison.OrdinalIgnoreCase) != true)
                    continue;

                var hasMatchingRole = result.ApplicationRoles.Any(role =>
                    role.Name.Equals(_config.ApplicationRole, StringComparison.OrdinalIgnoreCase) &&
                    role.Application.Equals(_config.ApplicationName, StringComparison.OrdinalIgnoreCase));

                if (hasMatchingRole && result.EntityType.Id is not null)
                {
                    allowedZaaktypen.Add(result.EntityType.Id);
                }
            }

            _logger.LogInformation("PABC returned {Count} allowed zaaktypen", allowedZaaktypen.Count);
            return allowedZaaktypen;
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

    internal class GetApplicationRolesRequest
    {
        [JsonPropertyName("functionalRoleNames")]
        public required IReadOnlyList<string> FunctionalRoleNames { get; init; }
    }

    internal class GetApplicationRolesResponse
    {
        [JsonPropertyName("results")]
        public List<GetApplicationRolesResponseModel>? Results { get; set; }
    }

    internal class GetApplicationRolesResponseModel
    {
        [JsonPropertyName("entityType")]
        public EntityTypeModel? EntityType { get; set; }

        [JsonPropertyName("applicationRoles")]
        public List<ApplicationRoleModel> ApplicationRoles { get; set; } = new();
    }

    internal class EntityTypeModel
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }

    internal class ApplicationRoleModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("application")]
        public string Application { get; set; } = "";
    }

    #endregion
}
