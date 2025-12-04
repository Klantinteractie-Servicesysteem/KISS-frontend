
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace Kiss.Bff.Extern
{
    public record RegistryConfig
    {
        public required IReadOnlyList<RegistrySystem> Systemen { get; init; }
        public RegistrySystem? GetRegistrySystem(string systemIdentifier) => Systemen.FirstOrDefault(x => x.Identifier == systemIdentifier);
    }

    public record RegistrySystem
    {
        public bool IsDefault { get; init; }
        public RegistryVersion RegistryVersion { get; init; }
        public KlantinteractieRegistry? KlantinteractieRegistry { get; init; }
        public InternetaakRegistry? InterneTaakRegistry { get; init; }
        public ContactmomentRegistry? ContactmomentRegistry { get; init; }
        public KlantRegistry? KlantRegistry { get; init; }
        public ZaaksysteemRegistry? ZaaksysteemRegistry { get; init; }
        public required string Identifier { get; init; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter<RegistryVersion>))]
    public enum RegistryVersion
    {
        OpenKlant1,
        OpenKlant2,
    }

    public abstract record RegistryBase
    {
        public required string BaseUrl { get; init; }
        public string? ClientId { get; init; }
        public string? ClientSecret { get; init; }
        public string? Token { get; init; }

        public virtual void ApplyHeaders(HttpRequestHeaders headers, System.Security.Claims.ClaimsPrincipal user)
        {
        }
    }

    public record KlantinteractieRegistry : RegistryBase
    {
        public override void ApplyHeaders(HttpRequestHeaders headers, System.Security.Claims.ClaimsPrincipal user)
        {
            var authHeaderProvider = new AuthenticationHeaderProvider(Token, ClientId, ClientSecret);
            authHeaderProvider.ApplyAuthorizationHeader(headers, user);
        }
    }

    public record KlantRegistry : RegistryBase
    {
        public override void ApplyHeaders(HttpRequestHeaders headers, System.Security.Claims.ClaimsPrincipal user)
        {
            var authHeaderProvider = new AuthenticationHeaderProvider(Token, ClientId, ClientSecret);
            authHeaderProvider.ApplyAuthorizationHeader(headers, user);
        }
    }

    public record ContactmomentRegistry : RegistryBase
    {
        public override void ApplyHeaders(HttpRequestHeaders headers, System.Security.Claims.ClaimsPrincipal user)
        {
            var authHeaderProvider = new AuthenticationHeaderProvider(Token, ClientId, ClientSecret);
            authHeaderProvider.ApplyAuthorizationHeader(headers, user);
        }
    }

    public record ZaaksysteemRegistry
    {
        public void ApplyHeaders(HttpRequestHeaders headers, System.Security.Claims.ClaimsPrincipal user)
        {
            var authHeaderProvider = new AuthenticationHeaderProvider(null, ClientId, ClientSecret);
            authHeaderProvider.ApplyAuthorizationHeader(headers, user);
            headers.Add(CrsHeaderConstants.AcceptCrs, CrsHeaderConstants.Value);
            headers.Add(CrsHeaderConstants.ContentCrs, CrsHeaderConstants.Value);
        }

        public required string ClientId { get; init; }
        public required string ClientSecret { get; init; }


        public string? DeeplinkUrl { get; init; }
        public string? DeeplinkProperty { get; init; }
        public bool UseExperimentalQueries { get; init; }


        public required string ZakenBaseUrl { get; init; }
        public required string CatalogiBaseUrl { get; init; }
        public required string DocumentenBaseUrl { get; init; }
    }

    public static class CrsHeaderConstants
    {
        public const string Value = "EPSG:4326";
        public const string ContentCrs = "Content-Crs";
        public const string AcceptCrs = "Accept-Crs";
    };

    public record InternetaakRegistry : RegistryBase
    {
        public required string ObjectTypeUrl { get; init; }
        public required string ObjectTypeVersion { get; init; }

        public override void ApplyHeaders(HttpRequestHeaders headers, System.Security.Claims.ClaimsPrincipal user)
        {
            var authHeaderProvider = new AuthenticationHeaderProvider(Token, ClientId, ClientSecret);
            authHeaderProvider.ApplyAuthorizationHeader(headers, user);
            headers.Add(CrsHeaderConstants.ContentCrs, CrsHeaderConstants.Value);
        }
    }
}
