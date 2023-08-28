using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Web;

namespace Kiss.Elastic.Sync.Sources
{
    public readonly record struct OverigObject(in JsonElement Id, in JsonElement Data);

    public sealed class ObjectenClient
    {
        private readonly HttpClient _httpClient;

        public ObjectenClient(Uri objectenBaseUri, string objectenToken)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = objectenBaseUri
            };
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", objectenToken);
        }

        public IAsyncEnumerable<OverigObject> GetObjecten(string type, CancellationToken token)
        {
            var url = $"/api/v2/objects?type={HttpUtility.UrlEncode(type)}";
            return GetObjectenInternal(url, token);
        }

        private async IAsyncEnumerable<OverigObject> GetObjectenInternal(string url, [EnumeratorCancellation] CancellationToken token)
        {
            string? next = null;

            using (var message = new HttpRequestMessage(HttpMethod.Get, url))
            {
                using var response = await _httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, token);

                if (response.IsSuccessStatusCode)
                {
                    await using var stream = await response.Content.ReadAsStreamAsync(token);
                    using var jsonDoc = await JsonDocument.ParseAsync(stream, cancellationToken: token);

                    if (!jsonDoc.TryParseZgwPagination(out var pagination))
                    {
                        yield break;
                    }

                    next = pagination.Next;

                    foreach (var item in pagination.Records)
                    {
                        if (!item.TryGetProperty("record", out var record) || record.ValueKind != JsonValueKind.Object ||
                            !record.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object ||
                            !item.TryGetProperty("uuid", out var uuid))
                        {
                            continue;
                        }

                        yield return new(uuid, data);
                    }
                }

                // 400 probably means there is something wrong with the objecttype. ignore it.
                else if (response.StatusCode != System.Net.HttpStatusCode.BadRequest)
                {
                    await Helpers.LogResponse(response, token);
                    response.EnsureSuccessStatusCode();
                }
            }

            if (!string.IsNullOrWhiteSpace(next))
            {
                await foreach (var el in GetObjectenInternal(next, token))
                {
                    yield return el;
                }
            }
        }

        public void Dispose() => _httpClient?.Dispose();
    }
}
