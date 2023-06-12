using Microsoft.AspNetCore.Http.Extensions;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Kiss.Elastic.Sync.Sources
{
	public sealed class ObjectenMedewerkerClient : IKissSourceClient
	{
		private readonly HttpClient _httpClient;
		private static readonly string[] s_nameProps = new[] { "voornaam", "voorvoegselAchternaam", "achternaam" };
		private static readonly string[] s_metaProps = new[] { "function", "department", "skills" };
		private readonly Uri _objectenBaseUri;
		private readonly string _objectenToken;
		private readonly Uri _objectTypesBaseUri;
		private readonly string _objectTypesToken;

		public ObjectenMedewerkerClient(Uri objectenBaseUri, string objectenToken, Uri objectTypesBaseUri, string objectTypesToken)
		{
			_httpClient = new HttpClient();
			_objectenBaseUri = objectenBaseUri;
			_objectenToken = objectenToken;
			_objectTypesBaseUri = objectTypesBaseUri;
			_objectTypesToken = objectTypesToken;
		}

		public async IAsyncEnumerable<KissEnvelope> Get([EnumeratorCancellation] CancellationToken token)
		{
			var type = await GetMedewerkerObjectType(token);
			
			if (string.IsNullOrWhiteSpace(type)) throw new Exception("Kan objecttype 'Medewerker' niet vinden");
			
			var query = new QueryBuilder
			{
				{ "type", type }
			};
			
			var url = $"{_objectenBaseUri}api/v2/objects{query.ToQueryString()}";
			
			await foreach (var item in GetMedewerkers(url, token))
			{
				yield return item;
			}
		}

		private Task<string?> GetMedewerkerObjectType(CancellationToken token) => GetMedewerkerObjectType(_objectTypesBaseUri + "api/v2/objecttypes", token);

		private async Task<string?> GetMedewerkerObjectType(string url, CancellationToken token)
		{
			string? next = null;
			string? result = null;

			using (var message = new HttpRequestMessage(HttpMethod.Get, url))
			{
				message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", _objectTypesToken);
				using var response = await _httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, token);
				if(!response.IsSuccessStatusCode)
				{
					var str = await response.Content.ReadAsStringAsync(token);
				}
				response.EnsureSuccessStatusCode();
				await using var stream = await response.Content.ReadAsStreamAsync(token);
				using var jsonDoc = await JsonDocument.ParseAsync(stream, cancellationToken: token);

				if (!jsonDoc.TryParseZgwPagination(out var pagination))
				{
					return result;
				}

				next = pagination.Next;

				foreach (var objectType in pagination.Records)
				{
					if (objectType.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String && (name.ValueEquals("Medewerker") || name.ValueEquals("medewerker")) &&
						objectType.TryGetProperty("url", out var objectUrl) && objectUrl.ValueKind == JsonValueKind.String)
					{
						result = objectUrl.GetString();
						break;
					}
				}
			};

			if (string.IsNullOrWhiteSpace(result) && !string.IsNullOrWhiteSpace(next)) return await GetMedewerkerObjectType(next, token);

			return result;
		}

		private async IAsyncEnumerable<KissEnvelope> GetMedewerkers(string url, [EnumeratorCancellation] CancellationToken token)
		{
			string? next;

			using (var message = new HttpRequestMessage(HttpMethod.Get, url))
			{
				message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", _objectenToken);
				using var response = await _httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, token);
				response.EnsureSuccessStatusCode();
				await using var stream = await response.Content.ReadAsStreamAsync(token);
				using var jsonDoc = await JsonDocument.ParseAsync(stream, cancellationToken: token);

				if (!jsonDoc.TryParseZgwPagination(out var pagination))
				{
					yield break;
				}

				next = pagination.Next;

				foreach (var medewerker in pagination.Records)
				{
					if (!medewerker.TryGetProperty("record", out var record) || record.ValueKind != JsonValueKind.Object ||
						!record.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object ||
						!data.TryGetProperty("id", out var idProp) || idProp.ValueKind != JsonValueKind.String)
					{
						continue;
					}

					data.TryGetProperty("contact", out var contact);
					var title = string.Join(' ', GetStringValues(contact, s_nameProps));
					var objectMeta = string.Join(' ', GetStringValues(data, s_metaProps));

					yield return new KissEnvelope(data, title, objectMeta, $"kennisartikel_{idProp.GetString()}");
				}
			}

			if (!string.IsNullOrWhiteSpace(next))
			{
				await foreach (var el in GetMedewerkers(next, token))
				{
					yield return el;
				}
			}
		}

		private static IEnumerable<string> GetStringValues(JsonElement element, string[] propNames)
		{
			if (element.ValueKind != JsonValueKind.Object) yield break;
			foreach (var item in propNames)
			{
				if (element.TryGetProperty(item, out var value) && value.ValueKind == JsonValueKind.String)
				{
					var str = value.GetString();
					if (!string.IsNullOrWhiteSpace(str)) yield return str;
				}
			}
		}

		public void Dispose() => _httpClient?.Dispose();
	}
}
