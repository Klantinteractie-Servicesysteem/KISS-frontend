using System.Runtime.CompilerServices;
using System.Text.Json;
using Kiss.Elastic.Sync.SharePoint;

namespace Kiss.Elastic.Sync.Sources
{
    public sealed class SharePointPageSourceClient : IKissSourceClient
    {
        private readonly SharePointClient _sharePointClient;
        private readonly string _pageUrl;

        public string Source => "SharePoint";

        public IReadOnlyList<string> CompletionFields { get; } = new[]
        {
            "title",
            "content"
        };

        public SharePointPageSourceClient(SharePointClient sharePointClient, string pageUrl)
        {
            _sharePointClient = sharePointClient;
            _pageUrl = pageUrl;
        }

        public async IAsyncEnumerable<KissEnvelope> Get([EnumeratorCancellation] CancellationToken token)
        {
            var page = await _sharePointClient.GetPageByUrl(_pageUrl, token);

            if (page == null)
            {
                yield break;
            }

            var (content, headings) = await _sharePointClient.ExtractTextFromPage(page);
            var pageData = new
            {
                id = page.Id,
                title = page.Title ?? "Geen titel",
                content,
                headings,
                url = _pageUrl,
                lastModified = page.LastModifiedDateTime?.UtcDateTime ?? DateTime.UtcNow,
                createdBy = page.CreatedBy?.User?.DisplayName,
                lastModifiedBy = page.LastModifiedBy?.User?.DisplayName
            };

            var data = JsonSerializer.SerializeToElement(pageData);

            yield return new KissEnvelope(
                Object: data,
                Title: pageData.title,
                ObjectMeta: "",
                Id: $"sharepoint_{pageData.id}",
                Url: _pageUrl
            );

            Console.WriteLine($"Page indexed: {pageData.title}");
        }

        public void Dispose()
        {
            _sharePointClient?.Dispose();
        }
    }
}
