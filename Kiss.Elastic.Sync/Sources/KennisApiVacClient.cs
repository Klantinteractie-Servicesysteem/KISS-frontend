using System.Runtime.CompilerServices;
using System.Text.Json;
using Kiss.Elastic.Sync.KennisApi;

namespace Kiss.Elastic.Sync.Sources
{
    /// <summary>
    /// Sync source for VAC's from the Polly Kennis API.
    /// Uses /vacs/zoek to discover IDs, then /vacs/selecteer to fetch full content.
    /// </summary>
    public sealed class KennisApiVacClient(KennisApiClient client) : IKissSourceClient
    {
        private readonly KennisApiClient _client = client;

        public string Source => "kennis-api-vac";

        public IReadOnlyList<string> CompletionFields { get; } =
        [
            "vraag",
            "trefwoorden"
        ];

        public async IAsyncEnumerable<KissEnvelope> Get([EnumeratorCancellation] CancellationToken token)
        {
            // First collect all IDs via zoek endpoint
            var ids = new List<string>();
            await foreach (var id in _client.GetAllIds("vacs/zoek", token))
            {
                ids.Add(id);
            }

            // Then fetch full content in batches via selecteer
            await foreach (var item in _client.SelecteerBatch("vacs/selecteer", ids, token))
            {
                var id = item.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.String
                    ? idProp.GetString()!
                    : null;

                if (id == null) continue;

                var title = item.TryGetProperty("vraag", out var vraagProp) && vraagProp.ValueKind == JsonValueKind.String
                    ? vraagProp.GetString()
                    : null;

                var objectMeta = item.TryGetProperty("antwoord", out var antwoordProp) && antwoordProp.ValueKind == JsonValueKind.String
                    ? antwoordProp.GetString()
                    : null;

                yield return new KissEnvelope(item, title, objectMeta, $"kennis-api-vac_{id}");
            }
        }

        public void Dispose() => _client.Dispose();
    }
}
