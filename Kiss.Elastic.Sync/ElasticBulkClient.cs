using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Kiss.Elastic.Sync
{
    internal sealed class ElasticBulkClient : IDisposable
    {
        private readonly HttpClient _httpClient;

        public ElasticBulkClient(Uri baseUri, string username, string password)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = baseUri;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", EncodeCredential(username, password));
        }

        public static string EncodeCredential(string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentNullException(nameof(userName));
            password ??= "";

            var encoding = Encoding.UTF8;
            var credential = $"{userName}:{password}";

            return Convert.ToBase64String(encoding.GetBytes(credential));
        }

        public void Dispose() => _httpClient?.Dispose();

        public static ElasticBulkClient Create()
        {
            var elasticBaseUrl = Helpers.GetEnvironmentVariable("ELASTIC_BASE_URL");
            var username = Helpers.GetEnvironmentVariable("ELASTIC_USERNAME");
            var password = Helpers.GetEnvironmentVariable("ELASTIC_PASSWORD");

            if (!Uri.TryCreate(elasticBaseUrl, UriKind.Absolute, out var elasticBaseUri))
            {
                throw new Exception("elastic base url is niet valide: " + elasticBaseUrl);
            }

            return new ElasticBulkClient(elasticBaseUri, username, password);
        }

        public async Task IndexBulk(IAsyncEnumerable<KissEnvelope> envelopes, string bron, CancellationToken token)
        {
            var indexName = bron.ToLowerInvariant();
            if (!await EnsureIndex(indexName, token)) return;
            await using var enumerator = envelopes.GetAsyncEnumerator(token);
            var hasNext = await enumerator.MoveNextAsync();
            const long MaxLength = 50 * 1000 * 1000;
            const byte NewLine = (byte)'\n';
            while (hasNext)
            {
                long written = 0;
                using var content = new PushStreamContent(async (stream) =>
                {
                    using var writer = new Utf8JsonWriter(stream);

                    while (hasNext && written < MaxLength)
                    {
                        writer.WriteStartObject();
                        writer.WritePropertyName("index");
                        writer.WriteStartObject();
                        writer.WriteString("_index", indexName);
                        writer.WriteString("_id", enumerator.Current.Id);
                        writer.WriteEndObject();
                        writer.WriteEndObject();
                        await writer.FlushAsync(token);
                        written += writer.BytesCommitted;
                        writer.Reset();
                        stream.WriteByte(NewLine);
                        written++;

                        enumerator.Current.WriteTo(writer, bron);
                        await writer.FlushAsync(token);
                        written += writer.BytesCommitted;
                        writer.Reset();
                        stream.WriteByte(NewLine);
                        written++;

                        hasNext = await enumerator.MoveNextAsync();
                    }
                });
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                using var request = new HttpRequestMessage(HttpMethod.Post, "_bulk")
                {
                    Content = content,
                };
                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
                await LogResponse(response, token);
			}
        }

        private static async Task LogResponse(HttpResponseMessage response, CancellationToken token)
        {
			await using var responseStream = await response.Content.ReadAsStreamAsync(token);
			await using var outStr = response.IsSuccessStatusCode ? Console.OpenStandardOutput() : Console.OpenStandardError();
			await responseStream.CopyToAsync(outStr, token);
		}

        private async Task<bool> EnsureIndex(string indexName, CancellationToken token)
        {
            using var existsRequest = new HttpRequestMessage(HttpMethod.Head, indexName);
            using var existsResponse = await _httpClient.SendAsync(existsRequest, HttpCompletionOption.ResponseHeadersRead, token);

            if (existsResponse.IsSuccessStatusCode) return true;

            using var putRequest = new HttpRequestMessage(HttpMethod.Put, indexName);
            using var putResponse = await _httpClient.SendAsync(putRequest, HttpCompletionOption.ResponseHeadersRead, token);

			await LogResponse(putResponse, token);
			
            return putResponse.IsSuccessStatusCode;
        }

        private class PushStreamContent : HttpContent
        {
            private readonly Func<Stream, Task> _handler;

            public PushStreamContent(Func<Stream, Task> handler)
            {
                _handler = handler;
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
            {
                return _handler(stream);
            }

            protected override bool TryComputeLength(out long length)
            {
                length = -1;
                return false;
            }
        }
    }
}
