using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using Kiss.Bff.Extern.Pabc;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem
{
    /// <summary>
    /// A proxy result that filters zaken based on PABC allowed zaaktypes.
    /// Intercepts the paginated zaken response, resolves each zaaktype URL to its
    /// omschrijving via the catalogi API, and removes zaken whose zaaktype omschrijving
    /// is not in the PABC allowed set.
    /// </summary>
    public sealed class PabcFilteredProxyResult(
        Func<HttpRequestMessage> requestFactory,
        PabcService pabcService,
        ClaimsPrincipal user,
        string catalogiBaseUrl,
        ZaaksysteemRegistry config) : IActionResult
    {
        private readonly Func<HttpRequestMessage> _requestFactory = requestFactory;
        private readonly PabcService _pabcService = pabcService;
        private readonly ClaimsPrincipal _user = user;
        private readonly string _catalogiBaseUrl = catalogiBaseUrl.TrimEnd('/');
        private readonly ZaaksysteemRegistry _config = config;

        public async Task ExecuteResultAsync(ActionContext context)
        {
            var factory = context.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();
            var token = context.HttpContext.RequestAborted;

            // Get allowed zaaktype omschrijvingen from PABC
            var allowedZaaktypen = await _pabcService.GetAllowedZaaktypenAsync(_user, token);

            // Fetch zaken from zaaksysteem
            using var client = factory.CreateClient("default");
            using var request = _requestFactory();
            using var responseMessage = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);

            if (!responseMessage.IsSuccessStatusCode)
            {
                context.HttpContext.Response.StatusCode = (int)responseMessage.StatusCode;
                context.HttpContext.Response.ContentType = responseMessage.Content.Headers.ContentType?.ToString() ?? "application/json";
                await using var errorStream = await responseMessage.Content.ReadAsStreamAsync(token);
                await errorStream.CopyToAsync(context.HttpContext.Response.Body, token);
                return;
            }

            // Parse response JSON
            var json = await responseMessage.Content.ReadAsStringAsync(token);
            var document = JsonNode.Parse(json);

            if (document == null || allowedZaaktypen == null)
            {
                // If we can't parse or have no allowed zaaktypen, return empty results
                context.HttpContext.Response.StatusCode = 200;
                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsync(
                    JsonSerializer.Serialize(new { count = 0, next = (string?)null, previous = (string?)null, results = Array.Empty<object>() }),
                    token);
                return;
            }

            // Filter results array (paginated list response)
            var results = document["results"]?.AsArray();
            if (results != null)
            {
                // Collect unique zaaktype URLs and resolve their omschrijvingen via catalogi
                var zaaktypeUrls = results
                    .Where(z => z != null)
                    .Select(z => z!["zaaktype"]?.GetValue<string>())
                    .Where(url => url != null)
                    .Cast<string>()
                    .Distinct()
                    .ToList();

                var omschrijvingByUrl = await ResolveZaaktypeOmschrijvingenAsync(client, zaaktypeUrls, token);

                var filtered = new JsonArray();
                foreach (var zaak in results)
                {
                    if (zaak == null) continue;

                    var zaaktypeUrl = zaak["zaaktype"]?.GetValue<string>();
                    if (zaaktypeUrl == null) continue;

                    if (omschrijvingByUrl.TryGetValue(zaaktypeUrl, out var omschrijving)
                        && allowedZaaktypen.Contains(omschrijving))
                    {
                        filtered.Add(zaak.DeepClone());
                    }
                }

                document["results"] = filtered;
                // Note: the upstream count/next/previous are left as-is; they no longer reflect the filtered results.
                // The frontend currently only fetches the first page and discards pagination metadata.
            }
            else
            {
                // Single zaak detail response — check if this zaak's type is allowed
                var zaaktypeUrl = document["zaaktype"]?.GetValue<string>();
                if (zaaktypeUrl != null)
                {
                    var omschrijvingByUrl = await ResolveZaaktypeOmschrijvingenAsync(client, [zaaktypeUrl], token);
                    var isAllowed = omschrijvingByUrl.TryGetValue(zaaktypeUrl, out var omschrijving)
                        && allowedZaaktypen.Contains(omschrijving);

                    if (!isAllowed)
                    {
                        context.HttpContext.Response.StatusCode = 403;
                        context.HttpContext.Response.ContentType = "application/json";
                        await context.HttpContext.Response.WriteAsync(
                            JsonSerializer.Serialize(new { detail = "U heeft geen toegang tot dit zaaktype." }),
                            token);
                        return;
                    }
                }
            }

            context.HttpContext.Response.StatusCode = 200;
            context.HttpContext.Response.ContentType = "application/json";
            await context.HttpContext.Response.WriteAsync(document.ToJsonString(), token);
        }

        /// <summary>
        /// Resolves zaaktype URLs to their omschrijving by fetching each unique zaaktype
        /// from the catalogi API. Multiple zaken often share the same zaaktype, so this
        /// deduplicates calls. Fetches are performed concurrently.
        /// </summary>
        private async Task<Dictionary<string, string>> ResolveZaaktypeOmschrijvingenAsync(
            HttpClient client, IList<string> zaaktypeUrls, CancellationToken cancellationToken)
        {
            var result = new Dictionary<string, string>();
            if (zaaktypeUrls.Count == 0) return result;

            var tasks = zaaktypeUrls.Select(async url =>
            {
                var omschrijving = await FetchZaaktypeOmschrijvingAsync(client, url, cancellationToken);
                return (url, omschrijving);
            });

            var resolved = await Task.WhenAll(tasks);

            foreach (var (url, omschrijving) in resolved)
            {
                if (omschrijving != null)
                {
                    result[url] = omschrijving;
                }
            }

            return result;
        }

        /// <summary>
        /// Fetches a single zaaktype from the catalogi API using the configured CatalogiBaseUrl
        /// and extracts its omschrijving. The UUID is extracted from the zaaktype URL.
        /// </summary>
        private async Task<string?> FetchZaaktypeOmschrijvingAsync(
            HttpClient client, string zaaktypeUrl, CancellationToken cancellationToken)
        {
            try
            {
                // Extract UUID from the zaaktype URL (e.g. https://host/catalogi/api/v1/zaaktypen/{uuid})
                var uuid = zaaktypeUrl.TrimEnd('/').Split('/').LastOrDefault();
                if (string.IsNullOrEmpty(uuid)) return null;

                // Route through configured CatalogiBaseUrl with proper auth
                var catalogiUrl = $"{_catalogiBaseUrl}/zaaktypen/{uuid}";
                var request = new HttpRequestMessage(HttpMethod.Get, catalogiUrl);
                _config.ApplyHeaders(request.Headers, _user);

                using var response = await client.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode) return null;

                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                var zaaktype = JsonNode.Parse(body);
                return zaaktype?["omschrijving"]?.GetValue<string>();
            }
            catch
            {
                return null;
            }
        }
    }
}
