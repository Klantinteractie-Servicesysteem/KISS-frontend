using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using Kiss.Bff.Extern.Pabc;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem
{
    /// <summary>
    /// A proxy result that filters zaken based on PABC allowed zaaktypes.
    /// Intercepts the paginated zaken response, removes zaken whose zaaktype
    /// is not in the allowed set, and returns the filtered JSON.
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
        private readonly string _catalogiBaseUrl = catalogiBaseUrl;
        private readonly ZaaksysteemRegistry _config = config;

        public async Task ExecuteResultAsync(ActionContext context)
        {
            var factory = context.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();
            var token = context.HttpContext.RequestAborted;

            // Get allowed zaaktypes from PABC
            var allowedZaaktypen = await _pabcService.GetAllowedZaaktypenAsync(_user, token);

            // Fetch zaken from zaaksysteem
            using var client = factory.CreateClient("default");
            using var request = _requestFactory();
            using var responseMessage = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);

            if (!responseMessage.IsSuccessStatusCode)
            {
                // Pass through error responses as-is
                context.HttpContext.Response.StatusCode = (int)responseMessage.StatusCode;
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
                var filtered = new JsonArray();
                foreach (var zaak in results)
                {
                    if (zaak == null) continue;

                    var zaaktypeUrl = zaak["zaaktype"]?.GetValue<string>();
                    if (zaaktypeUrl == null) continue;

                    // Extract zaaktype identifier from the URL
                    // zaaktype URLs look like: https://host/catalogi/api/v1/zaaktypen/{uuid}
                    var zaaktypeId = zaaktypeUrl.Split('/').LastOrDefault();

                    if (zaaktypeId != null && allowedZaaktypen.Contains(zaaktypeId))
                    {
                        filtered.Add(zaak.DeepClone());
                    }
                }

                document["results"] = filtered;
                document["count"] = filtered.Count;
            }
            else
            {
                // Single zaak detail response — check if this zaak's type is allowed
                var zaaktypeUrl = document["zaaktype"]?.GetValue<string>();
                if (zaaktypeUrl != null)
                {
                    var zaaktypeId = zaaktypeUrl.Split('/').LastOrDefault();
                    if (zaaktypeId == null || !allowedZaaktypen.Contains(zaaktypeId))
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
    }
}
