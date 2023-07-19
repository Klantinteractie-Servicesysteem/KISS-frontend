using System.Runtime.CompilerServices;
using System.Text.Json;
using Kiss.Elastic.Sync.Mapping;
using Kiss.Elastic.Sync.Objecten;

namespace Kiss.Elastic.Sync.Sources
{
    internal sealed class ObjectenMedewerkerClient : IKissSourceClient
    {
        private static readonly string[] s_nameProps = new[] { "voornaam", "voorvoegselAchternaam", "achternaam" };
        private static readonly string[] s_metaProps = new[] { "function", "department", "skills" };
        private readonly ObjectenClient _objectenClient;
        private readonly ObjectTypesClient _objectTypesClient;

        public string Source => "Smoelenboek";

        public CompletionMapping Mapping { get; } = new()
        {
            ["department"] = true,
            ["description"] = true,
            ["function"] = true,
            ["skills"] = true,
            ["contact"] = new()
            {
                ["achternaam"] = true,
                ["voornaam"] = true,
                ["voorvoegselAchternaam"] = true,
                ["identificatie"] = true,

                ["emails"] = new()
                {
                    ["email"] = true
                },
                ["telefoonnummers"] = new()
                {
                    ["telefoonnummer"] = true
                }
            }
        };

        public ObjectenMedewerkerClient(ObjectenClient objectenClient, ObjectTypesClient objectTypesClient)
        {
            _objectenClient = objectenClient;
            _objectTypesClient = objectTypesClient;
        }

        public async IAsyncEnumerable<KissEnvelope> Get([EnumeratorCancellation] CancellationToken token)
        {
            await foreach (var type in _objectTypesClient.GetObjectTypeUrls("Medewerker", token))
            {
                await foreach (var item in _objectenClient.GetObjecten(type, token))
                {
                    var data = item.Data;
                    if (!data.TryGetProperty("id", out var idProp) || idProp.ValueKind != JsonValueKind.String)
                        continue;

                    data.TryGetProperty("contact", out var contact);
                    var title = string.Join(' ', GetStringValues(contact, s_nameProps));
                    var objectMeta = string.Join(' ', GetStringValues(data, s_metaProps));

                    yield return new KissEnvelope(data, title, objectMeta, $"smoelenboek_{idProp.GetString()}");
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

        public void Dispose()
        {
            _objectenClient.Dispose();
            _objectTypesClient.Dispose();
        }
    }
}
