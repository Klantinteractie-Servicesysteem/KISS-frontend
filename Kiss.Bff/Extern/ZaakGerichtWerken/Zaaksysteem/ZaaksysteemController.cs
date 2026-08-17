using Kiss.Bff.Extern.Pabc;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem
{
    [ApiController]
    public class ZaaksysteemController(
        RegistryConfig registryConfig,
        ILogger<ZaaksysteemController> logger,
        IHttpClientFactory httpClientFactory,
        PabcClient? pabcClient = null) : ControllerBase
    {
        private HttpClient HttpClient => httpClientFactory.CreateClient("default");

        /// <summary>
        /// Haalt zakenlijst op, verrijkt met zaaktype details.
        /// Wanneer PABC geconfigureerd is, worden zaken gefilterd op toegestane zaaktypes.
        /// </summary>
        [HttpGet("api/zaken/zaken")]
        public async Task<IActionResult> GetZakenList([FromHeader(Name = "systemIdentifier")] string systemIdentifier, CancellationToken cancellationToken)
        {
            var config = registryConfig.GetRegistrySystem(systemIdentifier)?.ZaaksysteemRegistry;
            if (config == null) return LogAndReturnConfigError(systemIdentifier);

            try
            {
                var zakenResponse = await FetchZakenAsync(config, "zaken" + (Request.QueryString.Value ?? ""), cancellationToken);
                if (zakenResponse == null) return Ok(new ZakenPaginatedResponse());

                var zaaktypeByUrl = await FetchZaaktypenAsync(config, zakenResponse.Results, cancellationToken);

                var allowedZaaktypenIds = pabcClient != null
                    ? await GetAllowedZaaktypenIdsAsync(cancellationToken)
                    : null;

                var enrichedZaken = EnrichAndFilterZaken(zakenResponse.Results, zaaktypeByUrl, allowedZaaktypenIds);
                zakenResponse.Results = enrichedZaken;

                return Ok(zakenResponse);
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Fout bij ophalen van zaken");
                return Problem(title: "Fout bij ophalen van zaken", detail: ex.Message, statusCode: 502);
            }
        }

        /// <summary>
        /// Haalt een enkele zaak op, verrijkt met zaaktype en controleert PABC toegang.
        /// </summary>
        [HttpGet("api/zaken/zaken/{uuid}")]
        public async Task<IActionResult> GetSingleZaak(string uuid, [FromHeader(Name = "systemIdentifier")] string systemIdentifier, CancellationToken cancellationToken)
        {
            var config = registryConfig.GetRegistrySystem(systemIdentifier)?.ZaaksysteemRegistry;
            if (config == null) return LogAndReturnConfigError(systemIdentifier);

            try
            {
                var zaak = await FetchSingleZaakAsync(config, uuid, cancellationToken);
                if (zaak == null) return NotFound();

                var zaaktype = await FetchSingleZaaktypeByUrlAsync(config, zaak.Zaaktype, cancellationToken);
                if (zaaktype == null)
                {
                    return Problem(title: "Zaaktype niet gevonden", detail: "Zaaktype kon niet worden opgehaald uit de catalogi.", statusCode: 502);
                }

                if (pabcClient != null)
                {
                    var allowedZaaktypenIds = await GetAllowedZaaktypenIdsAsync(cancellationToken);
                    if (zaaktype.Omschrijving == null || !allowedZaaktypenIds.Contains(zaaktype.Omschrijving))
                    {
                        return Problem(title: "Geen toegang", detail: "U heeft geen toegang tot dit zaaktype.", statusCode: 403);
                    }
                }

                zaak.ZaaktypeDetails = zaaktype;
                return Ok(zaak);
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Fout bij ophalen van zaak");
                return Problem(title: "Fout bij ophalen van zaak", detail: ex.Message, statusCode: 502);
            }
        }

        /// <summary>
        /// Niet-zaak paden (rollen, statussen, etc.) — proxy zonder verrijking.
        /// </summary>
        [HttpGet("api/zaken/{**path}")]
        public IActionResult ProxyNonZaakPath(string path, [FromHeader(Name = "systemIdentifier")] string systemIdentifier)
        {
            return ProxyToEndpoint(path, systemIdentifier, "zaken");
        }

        /// <summary>
        /// Proxyt catalogi API calls naar het juiste zaaksysteem endpoint
        /// </summary>
        [HttpGet("api/catalogi/{**path}")]
        public IActionResult GetCatalogi(string path, [FromHeader(Name = "systemIdentifier")] string systemIdentifier)
            => ProxyToEndpoint(path, systemIdentifier, "catalogi");

        /// <summary>
        /// Proxyt documenten API calls naar het juiste zaaksysteem endpoint
        /// </summary>
        [HttpGet("api/documenten/{**path}")]
        public IActionResult GetDocumenten(string path, [FromHeader(Name = "systemIdentifier")] string systemIdentifier)
            => ProxyToEndpoint(path, systemIdentifier, "documenten");

        #region Zaken ophalen en verrijken

        private async Task<ZakenPaginatedResponse?> FetchZakenAsync(
            ZaaksysteemRegistry config, string path, CancellationToken cancellationToken)
        {
            var url = $"{config.ZakenBaseUrl.TrimEnd('/')}/{path}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            config.ApplyHeaders(request.Headers, User);

            using var response = await HttpClient.SendAsync(request, cancellationToken);
            return !response.IsSuccessStatusCode ? null : await response.Content.ReadFromJsonAsync<ZakenPaginatedResponse>(cancellationToken);
        }

        private async Task<ZaakResource?> FetchSingleZaakAsync(
            ZaaksysteemRegistry config, string uuid, CancellationToken cancellationToken)
        {
            var url = $"{config.ZakenBaseUrl.TrimEnd('/')}/zaken/{uuid}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            config.ApplyHeaders(request.Headers, User);

            using var response = await HttpClient.SendAsync(request, cancellationToken);
            return !response.IsSuccessStatusCode ? null : await response.Content.ReadFromJsonAsync<ZaakResource>(cancellationToken);
        }

        private async Task<Dictionary<string, ZaaktypeResource>> FetchZaaktypenAsync(
            ZaaksysteemRegistry config, List<ZaakResource> zaken, CancellationToken cancellationToken)
        {
            var zaaktypeUrls = zaken
                .Select(z => z.Zaaktype)
                .Distinct()
                .ToList();

            var result = new Dictionary<string, ZaaktypeResource>();
            if (zaaktypeUrls.Count == 0) return result;

            var tasks = zaaktypeUrls.Select(async url =>
            {
                var zaaktype = await FetchSingleZaaktypeByUrlAsync(config, url, cancellationToken);
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

        private async Task<ZaaktypeResource?> FetchSingleZaaktypeByUrlAsync(
            ZaaksysteemRegistry config, string zaaktypeUrl, CancellationToken cancellationToken)
        {
            try
            {
                var uuid = zaaktypeUrl.TrimEnd('/').Split('/').LastOrDefault();
                if (string.IsNullOrEmpty(uuid)) return null;

                var url = $"{config.CatalogiBaseUrl.TrimEnd('/')}/zaaktypen/{uuid}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                config.ApplyHeaders(request.Headers, User);

                using var response = await HttpClient.SendAsync(request, cancellationToken);
                return !response.IsSuccessStatusCode ? null : await response.Content.ReadFromJsonAsync<ZaaktypeResource>(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Zaaktype ophalen mislukt voor {ZaaktypeUrl}", zaaktypeUrl);
                return null;
            }
        }

        private static List<ZaakResource> EnrichAndFilterZaken(
            List<ZaakResource> zaken, Dictionary<string, ZaaktypeResource> zaaktypeByUrl,
            IReadOnlySet<string>? allowedZaaktypenIds)
        {
            var result = new List<ZaakResource>();

            foreach (var zaak in zaken)
            {
                if (!zaaktypeByUrl.TryGetValue(zaak.Zaaktype, out var zaaktype)) continue;

                // PABC toegangscontrole/filtering: skip zaken waarvan het zaaktype niet is toegestaan
                if (allowedZaaktypenIds != null)
                {
                    if (zaaktype.Omschrijving == null || !allowedZaaktypenIds.Contains(zaaktype.Omschrijving))
                        continue;
                }

                zaak.ZaaktypeDetails = zaaktype;
                result.Add(zaak);
            }

            return result;
        }

        #endregion

        #region PABC

        private async Task<IReadOnlySet<string>> GetAllowedZaaktypenIdsAsync(CancellationToken cancellationToken)
        {
            var response = await pabcClient!.GetApplicationRolesPerEntityTypeAsync(User, cancellationToken);

            if (response?.Results == null)
            {
                return new HashSet<string>();
            }

            // case-sensitive matching (default comparer)
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

        #endregion

        #region Generic proxy

        private IActionResult ProxyToEndpoint(string path, string systemIdentifier, string apiType)
        {
            var config = registryConfig.GetRegistrySystem(systemIdentifier)?.ZaaksysteemRegistry;

            if (config == null)
            {
                return LogAndReturnConfigError(systemIdentifier);
            }

            var baseUrl = GetBaseUrlForApiType(config, apiType);

            return new ProxyResult(() =>
            {
                var url = $"{baseUrl.AsSpan().TrimEnd('/')}/{path}{Request?.QueryString}";
                var message = new HttpRequestMessage(HttpMethod.Get, url);
                config.ApplyHeaders(message.Headers, User);
                return message;
            });
        }

        private IActionResult LogAndReturnConfigError(string systemIdentifier)
        {
            var sanitizedSystemIdentifier = systemIdentifier.Replace("\n", "").Replace("\r", "").Replace("\t", "");
            logger.LogError("Geen zaaksysteem gevonden voor ZaaksysteemId {ZaaksysteemId}",
                sanitizedSystemIdentifier[..(sanitizedSystemIdentifier.Length < 15 ? sanitizedSystemIdentifier.Length - 1 : 15)] + "...");
            return Problem(
                title: "Configuratieprobleem",
                detail: "Geen zaaksysteem gevonden voor ZaaksysteemId " + systemIdentifier,
                statusCode: 500
            );
        }

        private static string GetBaseUrlForApiType(ZaaksysteemRegistry config, string apiType)
        {
            return apiType switch
            {
                "zaken" => config.ZakenBaseUrl,
                "catalogi" => config.CatalogiBaseUrl,
                "documenten" => config.DocumentenBaseUrl,
                _ => throw new ArgumentException($"Unknown API type: {apiType}", nameof(apiType))
            };
        }

        #endregion
    }
}
