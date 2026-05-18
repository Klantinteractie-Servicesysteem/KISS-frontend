using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Kiss.Elastic.Sync.Sources
{
    internal sealed class ObjectenVacClient : IKissSourceClient
    {
        private readonly ObjectenClient _objectenClient;
        private readonly string _objecttypeUrl;

        public ObjectenVacClient(ObjectenClient objectenClient, string objecttypeUrl)
        {
            _objectenClient = objectenClient;
            _objecttypeUrl = objecttypeUrl;
        }

        public string Source => "VAC";

        public IReadOnlyList<string> CompletionFields { get; } = new[]
        {
            "vraag",
            "trefwoorden.trefwoord"
        };

        public void Dispose()
        {
            _objectenClient.Dispose();
        }

        public async IAsyncEnumerable<KissEnvelope> Get([EnumeratorCancellation] CancellationToken token)
        {
            await foreach (var item in _objectenClient.GetObjecten(_objecttypeUrl, token))
            {
                var id = $"vac_{item.Id.GetString()}";
                var title = item.Data.TryGetProperty("vraag", out var titleProp) && titleProp.ValueKind == JsonValueKind.String
                    ? titleProp.GetString()
                    : "";
                yield return new KissEnvelope(item.Data, title, null, id);
            }
        }
    }
}
