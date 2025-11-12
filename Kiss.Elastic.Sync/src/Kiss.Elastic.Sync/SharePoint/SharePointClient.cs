using System.Runtime.CompilerServices;
using AngleSharp;
using AngleSharp.Dom;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using Channel = System.Threading.Channels.Channel;

namespace Kiss.Elastic.Sync.SharePoint
{
    public sealed class SharePointClient : IDisposable
    {
        private readonly GraphServiceClient _graphClient;
        private readonly string _siteIdentifier;
        private readonly IBrowsingContext _context;

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
            var rootSite = await _graphClient.Sites[_siteIdentifier].GetAsync(cancellationToken: token);
            if (rootSite == null)
            {
                yield break;
            }
            await foreach (var subSite in GetAllSitesRecursive(rootSite, token))
            {
                await foreach (var page in GetAllPages(subSite, token))
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

        private async IAsyncEnumerable<Site> GetAllSitesRecursive(Site root, [EnumeratorCancellation] CancellationToken token)
        {
            yield return root;
            await foreach (var site in GetAllSubSites(root, token))
            {
                await foreach (var subSite in GetAllSitesRecursive(site, token))
                {
                    yield return subSite;
                }
            }
        }

        private IAsyncEnumerable<Site> GetAllSubSites(Site root, CancellationToken token)
        {
            var request = _graphClient.Sites[root.Id]
                .Sites
                .ToGetRequestInformation();

            return IterateAllEntities<SiteCollectionResponse, Site>(request, token);
        }

        private IAsyncEnumerable<SitePage> GetAllPages(Site site, CancellationToken token)
        {
            // Haal alle pagina's op met GUID
            var pagesRequest = _graphClient
                .Sites[site.Id]
                .Pages
                .GraphSitePage
                .ToGetRequestInformation(x => x.QueryParameters.Expand = ["webparts"]);

            return IterateAllEntities<SitePageCollectionResponse, SitePage>(pagesRequest, token);
        }

        /// <summary>
        /// Returns an asynchronous sequence that iterates over all entities across paginated responses for the
        /// specified request.
        /// </summary>
        /// <remarks>Entities are streamed as they are retrieved, allowing for efficient processing of
        /// large datasets. The iteration completes when all pages have been processed or when the cancellation token is
        /// triggered.</remarks>
        /// <typeparam name="TCollection">The type representing the paginated collection response. Must inherit from
        /// BaseCollectionPaginationCountResponse and have a parameterless constructor.</typeparam>
        /// <typeparam name="TEntity">The type of entity contained within each page of the collection.</typeparam>
        /// <param name="request">The request information used to retrieve the paginated data.</param>
        /// <param name="token">A cancellation token that can be used to cancel the asynchronous iteration.</param>
        /// <returns>An IAsyncEnumerable of TEntity that yields each entity from all pages of the response.</returns>
        private IAsyncEnumerable<TEntity> IterateAllEntities<TCollection, TEntity>(RequestInformation request, CancellationToken token) where TCollection : BaseCollectionPaginationCountResponse, new()
        {
            var channel = Channel.CreateUnbounded<TEntity>();

            Task.Run(async () =>
            {
                var response = await _graphClient.RequestAdapter.SendAsync<TCollection>(request, (_) => new(), cancellationToken: token);
                if (response == null) return;
                var iterator = PageIterator<TEntity, TCollection>.CreatePageIterator(_graphClient, response, async (x) =>
                {
                    await channel.Writer.WriteAsync(x, token);
                    return true;
                });
                await iterator.IterateAsync(token);
                channel.Writer.Complete();
            }, token);

            return channel.Reader.ReadAllAsync(token);
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
