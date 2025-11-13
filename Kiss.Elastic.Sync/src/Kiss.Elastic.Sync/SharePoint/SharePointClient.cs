using System.Runtime.CompilerServices;
using AngleSharp;
using AngleSharp.Dom;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;

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
            // Check site via Graph API lookup (GUID is nodig om paginas en subsites op te halen)
            var rootSite = await _graphClient.Sites[_siteIdentifier].GetAsync(cancellationToken: token);
            if (rootSite == null)
            {
                yield break;
            }
            // Haal alle sites op met GUID
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

        private async IAsyncEnumerable<TEntity> IterateAllEntities<TCollection, TEntity>(
            Task<TCollection?> firstPageTask,
            Func<TCollection, List<TEntity>?> getEntities,
            [EnumeratorCancellation] CancellationToken token)
            where TCollection : BaseCollectionPaginationCountResponse, new()
        {
            var response = await firstPageTask;

            while (response != null && getEntities(response) is { } entities)
            {
                foreach (var item in entities)
                {
                    yield return item;
                }
                if (string.IsNullOrEmpty(response.OdataNextLink))
                {
                    yield break;
                }
                var requestInfo = new RequestInformation
                {
                    HttpMethod = Method.GET,
                    UrlTemplate = response.OdataNextLink,
                };
                response = await _graphClient.RequestAdapter.SendAsync<TCollection>(requestInfo, (_) => new(), cancellationToken: token);
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
