using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System.Linq;

namespace Kiss.Elastic.Sync.SharePoint
{
    public sealed class SharePointClient : IDisposable
    {
        private readonly GraphServiceClient _graphClient;
        private readonly string _siteIdentifier;
        private readonly HttpClient _httpClient;
        private readonly string _siteUrl;

        public SharePointClient(string tenantId, string clientId, string clientSecret, string siteUrl)
        {
            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            _graphClient = new GraphServiceClient(credential);
            _httpClient = new HttpClient();
            _siteUrl = siteUrl;
            _siteIdentifier = ParseSiteIdentifierFromUrl(siteUrl);
        }

        private static string ParseSiteIdentifierFromUrl(string siteUrl)
        {
            // Input:  https://{hostname}/{path}
            // Output: {hostname}:{path}
            var uri = new Uri(siteUrl);
            return $"{uri.Host}:{uri.AbsolutePath}";
        }

        public async Task<BaseSitePage?> GetPageByUrl(string pageUrl, CancellationToken token)
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
                    .GetAsync(cancellationToken: token);

                if (pagesResponse?.Value == null || pagesResponse.Value.Count == 0)
                {
                    Console.WriteLine($"Geen pagina's gevonden op site: {site.DisplayName} ({_siteUrl})");
                    return null;
                }

                // Filter op specifieke pagina (heeft nog niet de canvasLayout)
                var uri = new Uri(pageUrl);
                var pageFileName = Path.GetFileName(uri.AbsolutePath);  
                var pageMetadata = pagesResponse.Value.FirstOrDefault(p =>
                    p.Name?.Equals(pageFileName, StringComparison.OrdinalIgnoreCase) == true);

                if (pageMetadata == null)
                {
                    Console.WriteLine($"Pagina {pageFileName} niet gevonden. Beschikbare pagina's:");
                    foreach (var p in pagesResponse.Value)
                    {
                        Console.WriteLine($"  - {p.Name} (ID: {p.Id})");
                    }
                    return null;
                }

                // Haal canvasLayout van pagina op
                try
                {
                    // Maak alleen een RequestInformation object
                    var graphPageRequest = _graphClient
                        .Sites[site.Id]
                        .Pages[pageMetadata.Id]
                        .ToGetRequestInformation();

                    // Workaround: De expand canvasLayout werkt alleen met type cast /microsoft.graph.sitePage.
                    graphPageRequest.UrlTemplate =
                        $"{{+baseurl}}/sites/{site.Id}/pages/{pageMetadata.Id}/microsoft.graph.sitePage?$expand=canvasLayout";

                    // Gebruik batch request om aangepaste UrlTemplate te versturen (GetAsync ondersteunt dit niet)
                    var content = new Microsoft.Graph.BatchRequestContentCollection(_graphClient);
                    var requestStepId = await content.AddBatchRequestStepAsync(graphPageRequest);
                    var batchResult = await _graphClient.Batch.PostAsync(content, cancellationToken: token);
                    var graphPage = await batchResult.GetResponseByIdAsync<SitePage>(requestStepId);

                    if (graphPage?.CanvasLayout != null)
                    {
                        var sectionCount = graphPage.CanvasLayout.HorizontalSections?.Count ?? 0;
                        return graphPage;
                    }

                    Console.WriteLine($"CanvasLayout is leeg");
                    return pageMetadata;
                }
                catch (Exception canvasEx)
                {
                    Console.WriteLine($"Fout bij ophalen CanvasLayout: {canvasEx.Message}");
                    return pageMetadata;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fout bij ophalen SharePoint pagina: {ex.Message}");
                return null;
            }
        }

        public static string ExtractTextFromPage(BaseSitePage page)
        {
            var textParts = new List<string>();

            if (!string.IsNullOrWhiteSpace(page.Title))
            {
                textParts.Add(page.Title);
            }

            if (!string.IsNullOrWhiteSpace(page.Description))
            {
                textParts.Add(page.Description);
            }

            if (page is not SitePage sitePage || sitePage.CanvasLayout == null)
            {
                return string.Join("\n\n", textParts.Where(t => !string.IsNullOrWhiteSpace(t)));
            }

            if (sitePage.CanvasLayout.HorizontalSections != null)
            {
                foreach (var section in sitePage.CanvasLayout.HorizontalSections)
                {
                    if (section?.Columns == null) continue;

                    foreach (var column in section.Columns)
                    {
                        if (column?.Webparts == null) continue;

                        foreach (var webpart in column.Webparts)
                        {
                            if (webpart is Microsoft.Graph.Models.TextWebPart textWebPart)
                            {
                                if (!string.IsNullOrWhiteSpace(textWebPart.InnerHtml))
                                {
                                    var plainText = StripHtmlTags(textWebPart.InnerHtml);
                                    textParts.Add(plainText);
                                }
                            }

                            var additionalData = webpart.AdditionalData;
                            if (additionalData != null && additionalData.TryGetValue("innerHtml", out var innerHtmlObj))
                            {
                                var innerHtml = innerHtmlObj?.ToString();
                                if (!string.IsNullOrWhiteSpace(innerHtml))
                                {
                                    var plainText = StripHtmlTags(innerHtml);
                                    textParts.Add(plainText);
                                }
                            }
                        }
                    }
                }
            }

            return string.Join("\n\n", textParts.Where(t => !string.IsNullOrWhiteSpace(t)));
        }

        private static string StripHtmlTags(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            var text = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", " ");

            text = text.Replace("&nbsp;", " ")
                       .Replace("&amp;", "&")
                       .Replace("&lt;", "<")
                       .Replace("&gt;", ">")
                       .Replace("&quot;", "\"");

            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");

            return text.Trim();
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
