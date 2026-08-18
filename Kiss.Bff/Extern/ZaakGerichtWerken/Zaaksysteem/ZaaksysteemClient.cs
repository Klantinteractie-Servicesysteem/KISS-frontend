using System.Net;
using System.Security.Claims;

namespace Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem
{
    /// <summary>
    /// Exception bij een niet-success response van het zaaksysteem.
    /// Bevat de originele response body en content type voor transparante doorgifte.
    /// </summary>
    public class ZaaksysteemException(string message, HttpStatusCode statusCode, string responseBody, string contentType)
        : HttpRequestException(message, null, statusCode)
    {
        public string ResponseBody { get; } = responseBody;
        public string ContentType { get; } = contentType;
    }

    /// <summary>
    /// Thin HTTP client voor communicatie met het zaaksysteem (OpenZaak).
    /// Doet alleen HTTP calls + error handling; geen business logic.
    /// </summary>
    public class ZaaksysteemClient(IHttpClientFactory httpClientFactory, ILogger<ZaaksysteemClient> logger)
    {
        private HttpClient HttpClient => httpClientFactory.CreateClient("default");

        /// <summary>
        /// Haalt een resource op van het zaaksysteem.
        /// Gooit ZaaksysteemException met originele response bij niet-success responses.
        /// </summary>
        public async Task<T> GetAsync<T>(string url, ClaimsPrincipal user, ZaaksysteemRegistry config, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            config.ApplyHeaders(request.Headers, user);

            using var response = await HttpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";
                logger.LogError("Zaaksysteem returned {StatusCode} voor {Url}: {ErrorBody}",
                    (int)response.StatusCode, url, errorBody);
                throw new ZaaksysteemException(
                    $"Zaaksysteem returned {(int)response.StatusCode}: {errorBody}",
                    response.StatusCode,
                    errorBody,
                    contentType);
            }

            return await response.Content.ReadFromJsonAsync<T>(cancellationToken)
                ?? throw new InvalidOperationException($"Lege response van zaaksysteem voor {url}");
        }

        /// <summary>
        /// Haalt een resource op, retourneert null bij fouten (voor niet-kritieke calls zoals zaaktype ophalen).
        /// </summary>
        public async Task<T?> GetOrDefaultAsync<T>(string url, ClaimsPrincipal user, ZaaksysteemRegistry config, CancellationToken cancellationToken) where T : class
        {
            try
            {
                return await GetAsync<T>(url, user, config, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ophalen mislukt voor {Url}", url);
                return null;
            }
        }
    }
}
