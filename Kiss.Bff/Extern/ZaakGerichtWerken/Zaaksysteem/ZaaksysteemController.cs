using Kiss.Bff.Extern.Pabc;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem
{
    [ApiController]
    public class ZaaksysteemController(
        RegistryConfig registryConfig,
        ILogger<ZaaksysteemController> logger,
        ZaaksysteemClient zaaksysteemClient,
        PabcClient? pabcClient = null) : ControllerBase
    {

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
                var zakenUrl = $"{config.ZakenBaseUrl.TrimEnd('/')}/zaken{Request.QueryString.Value ?? ""}";
                var zakenResponse = await zaaksysteemClient.GetAsync<ZakenPaginatedResponse>(zakenUrl, User, config, cancellationToken);

                var zaaktypeByUrl = await FetchZaaktypenAsync(config, zakenResponse.Results, cancellationToken);

                // PABC filtering alleen als PABC geconfigureerd is én dit zaaksysteem PABC gebruikt
                var allowedZaaktypenIds = pabcClient != null && config.UsePabc
                    ? await GetAllowedZaaktypenIdsAsync(cancellationToken)
                    : null;

                var enrichedZaken = EnrichAndFilterZaken(zakenResponse.Results, zaaktypeByUrl, allowedZaaktypenIds);
                zakenResponse.Results = enrichedZaken;

                return Ok(zakenResponse);
            }
            catch (ZaaksysteemException ex)
            {
                logger.LogError(ex, "Fout bij ophalen van zaken");
                return new ContentResult
                {
                    StatusCode = (int?)ex.StatusCode ?? 502,
                    Content = ex.ResponseBody,
                    ContentType = ex.ContentType
                };
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
                var zaakUrl = $"{config.ZakenBaseUrl.TrimEnd('/')}/zaken/{uuid}";
                var zaak = await zaaksysteemClient.GetAsync<ZaakResource>(zaakUrl, User, config, cancellationToken);

                var zaaktypeUrl = $"{config.CatalogiBaseUrl.TrimEnd('/')}/zaaktypen/{zaak.Zaaktype.TrimEnd('/').Split('/').Last()}";
                var zaaktype = await zaaksysteemClient.GetAsync<ZaaktypeResource>(zaaktypeUrl, User, config, cancellationToken);

                // PABC toegangscontrole alleen als PABC geconfigureerd is én dit zaaksysteem PABC gebruikt
                if (pabcClient != null && config.UsePabc)
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
            catch (ZaaksysteemException ex)
            {
                logger.LogError(ex, "Fout bij ophalen van zaak");
                return new ContentResult
                {
                    StatusCode = (int?)ex.StatusCode ?? 502,
                    Content = ex.ResponseBody,
                    ContentType = ex.ContentType
                };
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

        /// <summary>
        /// Haalt zaaktype details op uit catalogi voor alle unieke zaaktype URLs in de zakenlijst.
        /// Gebruikt GetOrDefaultAsync: als een zaaktype niet opgehaald kan worden, wordt het overgeslagen.
        /// </summary>
        private async Task<Dictionary<string, ZaaktypeResource>> FetchZaaktypenAsync(
            ZaaksysteemRegistry config, List<ZaakResource> zaken, CancellationToken cancellationToken)
        {
            var zaaktypeUrls = zaken
                .Select(z => z.Zaaktype)
                .Distinct()
                .ToList();

            var result = new Dictionary<string, ZaaktypeResource>();
            var catalogiBaseUrl = config.CatalogiBaseUrl.TrimEnd('/');

            var tasks = zaaktypeUrls.Select(async zaaktypeUrl =>
            {
                var uuid = zaaktypeUrl.TrimEnd('/').Split('/').Last();
                var url = $"{catalogiBaseUrl}/zaaktypen/{uuid}";
                var zaaktype = await zaaksysteemClient.GetOrDefaultAsync<ZaaktypeResource>(url, User, config, cancellationToken);
                return (zaaktypeUrl, zaaktype);
            });

            var resolved = await Task.WhenAll(tasks);

            foreach (var (zaaktypeUrl, zaaktype) in resolved)
            {
                if (zaaktype != null)
                {
                    result[zaaktypeUrl] = zaaktype;
                }
            }

            return result;
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
