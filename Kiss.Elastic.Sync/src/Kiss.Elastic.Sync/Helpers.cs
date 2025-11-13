using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Kiss.Elastic.Sync
{
    public static partial class Helpers
    {
        public const string CompletionField = "_completion_all";
        public const string CrawlEngineName = "engine-crawler";

        public static void CancelSafely(this CancellationTokenSource source)
        {
            try
            {
                source.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        public static string GetRequiredEnvironmentVariable(string name) => GetOptionalEnvironmentVariable(name) ?? throw new Exception("missing environment variable: " + name);
        public static string? GetOptionalEnvironmentVariable(string name) => Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);

        public static string EncodeCredential(string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentNullException(nameof(userName));
            password ??= "";

            var encoding = Encoding.UTF8;
            var credential = $"{userName}:{password}";

            return Convert.ToBase64String(encoding.GetBytes(credential));
        }

        public static async Task LogResponse(HttpResponseMessage response, CancellationToken token)
        {
            await using var responseStream = await response.Content.ReadAsStreamAsync(token);
            await using var outStr = response.IsSuccessStatusCode ? Console.OpenStandardOutput() : Console.OpenStandardError();
            await responseStream.CopyToAsync(outStr, token);
            var writer = response.IsSuccessStatusCode ? Console.Out : Console.Error;
            await writer.WriteLineAsync();
        }

        public static Stream GetEmbedded(string endsWith)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(endsWith));
            return assembly.GetManifestResourceStream(resourceName) ?? throw new NullReferenceException();
        }

        public static async Task<HttpResponseMessage> HeadAsync(this HttpClient client, string? url, CancellationToken token)
        {
            using var headRequest = new HttpRequestMessage(HttpMethod.Head, url);
            return await client.SendAsync(headRequest, HttpCompletionOption.ResponseHeadersRead, token);
        }

        public static async Task<HttpResponseMessage> SendJsonAsync(this HttpClient client, HttpMethod httpMethod, string? url, JsonNode node, CancellationToken token)
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);
            node.WriteTo(writer);
            await writer.FlushAsync(token);
            stream.Seek(0, SeekOrigin.Begin);
            return await SendJsonAsync(client, httpMethod, url, stream, token);
        }

        public static async Task<HttpResponseMessage> SendJsonAsync(this HttpClient client, HttpMethod httpMethod, string? url, Stream jsonStream, CancellationToken token)
        {
            using var content = new StreamContent(jsonStream);
            using var request = new HttpRequestMessage(httpMethod, url)
            {
                Content = content
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
        }

        public static string GenerateValidIndexName(string input)
        {
            var escaped = EscapeElasticsearchRegex()
                .Replace(input, "-")
                .ToLowerInvariant();

            return $"search-{escaped}";
        }

        [GeneratedRegex("""[?|*|<|>|"| |\\|/|,|\|]+""")]
        private static partial Regex EscapeElasticsearchRegex();
    }
}
