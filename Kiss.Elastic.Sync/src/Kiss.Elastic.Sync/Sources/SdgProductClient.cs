using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Kiss.Elastic.Sync.Sources
{
    public sealed class SdgProductClient : IKissSourceClient
    {
        private readonly HttpClient _httpClient;

        public string Source => "Kennisartikel";

        public IReadOnlyList<string> CompletionFields { get; } = new[]
        {
            "vertalingen.productTitelDecentraal",
            "vertalingen.specifiekeTekst"
        };

        public SdgProductClient(Uri baseUri, string apiKey)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = baseUri,
            };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", apiKey);
        }

        public IAsyncEnumerable<KissEnvelope> Get(CancellationToken token) => Get("/api/v1/producten", token);

        private async IAsyncEnumerable<KissEnvelope> Get(string url, [EnumeratorCancellation] CancellationToken token)
        {
            string? next = null;

            using (var message = new HttpRequestMessage(HttpMethod.Get, url))
            {
                using var response = await _httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, token);
                response.EnsureSuccessStatusCode();
                await using var stream = await response.Content.ReadAsStreamAsync(token);
                using var jsonDoc = await JsonDocument.ParseAsync(stream, cancellationToken: token);

                if (!jsonDoc.TryParseZgwPagination(out var pagination))
                {
                    yield break;
                }

                next = pagination.Next;

                foreach (var sdgProduct in pagination.Records)
                {
                    if (!sdgProduct.TryGetProperty("uuid", out var id))
                    {
                        continue;
                    }

                    string? title = default;
                    string? objectMeta = default;

                    if (sdgProduct.TryGetProperty("vertalingen", out var vertalingenProp) && vertalingenProp.ValueKind == JsonValueKind.Array)
                    {
                        var vertaling = vertalingenProp[0];
                        if (vertaling.TryGetProperty("productTitelDecentraal", out var titleProp) && titleProp.ValueKind == JsonValueKind.String)
                        {
                            title = titleProp.GetString();
                        }
                        if (vertaling.TryGetProperty("specifiekeTekst", out var objectMetaProp) && objectMetaProp.ValueKind == JsonValueKind.String)
                        {
                            objectMeta = objectMetaProp.GetString();
                        }
                    }

                    yield return new KissEnvelope(sdgProduct, title, objectMeta, $"kennisartikel_{id.GetString()}");
                }
            }

            if (!string.IsNullOrWhiteSpace(next))
            {
                await foreach (var el in Get(next, token))
                {
                    yield return el;
                }
            }
        }

        public void Dispose() => _httpClient.Dispose();
    }
}
