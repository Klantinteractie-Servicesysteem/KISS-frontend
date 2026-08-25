using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Kiss.Elastic.Sync.KennisApi
{
    /// <summary>
    /// HTTP client for the Polly Kennis API.
    /// Handles OAuth2 client-credentials token acquisition and provides
    /// zoek (list IDs) and selecteer (batch retrieve) operations.
    /// </summary>
    public sealed class KennisApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string? _publicationId;
        private string? _accessToken;
        private DateTimeOffset _tokenExpiry = DateTimeOffset.MinValue;

        public KennisApiClient(Uri baseUri, string clientId, string clientSecret, string? publicationId)
        {
            _httpClient = new HttpClient { BaseAddress = baseUri };
            _clientId = clientId;
            _clientSecret = clientSecret;
            _publicationId = publicationId;
        }

        private async Task EnsureTokenAsync(CancellationToken token)
        {
            if (_accessToken != null && DateTimeOffset.UtcNow < _tokenExpiry.AddMinutes(-1))
                return;

            var formData = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _clientId,
                ["client_secret"] = _clientSecret,
            };

            if (!string.IsNullOrWhiteSpace(_publicationId))
            {
                formData["publication_id"] = _publicationId;
            }

            using var content = new FormUrlEncodedContent(formData);
            using var request = new HttpRequestMessage(HttpMethod.Post, "token") { Content = content };
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(token);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: token);

            _accessToken = doc.RootElement.GetProperty("access_token").GetString()
                ?? throw new Exception("Kennis API token response missing access_token");

            var expiresIn = doc.RootElement.GetProperty("expires_in").GetInt32();
            _tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(expiresIn);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        /// <summary>
        /// Pages through all items via a zoek endpoint using offset pagination.
        /// </summary>
        public async IAsyncEnumerable<string> GetAllIds(
            string zoekPath,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await EnsureTokenAsync(cancellationToken);

            var offset = 0;
            const int Limit = 1000;
            int totalReturned;

            do
            {
                // vertrouwelijkheid=intern is hardcoded: Polly serves only internal publications
                var url = $"{zoekPath}?vertrouwelijkheid=intern&limit={Limit}&offset={offset}";
                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

                var items = doc.RootElement.GetProperty("items");
                totalReturned = items.GetArrayLength();

                foreach (var item in items.EnumerateArray())
                {
                    if (item.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.String)
                    {
                        yield return idProp.GetString()!;
                    }
                }

                offset += totalReturned;
            }
            while (totalReturned == Limit);
        }

        /// <summary>
        /// Retrieves full items via a selecteer endpoint in batches of up to 100.
        /// </summary>
        public async IAsyncEnumerable<JsonElement> SelecteerBatch(
            string selecteerPath,
            IReadOnlyList<string> ids,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await EnsureTokenAsync(cancellationToken);

            const int BatchSize = 100;

            for (var i = 0; i < ids.Count; i += BatchSize)
            {
                var batch = ids.Skip(i).Take(BatchSize).ToList();
                var requestBody = BuildSelecteerRequestBody(batch);

                using var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                using var request = new HttpRequestMessage(HttpMethod.Post, selecteerPath) { Content = content };
                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

                if (doc.RootElement.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in items.EnumerateArray())
                    {
                        yield return item.Clone();
                    }
                }
            }
        }

        private static string BuildSelecteerRequestBody(List<string> ids)
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);

            writer.WriteStartObject();
            writer.WriteStartArray("items");
            foreach (var id in ids)
            {
                writer.WriteStartObject();
                writer.WriteString("id", id);
                writer.WriteString("taal", "*");
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
            writer.Flush();

            return Encoding.UTF8.GetString(stream.ToArray());
        }

        public void Dispose() => _httpClient?.Dispose();
    }
}
