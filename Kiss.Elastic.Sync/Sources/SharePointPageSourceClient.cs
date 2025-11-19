using System.Runtime.CompilerServices;
using System.Text.Json;
using Kiss.Elastic.Sync.SharePoint;

namespace Kiss.Elastic.Sync.Sources
{
    public sealed class SharePointPageSourceClient(SharePointClient sharePointClient, string source) : IKissSourceClient
    {
        public string Source => source;

        public IReadOnlyList<string> CompletionFields { get; } = new[]
        {
            "title",
            "content"
        };

        public async IAsyncEnumerable<KissEnvelope> Get([EnumeratorCancellation] CancellationToken token)
        {
            await foreach (var page in sharePointClient.GetAllPages(token))
            {
                var (content, headings) = await sharePointClient.ExtractTextFromPage(page);
                var pageData = new
                {
                    id = page.Id,
                    title = page.Title ?? "Geen titel",
                    content,
                    headings,
                    url = page.WebUrl,
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
                    Url: page.WebUrl
                );

                Console.WriteLine($"Page indexed: {pageData.title}");
            }
        }

        public void Dispose()
        {
            sharePointClient?.Dispose();
        }
    }
}
