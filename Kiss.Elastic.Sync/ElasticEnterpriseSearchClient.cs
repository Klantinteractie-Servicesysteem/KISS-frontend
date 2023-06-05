using System.Net.Http.Headers;
using System.Text.Json;

namespace Kiss.Elastic.Sync
{
	public sealed class ElasticEnterpriseSearchClient : IDisposable
	{
		const int MaxDocuments = 100;

		private readonly HttpClient _httpClient;

		public ElasticEnterpriseSearchClient(Uri baseUri, string apiKey)
		{
			var handler = new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback = (_, _, _, _) => true
			};

			_httpClient = new HttpClient(handler);
			_httpClient.BaseAddress = baseUri;
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
		}

		public async Task IndexDocumentsAsync(IAsyncEnumerable<KissEnvelope> documents, string engine, string bron, CancellationToken token)
		{
			await using var standardOutput = Console.OpenStandardOutput();
			await using var standardError = Console.OpenStandardError();
			var url = $"/api/as/v1/engines/{engine}/documents";
			await using var enumerator = documents.GetAsyncEnumerator(token);
			var hasData = await enumerator.MoveNextAsync();

			while (hasData)
			{
				// enterprise search demands a content-length header. by writing to a file first, we know the content-length in advance, without needing to load anything into memory.
				await using var stream = new FileStream(
					Path.GetTempFileName(),
					FileMode.OpenOrCreate,
					FileAccess.ReadWrite,
					FileShare.None,
					4096,
					FileOptions.RandomAccess | FileOptions.DeleteOnClose | FileOptions.Asynchronous);

				using var jsonWriter = new Utf8JsonWriter(stream);
				var count = 0;
				jsonWriter.WriteStartArray();

				while (true)
				{
					enumerator.Current.WriteTo(jsonWriter, bron);
					hasData = await enumerator.MoveNextAsync();
					count++;

					if (!hasData || count >= MaxDocuments)
					{
						jsonWriter.WriteEndArray();
						await jsonWriter.FlushAsync(token);
						break;
					}

					await jsonWriter.FlushAsync(token);
				}

				stream.Seek(0, SeekOrigin.Begin);

				var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
				{
					Content = new StreamContent(stream)
				};
				requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
				// https://www.stevejgordon.co.uk/using-httpcompletionoption-responseheadersread-to-improve-httpclient-performance-dotnet
				using var response = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, token);
				await using var responseStream = await response.Content.ReadAsStreamAsync(token);
				var outputStream = response.IsSuccessStatusCode
					? standardOutput
					: standardError;
				await responseStream.CopyToAsync(outputStream, token);
				response.EnsureSuccessStatusCode();
			}
		}

		public void Dispose() => _httpClient.Dispose();
	}
}
