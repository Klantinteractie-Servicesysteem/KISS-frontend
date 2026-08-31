using System.Runtime.CompilerServices;
using System.Text.Json;
using Kiss.Elastic.Sync.KennisApi;

namespace Kiss.Elastic.Sync.Sources
{
    /// <summary>
    /// Sync source for kennisartikelen from the Polly Kennis API.
    /// Uses /kennisartikelen/zoek to discover IDs, then /kennisartikelen/selecteer to fetch full content.
    /// </summary>
    public sealed class KennisApiKennisartikelClient(KennisApiClient client) : IKissSourceClient
    {
        private readonly KennisApiClient _client = client;

        public string Source => "kennis-api-artikel";

        public IReadOnlyList<string> CompletionFields { get; } =
        [
            "titel",
            "trefwoorden",
            "secties.inhoud"
        ];

        public async IAsyncEnumerable<KissEnvelope> Get([EnumeratorCancellation] CancellationToken token)
        {
            // First collect all IDs via zoek endpoint
            var ids = new List<string>();
            await foreach (var id in _client.GetAllIds("kennisartikelen/zoek", token))
            {
                ids.Add(id);
            }

            // Then fetch full content in batches via selecteer
            await foreach (var item in _client.SelecteerBatch("kennisartikelen/selecteer", ids, token))
            {
                var id = item.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.String
                    ? idProp.GetString()!
                    : null;

                if (id == null) continue;

                var title = item.TryGetProperty("titel", out var titelProp) && titelProp.ValueKind == JsonValueKind.String
                    ? titelProp.GetString()
                    : null;

                // Use first section content as objectMeta for search snippet
                string? objectMeta = null;
                if (item.TryGetProperty("secties", out var secties) && secties.ValueKind == JsonValueKind.Array)
                {
                    foreach (var sectie in secties.EnumerateArray())
                    {
                        if (sectie.TryGetProperty("inhoud", out var inhoud) && inhoud.ValueKind == JsonValueKind.String)
                        {
                            objectMeta = inhoud.GetString();
                            break;
                        }
                    }
                }

                yield return new KissEnvelope(item, title, objectMeta, $"kennis-api-artikel_{id}");
            }
        }

        public void Dispose() => _client.Dispose();
    }
}
