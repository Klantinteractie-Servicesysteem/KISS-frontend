using System.Security.Claims;
using System.Text.Json.Nodes;
using Kiss;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Extensions.DependencyInjection
{
    public class HaalCentraalConfig(IConfiguration config)
    {
        readonly string? _apiKey = config["HAAL_CENTRAAL_API_KEY"];
        readonly string? _userHeaderName = config["HAAL_CENTRAAL_USER_HEADER_NAME"];
        readonly Dictionary<string, string>? _generalHeaders = config.GetSection("HAAL_CENTRAAL_CUSTOM_HEADERS")?.Get<Dictionary<string, string>>();
        readonly Dictionary<string, Dictionary<string, string>>? _headersForSpecificTypeOfSearch = config.GetSection("HAAL_CENTRAAL_CUSTOM_HEADERS")?.Get<Dictionary<string, Dictionary<string, string>>>();

        public string? BaseUrl { get; } = config["HAAL_CENTRAAL_BASE_URL"];

        public Dictionary<string, string> GetHeaders(string type, ClaimsPrincipal user)
        {
            var result = _generalHeaders?.ToDictionary() ?? [];

            if (!string.IsNullOrWhiteSpace(_apiKey))
            {
                result["X-API-KEY"] = _apiKey;
            }

            if (!string.IsNullOrWhiteSpace(_userHeaderName))
            {
                result[_userHeaderName] = user.GetUserName() ?? "";
            }

            if (_headersForSpecificTypeOfSearch != null)
            {
                foreach (var (headerName, valuesPerTypeOfSearch) in _headersForSpecificTypeOfSearch)
                {
                    if (valuesPerTypeOfSearch.TryGetValue(type, out var headerValue) && !string.IsNullOrWhiteSpace(headerValue))
                    {
                        result[headerName] = headerValue;
                    }
                }
            }

            return result;
        }
    }

    public static class HaalCentraalExtensions
    {
        public static IServiceCollection AddHaalCentraal(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton(new HaalCentraalConfig(config));
            return services;
        }
    }

    [ApiController]
    public class HaalCentraalCustomProxy(HaalCentraalConfig config) : ControllerBase
    {
        [HttpPost("/api/haalcentraal/brp/personen")]
        public IActionResult Zoeken(JsonObject request) => new ProxyResult(() =>
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{config.BaseUrl.AsSpan().TrimEnd('/')}/brp/personen")
            {
                Content = JsonContent.Create(request)
            };

            httpRequestMessage.Content.Headers.ContentLength = Request.Headers.ContentLength;

            foreach (var (key, value) in config.GetHeaders(request["type"]?.GetValue<string>() ?? "", User))
            {
                httpRequestMessage.Headers.Add(key, value);
            }

            return httpRequestMessage;
        });
    }
}
