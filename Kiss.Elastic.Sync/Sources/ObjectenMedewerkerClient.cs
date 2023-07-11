using System.Runtime.CompilerServices;
using System.Text.Json;
using Kiss.Elastic.Sync.Objecten;

namespace Kiss.Elastic.Sync.Sources
{
    internal sealed class ObjectenMedewerkerClient : IKissSourceClient
    {
        private static readonly string[] s_nameProps = new[] { "voornaam", "voorvoegselAchternaam", "achternaam" };
        private static readonly string[] s_metaProps = new[] { "function", "department", "skills" };
        private readonly ObjectenClient _objectenClient;
        private readonly ObjectTypesClient _objectTypesClient;

        public ObjectenMedewerkerClient(ObjectenClient objectenClient, ObjectTypesClient objectTypesClient)
        {
            _objectenClient = objectenClient;
            _objectTypesClient = objectTypesClient;
        }

        public async IAsyncEnumerable<KissEnvelope> Get([EnumeratorCancellation] CancellationToken token)
        {
            var typeCount = 0;

            await foreach (var type in _objectTypesClient.GetObjectTypes("medewerker", token))
            {
                await foreach (var item in _objectenClient.GetObjecten(type, token))
                {
                    if (!item.TryGetProperty("id", out var idProp) || idProp.ValueKind != JsonValueKind.String)
                        continue;

                    item.TryGetProperty("contact", out var contact);
                    var title = string.Join(' ', GetStringValues(contact, s_nameProps));
                    var objectMeta = string.Join(' ', GetStringValues(item, s_metaProps));

                    yield return new KissEnvelope(item, title, objectMeta, $"smoelenboek_{idProp.GetString()}");
                }
            }

            if (typeCount == 0)
            {
                throw new Exception("Kan objecttype 'Medewerker' niet vinden");
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
