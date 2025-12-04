using System.Runtime.CompilerServices;
using System.Text.Json;
using Kiss.Elastic.Sync.SharePoint;

namespace Kiss.Elastic.Sync.Sources
{
    public sealed partial class SharePointPageSourceClient(SharePointClient sharePointClient, string source) : IKissSourceClient
    {
        public string Source => source;

        public IReadOnlyList<string> CompletionFields { get; } = new[]
        {
            "title",
            "content"
        };

        public async IAsyncEnumerable<KissEnvelope> Get([EnumeratorCancellation] CancellationToken token)
        {
            await foreach (var pageData in sharePointClient.GetAllPages(token))
            {
                var data = JsonSerializer.SerializeToElement(pageData);

                yield return new KissEnvelope(
                    Object: data,
                    Title: pageData.Title,
                    ObjectMeta: "",
                    Id: $"${Helpers.GenerateValidIndexName(Source)}_{pageData.Id}",
                    Url: pageData.Url
                );

                Console.WriteLine($"Page indexed: {pageData.Title}");
            }
        }

        public void Dispose()
        {
            sharePointClient?.Dispose();
        }
    }
}
