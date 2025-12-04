using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Kiss.Elastic.Sync.SharePoint
{
    public sealed partial class SharePointClient : IDisposable
    {
        private readonly GraphServiceClient _graphClient;
        private readonly string _siteIdentifier;
        private readonly IBrowsingContext _context;

        // This is the same error mapping that is used by the GraphServiceClient internally.
        // Keys typically map HTTP ranges ("4XX", "5XX") to a factory that can deserialize error payloads.
        private static readonly Dictionary<string, ParsableFactory<IParsable>> s_errorMapping = new()
        {
            ["XXX"] = ODataError.CreateFromDiscriminatorValue,
        };

        public SharePointClient(string tenantId, string clientId, string clientSecret, string siteUrl)
        {
            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            _graphClient = new GraphServiceClient(credential);
            _siteIdentifier = ParseSiteIdentifierFromUrl(siteUrl);
            _context = BrowsingContext.New(Configuration.Default);
        }

        private static string ParseSiteIdentifierFromUrl(string siteUrl)
        {
            // Input:  https://{hostname}/{path}
            // Output: {hostname}:{path}
            var uri = new Uri(siteUrl);
            return $"{uri.Host}:{uri.AbsolutePath}";
        }

        public async IAsyncEnumerable<SharePointPage> GetAllPages([EnumeratorCancellation] CancellationToken token)
        {
            // Check site via Graph API lookup (Guid is necessary to retrieve pages and subsites)
            var startingSite = await _graphClient.Sites[_siteIdentifier].GetAsync(
                x => x.QueryParameters.Select = [
                    // we need the id to get all pages for the site
                    "id", 
                    // we need the sharePointIds to get related sites if the starting site is a hub
                    "sharepointIds", 
                    // we need the parentReference to see if the starting site is a sub site
                    "parentReference"
                ],
                token);

            if (startingSite == null)
            {
                yield break;
            }

            // Retrieve all pages that are directly in the starting site
            await foreach (var page in GetAllPages(startingSite, token))
            {
                yield return page;
            }

            Func<Site, CancellationToken, IAsyncEnumerable<Site>> getAllSites = IsSubsite(startingSite)
                // If the starting site is a **subsite**, it cannot be a hub site.
                // Therefore, the only valid related sites are its own nested subsites.
                // IMPORTANT: We **must not** use GetAllHubRelatedSitesAndSubSites here, because hub-site
                // discovery would incorrectly traverse *upwards* to parent sites
                ? GetAllSubSitesRecursive
                // If the starting site is a **root site**, it may be a hub site.
                // GetAllHubRelatedSitesAndSubSites returns:
                //   - all hub-associated sites (if the root is a hub), AND
                //   - all (nested) subsites of those sites.
                // IMPORTANT: This method must only be used when starting from a root site,
                // because calling it for subsites would also include their parent sites,
                // which is not desired.
                : GetAllHubRelatedSitesAndSubSites;

            // loop through all the related / subsites
            await foreach (var site in getAllSites(startingSite, token))
            {
                // Retrieve all pages for each related / subsite
                await foreach (var page in GetAllPages(site, token))
                {
                    yield return page;
                }
            }
        }

        

        private static bool IsSubsite(Site site) =>
            !string.IsNullOrWhiteSpace(site?.ParentReference?.SiteId);

        /// <summary>
        /// Recursively retrieves all descendant sub-sites of the specified root site as an asynchronous stream.
        /// </summary>
        /// <remarks>Enumeration is performed recursively and may result in a large number of results if
        /// the site hierarchy is deep. The operation is performed lazily; sub-sites are retrieved as the stream is
        /// enumerated. The caller is responsible for handling cancellation via the provided token.</remarks>
        /// <param name="root">The root <see cref="Site"/> from which to begin retrieving sub-sites. Cannot be null.</param>
        /// <param name="token">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>An asynchronous stream of <see cref="Site"/> objects representing all sub-sites under the specified root,
        /// including nested descendants. The stream is empty if the root has no sub-sites.</returns>
        private async IAsyncEnumerable<Site> GetAllSubSitesRecursive(Site root, [EnumeratorCancellation] CancellationToken token)
        {
            await foreach (var site in GetAllSubSites(root, token))
            {
                yield return site;
                await foreach (var subSite in GetAllSubSitesRecursive(site, token))
                {
                    yield return subSite;
                }
            }
        }

        /// <summary>
        /// Asynchronously retrieves all sites and subsites related to the specified SharePoint hub site, excluding the
        /// root site itself.
        /// </summary>
        /// <remarks>This method queries for sites that share the same DepartmentId (SiteId) as the root
        /// hub site. The root site itself is not included in the results. The enumeration is performed asynchronously
        /// and supports cancellation.</remarks>
        /// <param name="rootSite">The root SharePoint site representing the hub. Must have a valid SharePoint SiteId in its SharepointIds
        /// property.</param>
        /// <param name="token">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>An asynchronous stream of Site objects representing all sites and subsites associated with the specified hub
        /// site, excluding the root site. The stream is empty if the root site does not have a valid SiteId.</returns>
        private async IAsyncEnumerable<Site> GetAllHubRelatedSitesAndSubSites(Site rootSite, [EnumeratorCancellation] CancellationToken token)
        {
            // Hub-related sites are discovered by searching for the hub's DepartmentId (which is its SiteId)
            if (!Guid.TryParse(rootSite.SharepointIds?.SiteId, out var siteId))
            {
                yield break;
            }

            var requestInfo = _graphClient.Sites.ToGetRequestInformation(x =>
            {
                x.QueryParameters.Select = ["id", "sharepointIds"];
            });

            // prepend the url template with the `search` query parameter.
            // NB: this is NOT the `$search` odata parameter
            // we can't use the `$search` parameter directly because it doesn't allow filtering by DepartmentId
            requestInfo.UrlTemplate = requestInfo.UrlTemplate?.Replace("{?", "{?search,");
            requestInfo.QueryParameters.Add("search", $"DepartmentId:{{{siteId}}}");

            var firstPage = _graphClient.RequestAdapter.SendAsync(
                requestInfo,
                SiteCollectionResponse.CreateFromDiscriminatorValue,
                s_errorMapping,
                token);

            await foreach (var site in IterateAllEntities(firstPage, x => x.Value, token))
            {
                if (
                    // this query also returns the root site, we don't want that
                    site.Id != rootSite.Id &&
                    // to be completely safe in case the DepartmentId query stops working as expected at some point in the futre,
                    // we double-check that the site indeed has the correct SiteId
                    site.SharepointIds?.SiteId == rootSite.SharepointIds.SiteId
                    )
                {
                    yield return site;
                }
            }
        }

        private IAsyncEnumerable<Site> GetAllSubSites(Site root, CancellationToken token)
        {
            var firstPage = _graphClient.Sites[root.Id]
                .Sites
                .GetAsync(cancellationToken: token);

            return IterateAllEntities(firstPage, x => x.Value, token);
        }

        private async IAsyncEnumerable<SharePointPage> GetAllPages(Site site, [EnumeratorCancellation] CancellationToken token)
        {
            await foreach (var modernPage in GetModernPages(site, token))
            {
                yield return modernPage;
            }

            await foreach (var classicPage in GetClassicPages(site, token))
            {
                yield return classicPage;
            }
        }

        private async IAsyncEnumerable<SharePointPage> GetModernPages(Site site, [EnumeratorCancellation] CancellationToken token)
        {
            var firstPage = _graphClient
                .Sites[site.Id]
                .Pages
                .GraphSitePage
                .GetAsync(x => { }, token);

            await foreach (var page in IterateAllEntities(firstPage, x => x.Value, token))
            {
                var htmlStrings = page.WebParts?.SelectMany(GetHtml) ?? [];
                var (content, headings) = await ExtractTextFromPage(htmlStrings, token);

                yield return new SharePointPage
                {
                    Id = page.Id!,
                    Title = string.IsNullOrWhiteSpace(page.Title) ? "Geen titel" : page.Title,
                    LastModified = page.LastModifiedDateTime,
                    CreatedBy = page.CreatedBy?.User?.DisplayName,
                    LastModifiedBy = page.LastModifiedBy?.User?.DisplayName,
                    Url = page.WebUrl,
                    Content = content,
                    Headings = headings
                };
            }
        }

        private async IAsyncEnumerable<SharePointPage> GetClassicPages(Site site, [EnumeratorCancellation] CancellationToken token)
        {
            var firstPage = _graphClient
                .Sites[site.Id]
                .Lists
                .GetAsync(x =>
                {
                    x.QueryParameters.Expand = ["items($expand=fields)"];
                }, cancellationToken: token);

            await foreach (var list in IterateAllEntities(firstPage, x => x.Value, token))
            {
                foreach (var item in list.Items ?? [])
                {
                    if (
                        item is not { WebUrl: { } url, Fields.AdditionalData: { } data } ||
                        !url.EndsWith(".aspx") ||
                        !data.TryGetValue("WikiField", out var wikiField) || 
                        wikiField is not string html
                        )
                    {
                        // this is not a page or we don't support it
                        Console.WriteLine($"skipped item because not a page or unsupported: {item.WebUrl ?? item.Id}");
                        continue;
                    }

                    // unfortunately, to get a safe id, we need to do another call
                    var itemWithOnlyIds = await _graphClient
                        .Sites[site.Id]
                        .Lists[list.Id]
                        .Items[item.Id]
                        .GetAsync(x => x.QueryParameters.Select = ["sharepointIds", "id"], token);

                    if (itemWithOnlyIds is not { SharepointIds.TenantId: { } tenantId, SharepointIds.SiteId: { } siteId, SharepointIds.ListItemUniqueId: { } itemId })
                    {
                        Console.WriteLine($"skipped item because we couldn't find a unique id: {item.WebUrl ?? item.Id}");
                        continue;
                    }

                    var (content, headings) = await ExtractTextFromPage([html], token);

                    var title = item.Name;
                    if (string.IsNullOrWhiteSpace(title))
                    {
                        title = ExtractTitleFromUrl(item.WebUrl);
                    }
                    if (string.IsNullOrWhiteSpace(title))
                    {
                        title = "Geen titel";
                    }

                    yield return new SharePointPage
                    {
                        Id = string.Join("/", tenantId, siteId, itemId),
                        Title = title,
                        Url = item.WebUrl,
                        LastModified = item.LastModifiedDateTime,
                        LastModifiedBy = item.LastModifiedBy?.User?.DisplayName,
                        CreatedBy = item.CreatedBy?.User?.DisplayName,
                        Content = content,
                        Headings = headings
                    };
                }
            }
        }

        /// <summary>
        /// Asynchronously iterates over all entities in a paginated collection, retrieving items from each page until
        /// no further pages are available.
        /// </summary>
        /// <remarks>This method transparently handles pagination by following the OData next link in each
        /// collection response. Entities are yielded as they are retrieved from each page. The iteration stops if the
        /// cancellation token is triggered or if no further pages are available.</remarks>
        /// <typeparam name="TCollection">The type of the paginated collection response, which must inherit from BaseCollectionPaginationCountResponse
        /// and have a parameterless constructor.</typeparam>
        /// <typeparam name="TEntity">The type of the entity contained within each collection page.</typeparam>
        /// <param name="firstPageTask">A task that, when completed, provides the first page of the collection to begin iteration.</param>
        /// <param name="getEntities">A function that extracts the list of entities from a given collection page. Returns null or an empty list if
        /// no entities are present.</param>
        /// <param name="token">A cancellation token that can be used to cancel the asynchronous iteration.</param>
        /// <returns>An asynchronous sequence of entities of type TEntity from all pages in the collection. The sequence ends
        /// when there are no more pages to retrieve.</returns>
        private async IAsyncEnumerable<TEntity> IterateAllEntities<TCollection, TEntity>(
            Task<TCollection?> firstPageTask,
            Func<TCollection, List<TEntity>?> getEntities,
            [EnumeratorCancellation] CancellationToken token)
            where TCollection : BaseCollectionPaginationCountResponse, new()
        {
            // Retrieve the first page
            var response = await firstPageTask;

            // Iterate through all pages
            while (response != null && getEntities(response) is { } entities)
            {
                // Yield each entity from the current page
                foreach (var item in entities)
                {
                    yield return item;
                }

                if (string.IsNullOrEmpty(response.OdataNextLink))
                {
                    // No more pages to retrieve
                    yield break;
                }

                // Prepare request to retrieve the next page
                var requestInfo = new RequestInformation
                {
                    HttpMethod = Method.GET,
                    // OdataNextLink already contains a fully-qualified URL, so we reuse it as UrlTemplate
                    UrlTemplate = response.OdataNextLink,
                };

                // Retrieve the next page
                response = await _graphClient.RequestAdapter.SendAsync<TCollection>(
                    requestInfo,
                    _ => new(),
                    s_errorMapping,
                    token);
            }
        }

        private static IEnumerable<string> GetHtml(WebPart webPart)
        {
            if (webPart is TextWebPart textWebPart
                && !string.IsNullOrWhiteSpace(textWebPart.InnerHtml))
            {
                yield return textWebPart.InnerHtml;
            }

            var additionalData = webPart.AdditionalData;
            if (additionalData != null
                && additionalData.TryGetValue("innerHtml", out var innerHtmlObj)
                && innerHtmlObj?.ToString() is string innerHtml
                && !string.IsNullOrWhiteSpace(innerHtml))
            {
                yield return innerHtml;
            }
        }

        private async Task<(string Content, IReadOnlyCollection<string> Headings)> ParseHtml(string html, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(html))
                return ("", []);

            using var doc = await _context.OpenAsync(req => req.Content(html), cancel: token);

            var headings = doc.QuerySelectorAll("h1,h2,h3,h4,h5,h6")
                .Select(x => x.TextContent)
                .ToHashSet();

            var content = doc.ToHtml(TextWithWhitespaceFormatter.Instance).Trim();

            return (content, headings);
        }

        private static string? ExtractTitleFromUrl(string? url) => string.IsNullOrWhiteSpace(url) ? url : Uri.UnescapeDataString(UrlRegex().Replace(url, "$2"));
        [GeneratedRegex("(.*/)+(.*)\\..*")]
        private static partial Regex UrlRegex();

        public async Task<(IReadOnlyCollection<string> Content, IReadOnlyCollection<string> Headings)> ExtractTextFromPage(IEnumerable<string> allHtml, CancellationToken token)
        {
            var allContent = new HashSet<string>();
            var allHeadings = new HashSet<string>();

            foreach (var html in allHtml)
            {
                var (content, headings) = await ParseHtml(html, token);
                allContent.Add(content);
                foreach (var heading in headings)
                {
                    allHeadings.Add(heading);
                }
            }

            return (allContent, allHeadings);
        }

        public void Dispose()
        {
            _graphClient.Dispose();
            _context.Dispose();
        }

        /// <summary>
        /// Custom formatter that turns HTML into plain-ish text while keeping some structural whitespace.
        /// </summary>
        private class TextWithWhitespaceFormatter : IMarkupFormatter
        {
            public static readonly IMarkupFormatter Instance = new TextWithWhitespaceFormatter();

            private TextWithWhitespaceFormatter()
            {

            }

            public string CloseTag(IElement element, bool selfClosing) => "";

            public string Comment(IComment comment) => "";

            public string Doctype(IDocumentType doctype) => "";

            // Used for <script>, <style>, etc. We trim to avoid carrying over layout noise.
            public string LiteralText(ICharacterData text) => text.Data.Trim();

            public string OpenTag(IElement element, bool selfClosing) => element.LocalName switch
            {
                // Paragraph or heading = blank line
                "p" or "h1" or "h2" or "h3" or "h4" or "h5" or "h6" => "\n\n",
                // Line break = single newline
                "br" => "\n",
                // Span = inline, separated by a space
                "span" => " ",
                // otherwise don't add any whitespace
                _ => ""
            };

            public string Processing(IProcessingInstruction processing) => "";

            public string Text(ICharacterData text) => text.Data.Trim();
        }
    }
}
