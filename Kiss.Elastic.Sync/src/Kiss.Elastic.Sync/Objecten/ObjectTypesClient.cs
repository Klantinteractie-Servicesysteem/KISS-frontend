using System.Runtime.CompilerServices;
using System.Text.Json;
using Kiss.Elastic.Sync.Sources;

namespace Kiss.Elastic.Sync.Objecten
{
    internal class ObjectTypesClient : IDisposable
    {
        private readonly HttpClient _httpClient;

        public ObjectTypesClient(Uri objectTypesBaseUri, string token)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = objectTypesBaseUri
            };
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", token);
        }

        public void Dispose() => _httpClient.Dispose();

        public IAsyncEnumerable<string> GetObjectTypes(string name, CancellationToken token) => GetObjectTypes(name, "api/v2/objecttypes", token);

        private async IAsyncEnumerable<string> GetObjectTypes(string name, string url, [EnumeratorCancellation] CancellationToken token)
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

                foreach (var objectType in pagination.Records)
                {
                    if (objectType.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String && nameProp.ValueEquals(name) &&
                        objectType.TryGetProperty("url", out var objectUrl) && objectUrl.ValueKind == JsonValueKind.String)
                    {
                        var result = objectUrl.GetString();
                        if (!string.IsNullOrWhiteSpace(result))
                        {
                            yield return result;
                        }
                    }
                }
            };

            if (!string.IsNullOrWhiteSpace(next))
            {
                await foreach (var item in GetObjectTypes(name, next, token))
                {
                    yield return item;
                }
            }
        }
    }
}
