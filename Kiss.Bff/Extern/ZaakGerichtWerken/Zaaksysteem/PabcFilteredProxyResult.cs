using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using Kiss.Bff.Extern.Pabc;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem
{
    /// <summary>
    /// Extends ZaaktypeEnrichedProxyResult with PABC-based zaaktype filtering.
    /// After enrichment, removes zaken whose zaaktype omschrijving is not in the
    /// PABC allowed set, and blocks single zaak detail access for unauthorized zaaktypes.
    /// </summary>
    public sealed class PabcFilteredProxyResult(
        Func<HttpRequestMessage> requestFactory,
        PabcService pabcService,
        ClaimsPrincipal user,
        string catalogiBaseUrl,
        ZaaksysteemRegistry config)
        : ZaaktypeEnrichedProxyResult(requestFactory, user, catalogiBaseUrl, config)
    {
        private readonly PabcService _pabcService = pabcService;

        private IReadOnlySet<string>? _allowedZaaktypen;

        public new async Task ExecuteResultAsync(ActionContext context)
        {
            var token = context.HttpContext.RequestAborted;

            // Pre-fetch allowed zaaktypen before the base class runs
            _allowedZaaktypen = await _pabcService.GetAllowedZaaktypenAsync(_user, token);

            if (_allowedZaaktypen == null)
            {
                // No access at all — return empty results
                context.HttpContext.Response.StatusCode = 200;
                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsync(
                    JsonSerializer.Serialize(new { count = 0, next = (string?)null, previous = (string?)null, results = Array.Empty<object>() }),
                    token);
                return;
            }

            await base.ExecuteResultAsync(context);
        }

        protected override Task FilterResultsAsync(
            JsonNode document, JsonArray results,
            Dictionary<string, JsonNode> zaaktypeByUrl,
            ActionContext context, CancellationToken token)
        {
            if (_allowedZaaktypen == null) return Task.CompletedTask;

            var filtered = new JsonArray();
            foreach (var zaak in results)
            {
                if (zaak == null) continue;
                var zaaktypeUrl = zaak["zaaktype"]?.GetValue<string>();
                if (zaaktypeUrl == null) continue;

                if (zaaktypeByUrl.TryGetValue(zaaktypeUrl, out var zaaktype))
                {
                    var omschrijving = zaaktype["omschrijving"]?.GetValue<string>();
                    if (omschrijving != null && _allowedZaaktypen.Contains(omschrijving))
                    {
                        filtered.Add(zaak.DeepClone());
                    }
                }
            }

            document["results"] = filtered;
            // Note: count reflects filtered items on this page only, not total across all pages.
            // The frontend currently only fetches the first page and discards pagination metadata.
            document["count"] = filtered.Count;

            return Task.CompletedTask;
        }

        protected override async Task<bool> CheckSingleZaakAccessAsync(
            JsonNode document, Dictionary<string, JsonNode> zaaktypeByUrl,
            ActionContext context, CancellationToken token)
        {
            if (_allowedZaaktypen == null) return false;

            var zaaktypeUrl = document["zaaktype"]?.GetValue<string>();
            if (zaaktypeUrl == null) return true;

            if (zaaktypeByUrl.TryGetValue(zaaktypeUrl, out var zaaktype))
            {
                var omschrijving = zaaktype["omschrijving"]?.GetValue<string>();
                if (omschrijving != null && _allowedZaaktypen.Contains(omschrijving))
                {
                    return true;
                }
            }

            context.HttpContext.Response.StatusCode = 403;
            context.HttpContext.Response.ContentType = "application/json";
            await context.HttpContext.Response.WriteAsync(
                JsonSerializer.Serialize(new { detail = "U heeft geen toegang tot dit zaaktype." }),
                token);
            return false;
        }
    }
}
