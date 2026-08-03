using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using Kiss.Bff.Extern.Pabc;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem
{
    /// <summary>
    /// A proxy result that enriches zaken responses with zaaktype details from the catalogi API.
    /// When a PabcClient is provided, also filters zaken based on allowed zaaktypes.
    /// </summary>
    public sealed class ZaaktypeEnrichedProxyResult(
        Func<HttpRequestMessage> requestFactory,
        ClaimsPrincipal user,
        string catalogiBaseUrl,
        ZaaksysteemRegistry config,
        PabcClient? pabcClient = null) : IActionResult
    {
        private readonly Func<HttpRequestMessage> _requestFactory = requestFactory;
        private readonly ClaimsPrincipal _user = user;
        private readonly string _catalogiBaseUrl = catalogiBaseUrl.TrimEnd('/');
        private readonly ZaaksysteemRegistry _config = config;
        private readonly PabcClient? _pabcClient = pabcClient;

        public async Task ExecuteResultAsync(ActionContext context)
        {
            var factory = context.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();
            var token = context.HttpContext.RequestAborted;

            // Optionally fetch PABC allowed zaaktypen
            IReadOnlySet<string>? allowedZaaktypen = null;
            if (_pabcClient != null)
            {
                allowedZaaktypen = await GetAllowedZaaktypenAsync(token);
                if (allowedZaaktypen == null)
                {
                    // No access at all — return empty results
                    context.HttpContext.Response.StatusCode = 200;
                    context.HttpContext.Response.ContentType = "application/json";
                    await context.HttpContext.Response.WriteAsync(
                        JsonSerializer.Serialize(new { count = 0, next = (string?)null, previous = (string?)null, results = Array.Empty<object>() }),
                        token);
                    return;
                }
            }

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

            var json = await responseMessage.Content.ReadAsStringAsync(token);
            var document = JsonNode.Parse(json);

            if (document == null)
            {
                context.HttpContext.Response.StatusCode = 200;
                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsync(json, token);
                return;
            }

            var results = document["results"]?.AsArray();
            if (results != null)
            {
                // Resolve unique zaaktype details
                var zaaktypeUrls = results
                    .Where(z => z != null)
                    .Select(z => z!["zaaktype"]?.GetValue<string>())
                    .Where(url => url != null)
                    .Cast<string>()
                    .Distinct()
                    .ToList();

                var zaaktypeByUrl = await ResolveZaaktypenAsync(client, zaaktypeUrls, token);

                // All zaaktype details must be resolvable — otherwise data would be silently lost
                if (zaaktypeByUrl.Count < zaaktypeUrls.Count)
                {
                    context.HttpContext.Response.StatusCode = 502;
                    context.HttpContext.Response.ContentType = "application/json";
                    await context.HttpContext.Response.WriteAsync(
                        JsonSerializer.Serialize(new { detail = "Niet alle zaaktype details konden worden opgehaald uit de catalogi." }),
                        token);
                    return;
                }

                // Enrich and optionally filter
                var filtered = new JsonArray();
                foreach (var zaak in results)
                {
                    if (zaak == null) continue;
                    var zaaktypeUrl = zaak["zaaktype"]?.GetValue<string>();
                    if (zaaktypeUrl == null) continue;

                    var zaaktype = zaaktypeByUrl[zaaktypeUrl];

                    // PABC filtering
                    if (allowedZaaktypen != null)
                    {
                        var omschrijving = zaaktype["omschrijving"]?.GetValue<string>();
                        if (omschrijving == null || !allowedZaaktypen.Contains(omschrijving))
                            continue;
                    }

                    var enrichedZaak = zaak.DeepClone();
                    enrichedZaak["_zaaktype"] = zaaktype.DeepClone();
                    filtered.Add(enrichedZaak);
                }

                document["results"] = filtered;
                // Note: the original count/next/previous pagination fields are preserved as-is.
                // The frontend currently only fetches the first page and discards pagination metadata.
            }
            else
            {
                // Single zaak detail
                var zaaktypeUrl = document["zaaktype"]?.GetValue<string>();
                if (zaaktypeUrl != null)
                {
                    var zaaktypeByUrl = await ResolveZaaktypenAsync(client, [zaaktypeUrl], token);

                    if (zaaktypeByUrl.TryGetValue(zaaktypeUrl, out var zaaktype))
                    {
                        // PABC access check
                        if (allowedZaaktypen != null)
                        {
                            var omschrijving = zaaktype["omschrijving"]?.GetValue<string>();
                            if (omschrijving == null || !allowedZaaktypen.Contains(omschrijving))
                            {
                                context.HttpContext.Response.StatusCode = 403;
                                context.HttpContext.Response.ContentType = "application/json";
                                await context.HttpContext.Response.WriteAsync(
                                    JsonSerializer.Serialize(new { detail = "U heeft geen toegang tot dit zaaktype." }),
                                    token);
                                return;
                            }
                        }

                        document["_zaaktype"] = zaaktype.DeepClone();
                    }
                    else
                    {
                        // Zaaktype could not be resolved from catalogi — frontend requires _zaaktype
                        context.HttpContext.Response.StatusCode = 502;
                        context.HttpContext.Response.ContentType = "application/json";
                        await context.HttpContext.Response.WriteAsync(
                            JsonSerializer.Serialize(new { detail = "Zaaktype kon niet worden opgehaald uit de catalogi." }),
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
        /// Fetches zaaktype details from the catalogi API for each unique URL concurrently.
        /// </summary>
        private async Task<Dictionary<string, JsonNode>> ResolveZaaktypenAsync(
            HttpClient client, IList<string> zaaktypeUrls, CancellationToken cancellationToken)
        {
            var result = new Dictionary<string, JsonNode>();
            if (zaaktypeUrls.Count == 0) return result;

            var tasks = zaaktypeUrls.Select(async url =>
            {
                var zaaktype = await FetchZaaktypeAsync(client, url, cancellationToken);
                return (url, zaaktype);
            });

            var resolved = await Task.WhenAll(tasks);

            foreach (var (url, zaaktype) in resolved)
            {
                if (zaaktype != null)
                {
                    result[url] = zaaktype;
                }
            }

            return result;
        }

        private async Task<JsonNode?> FetchZaaktypeAsync(
            HttpClient client, string zaaktypeUrl, CancellationToken cancellationToken)
        {
            try
            {
                var uuid = zaaktypeUrl.TrimEnd('/').Split('/').LastOrDefault();
                if (string.IsNullOrEmpty(uuid)) return null;

                var catalogiUrl = $"{_catalogiBaseUrl}/zaaktypen/{uuid}";
                var request = new HttpRequestMessage(HttpMethod.Get, catalogiUrl);
                _config.ApplyHeaders(request.Headers, _user);

                using var response = await client.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode) return null;

                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonNode.Parse(body);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Interprets the PABC response to extract allowed zaaktype identifiers.
        /// Matching against catalogi omschrijving is intentionally case-sensitive:
        /// the entityType.id in PABC must exactly match the zaaktype omschrijving from catalogi.
        /// </summary>
        private async Task<IReadOnlySet<string>?> GetAllowedZaaktypenAsync(CancellationToken cancellationToken)
        {
            var response = await _pabcClient!.GetApplicationRolesPerEntityTypeAsync(_user, cancellationToken);

            if (response?.Results == null)
            {
                return new HashSet<string>();
            }

            var allowedZaaktypen = new HashSet<string>();

            foreach (var result in response.Results)
            {
                if (result.EntityType?.Type?.Equals("zaaktype", StringComparison.OrdinalIgnoreCase) != true)
                    continue;

                var hasMatchingRole = result.ApplicationRoles.Any(role =>
                    role.Name.Equals(PabcConfig.ApplicationRole, StringComparison.OrdinalIgnoreCase) &&
                    role.Application.Equals(PabcConfig.ApplicationName, StringComparison.OrdinalIgnoreCase));

                if (hasMatchingRole && result.EntityType.Id is not null)
                {
                    allowedZaaktypen.Add(result.EntityType.Id);
                }
            }

            return allowedZaaktypen;
        }
    }
}
