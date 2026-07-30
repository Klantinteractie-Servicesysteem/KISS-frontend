using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem
{
    /// <summary>
    /// A proxy result that enriches zaken responses with zaaktype details.
    /// For each unique zaaktype URL, fetches the full zaaktype from the catalogi API
    /// and injects it as a "_zaaktype" property on each zaak.
    /// </summary>
    public class ZaaktypeEnrichedProxyResult(
        Func<HttpRequestMessage> requestFactory,
        ClaimsPrincipal user,
        string catalogiBaseUrl,
        ZaaksysteemRegistry config) : IActionResult
    {
        private readonly Func<HttpRequestMessage> _requestFactory = requestFactory;
        protected readonly ClaimsPrincipal _user = user;
        private readonly string _catalogiBaseUrl = catalogiBaseUrl.TrimEnd('/');
        private readonly ZaaksysteemRegistry _config = config;

        public async Task ExecuteResultAsync(ActionContext context)
        {
            var factory = context.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();
            var token = context.HttpContext.RequestAborted;

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

            // Resolve zaaktype details for all zaken
            var results = document["results"]?.AsArray();
            if (results != null)
            {
                var zaaktypeUrls = results
                    .Where(z => z != null)
                    .Select(z => z!["zaaktype"]?.GetValue<string>())
                    .Where(url => url != null)
                    .Cast<string>()
                    .Distinct()
                    .ToList();

                var zaaktypeByUrl = await ResolveZaaktypenAsync(client, zaaktypeUrls, token);

                // Enrich each zaak with its zaaktype details
                foreach (var zaak in results)
                {
                    if (zaak == null) continue;
                    var zaaktypeUrl = zaak["zaaktype"]?.GetValue<string>();
                    if (zaaktypeUrl != null && zaaktypeByUrl.TryGetValue(zaaktypeUrl, out var zaaktype))
                    {
                        zaak["_zaaktype"] = zaaktype.DeepClone();
                    }
                }

                // Apply optional filtering (overridden by PabcFilteredProxyResult)
                await FilterResultsAsync(document, results, zaaktypeByUrl, context, token);
            }
            else
            {
                // Single zaak detail — enrich with zaaktype
                var zaaktypeUrl = document["zaaktype"]?.GetValue<string>();
                if (zaaktypeUrl != null)
                {
                    var zaaktypeByUrl = await ResolveZaaktypenAsync(client, [zaaktypeUrl], token);
                    if (zaaktypeByUrl.TryGetValue(zaaktypeUrl, out var zaaktype))
                    {
                        document["_zaaktype"] = zaaktype.DeepClone();
                    }

                    if (!await CheckSingleZaakAccessAsync(document, zaaktypeByUrl, context, token))
                    {
                        return;
                    }
                }
            }

            context.HttpContext.Response.StatusCode = 200;
            context.HttpContext.Response.ContentType = "application/json";
            await context.HttpContext.Response.WriteAsync(document.ToJsonString(), token);
        }

        /// <summary>
        /// Override to apply filtering on the results. Base implementation does nothing.
        /// </summary>
        protected virtual Task FilterResultsAsync(
            JsonNode document, JsonArray results,
            Dictionary<string, JsonNode> zaaktypeByUrl,
            ActionContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Override to check access for a single zaak detail. Return false to block the response.
        /// Base implementation always allows access.
        /// </summary>
        protected virtual Task<bool> CheckSingleZaakAccessAsync(
            JsonNode document, Dictionary<string, JsonNode> zaaktypeByUrl,
            ActionContext context, CancellationToken token)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Fetches zaaktype details from the catalogi API for each unique URL.
        /// Returns the full zaaktype JsonNode keyed by URL.
        /// </summary>
        protected async Task<Dictionary<string, JsonNode>> ResolveZaaktypenAsync(
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
    }
}
