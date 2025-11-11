using AngleSharp;
using AngleSharp.Dom;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace Kiss.Elastic.Sync.SharePoint
{
    public sealed class SharePointClient : IDisposable
    {
        private readonly GraphServiceClient _graphClient;
        private readonly string _siteIdentifier;
        private readonly IBrowsingContext _context;
        private readonly string _siteUrl;

        public SharePointClient(string tenantId, string clientId, string clientSecret, string siteUrl)
        {
            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            _graphClient = new GraphServiceClient(credential);
            _siteUrl = siteUrl;
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

        public async Task<SitePage?> GetPageByUrl(string pageUrl, CancellationToken token)
        {
            try
            {
                // Check site via Graph API lookup (GUID is nodig om paginas op te halen)
                var site = await _graphClient.Sites[_siteIdentifier].GetAsync(cancellationToken: token);

                if (site?.Id == null)
                {
                    Console.WriteLine($"Site niet gevonden: {_siteUrl} (identifier: {_siteIdentifier})");
                    return null;
                }

                // Haal alle pagina's op met GUID
                var pagesResponse = await _graphClient
                    .Sites[site.Id]
                    .Pages
                    .GraphSitePage
                    .GetAsync(x =>
                    {
                        // we filteren nu nog op de url van de pagina. in de toekomst halen we alle pagina's bij een site op.
                        x.QueryParameters.Filter = $"webUrl eq '{pageUrl}'";
                        x.QueryParameters.Expand = ["webparts"];
                    }, cancellationToken: token);

                if (pagesResponse?.Value == null || pagesResponse.Value.Count == 0)
                {
                    Console.WriteLine($"Geen pagina's gevonden op site: {site.DisplayName} ({_siteUrl})");
                    return null;
                }

                var firstPage = pagesResponse.Value[0];
                return firstPage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fout bij ophalen SharePoint pagina: {ex.Message}");
                return null;
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

            var content = doc.ToHtml(MyMarkupFormatter.Instance).Trim();

            return (content, headings);
        }

        public void Dispose()
        {
            _graphClient.Dispose();
        }

        private class MyMarkupFormatter : IMarkupFormatter
        {
            public static readonly IMarkupFormatter Instance = new MyMarkupFormatter();

            private MyMarkupFormatter()
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
