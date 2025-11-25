using System.Runtime.CompilerServices;
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
    public sealed class SharePointClient : IDisposable
    {
        private readonly GraphServiceClient _graphClient;
        private readonly string _siteIdentifier;
        private readonly IBrowsingContext _context;
        
        // this is the same error mapping that is used by the GraphServiceClient internally 
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

        public async IAsyncEnumerable<SitePage> GetAllPages([EnumeratorCancellation] CancellationToken token)
        {
            // Check site via Graph API lookup (Guid is necessary to retrieve pages and subsites)
            var startingSite = await _graphClient.Sites[_siteIdentifier].GetAsync(
                x=> x.QueryParameters.Select = [
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
                // a subsite can never be a hub site, so we MUST ONLY look for nested subsites
                ? GetAllSubSitesRecursive 
                // a root site can be a hub site. the method below returns all linked sites and all (nested) subsites
                // we should never call this method if the starting site is a subsite, because then it includes it's parent(s)
                : GetAllLinkedSites;

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

        public async Task<(IReadOnlyCollection<string> Content, IReadOnlyCollection<string> Headings)> ExtractTextFromPage(SitePage sitePage)
        {
            var allContent = new HashSet<string>();
            var allHeadings = new HashSet<string>();
            var allHtml = GetWebParts(sitePage)
                .SelectMany(GetHtml);

            foreach (var html in allHtml)
            {
                var (content, headings) = await ParseHtml(html);
                allContent.Add(content);
                foreach (var heading in headings)
                {
                    allHeadings.Add(heading);
                }
            }

            return (allContent, allHeadings);
        }

        private static bool IsSubsite(Site site) =>
            !string.IsNullOrWhiteSpace(site?.ParentReference?.SiteId);

        private async IAsyncEnumerable<Site> GetAllLinkedSites(Site rootSite, [EnumeratorCancellation] CancellationToken token)
        {
            if (!Guid.TryParse(rootSite.SharepointIds?.SiteId, out var siteId))
            {
                yield break;
            }
            
            var requestInfo = _graphClient.Sites.ToGetRequestInformation(x =>
            {
                x.QueryParameters.Select = ["id"];
            });
            
            // prepend the url template with the `search` query parameter.
            // NB: this is NOT the `$search` odata parameter
            requestInfo.UrlTemplate = requestInfo.UrlTemplate?.Replace("{?", "{?search,");
            // fill in the `search` query parameter. DepartmentId is a legacy field,
            // but it's the only way to get the sites that are linked to a hub site.
            requestInfo.QueryParameters.Add("search", $"DepartmentId:{{{siteId}}}");
            
            var firstPage = _graphClient.RequestAdapter.SendAsync(
                requestInfo,
                SiteCollectionResponse.CreateFromDiscriminatorValue, 
                s_errorMapping, 
                token);

            await foreach (var site in IterateAllEntities(firstPage, x => x.Value, token))
            {
                // this query also returns the root site, we don't want that
                if(site.Id == rootSite.Id) continue;
                yield return site;
            }
        }
        
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
        
        private IAsyncEnumerable<Site> GetAllSubSites(Site root, CancellationToken token)
        {
            var firstPage = _graphClient.Sites[root.Id]
                .Sites
                .GetAsync(cancellationToken: token);

            return IterateAllEntities(firstPage, x => x.Value, token);
        }

        private IAsyncEnumerable<SitePage> GetAllPages(Site site, CancellationToken token)
        {
            var firstPage = _graphClient
                .Sites[site.Id]
                .Pages
                .GraphSitePage
                .GetAsync(x => x.QueryParameters.Expand = ["webparts"], token);

            return IterateAllEntities(firstPage, x => x.Value, token);
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
                    UrlTemplate = response.OdataNextLink,
                };
                // Retrieve the next page
                response = await _graphClient.RequestAdapter.SendAsync<TCollection>(
                    requestInfo, 
                    (_) => new(),
                    s_errorMapping,
                    token);
            }
        }

        private static IEnumerable<WebPart> GetWebParts(SitePage sitePage) =>
            sitePage.WebParts
                ?.AsEnumerable() ?? [];

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

        private async Task<(string Content, IReadOnlyCollection<string> Headings)> ParseHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return ("", []);

            using var doc = await _context.OpenAsync(req => req.Content(html));

            var headings = doc.QuerySelectorAll("h1,h2,h3,h4,h5,h6")
                .Select(x => x.TextContent)
                .ToHashSet();

            var content = doc.ToHtml(TextWithWhitespaceFormatter.Instance).Trim();

            return (content, headings);
        }

        public void Dispose()
        {
            _graphClient.Dispose();
            _context.Dispose();
        }

        private class TextWithWhitespaceFormatter : IMarkupFormatter
        {
            public static readonly IMarkupFormatter Instance = new TextWithWhitespaceFormatter();

            private TextWithWhitespaceFormatter()
            {

            }

            public string CloseTag(IElement element, bool selfClosing) => "";

            public string Comment(IComment comment) => "";

            public string Doctype(IDocumentType doctype) => "";

            public string LiteralText(ICharacterData text) => text.Data.Trim();

            public string OpenTag(IElement element, bool selfClosing) => element.LocalName switch
            {
                "p" => "\n\n",
                "br" => "\n",
                "span" => " ",
                _ => ""
            };

            public string Processing(IProcessingInstruction processing) => "";

            public string Text(ICharacterData text) => text.Data.Trim();
        }
    }
}
