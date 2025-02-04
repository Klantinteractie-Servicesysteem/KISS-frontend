﻿
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace Kiss.Bff.Extern
{
    public record RegistryConfig
    {
        public required IReadOnlyList<RegistrySystem> Systemen { get; init; }
        public RegistrySystem? GetRegistrySystem(string? systemIdentifier) => string.IsNullOrWhiteSpace(systemIdentifier)
            ? Systemen.FirstOrDefault(x => x.IsDefault)
            : Systemen.FirstOrDefault(x => x.Identifier == systemIdentifier);
    }

    public record RegistrySystem
    {
        public bool IsDefault { get; init; }
        public KlantinteractieVersion KlantinteractieVersion { get; init; }
        public required KlantinteractieRegistry KlantinteractieRegistry { get; init; }
        public InternetaakRegistry? InterneTaakRegistry { get; init; }
        public required string Identifier { get; init; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter<KlantinteractieVersion>))]
    public enum KlantinteractieVersion
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

    public record InternetaakRegistry : RegistryBase
    {
        private const string ContentCrsHeaderName = "Content-Crs";
        private const string DefaultContentCrsHeaderValue = "EPSG:4326";

        public required string ObjectTypeUrl { get; init; }
        public required string ObjectTypeVersion { get; init; }

        public override void ApplyHeaders(HttpRequestHeaders headers, System.Security.Claims.ClaimsPrincipal user)
        {
            var authHeaderProvider = new AuthenticationHeaderProvider(Token, ClientId, ClientSecret);
            authHeaderProvider.ApplyAuthorizationHeader(headers, user);
            headers.Add(ContentCrsHeaderName, DefaultContentCrsHeaderValue);
        }
    }
}
