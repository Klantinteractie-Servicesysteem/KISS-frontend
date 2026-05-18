using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Kiss.Elastic.Sync.Sources
{
    public sealed class SdgProductClient : IKissSourceClient
    {
        private readonly ObjectenClient _objectenClient;
        private readonly string _objecttypeUrl;

        public string Source => "Kennisbank";

        public IReadOnlyList<string> CompletionFields { get; } = new[]
        {
            "vertalingen.tekst",
            "vertalingen.titel"
        };

        public SdgProductClient(ObjectenClient objectenClient, string objecttypeUrl)
        {
            _objectenClient = objectenClient;
            _objecttypeUrl = objecttypeUrl;
        }

        public async IAsyncEnumerable<KissEnvelope> Get([EnumeratorCancellation] CancellationToken token)
        {
            await foreach (var item in _objectenClient.GetObjecten(_objecttypeUrl, token))
            {
                if (!item.Data.TryGetProperty("uuid", out var id))
                {
                    continue;
                }

                string? title = default;
                string? objectMeta = default;

                if (item.Data.TryGetProperty("vertalingen", out var vertalingenProp) && vertalingenProp.ValueKind == JsonValueKind.Array)
                {
                    var vertaling = vertalingenProp[0];
                    if (vertaling.TryGetProperty("titel", out var titleProp) && titleProp.ValueKind == JsonValueKind.String)
                    {
                        title = titleProp.GetString();
                    }
                    if (vertaling.TryGetProperty("tekst", out var objectMetaProp) && objectMetaProp.ValueKind == JsonValueKind.String)
                    {
                        objectMeta = objectMetaProp.GetString();
                    }
                }

                yield return new KissEnvelope(item.Data, title, objectMeta, $"kennisbank_{id.GetString()}");
            }
        }

        public void Dispose() => _objectenClient.Dispose();
    }
}
