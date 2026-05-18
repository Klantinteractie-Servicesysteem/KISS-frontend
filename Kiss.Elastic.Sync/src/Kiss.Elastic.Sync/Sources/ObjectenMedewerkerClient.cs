using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Kiss.Elastic.Sync.Sources
{
    internal sealed class ObjectenMedewerkerClient : IKissSourceClient
    {
        private static readonly string[] s_nameProps = new[] { "voornaam", "voorvoegselAchternaam", "achternaam" };
        private static readonly string[] s_metaProps = new[] { "function", "department", "skills" };
        private readonly ObjectenClient _objectenClient;
        private readonly string _objecttypeUrl;

        public string Source => "Smoelenboek";

        public IReadOnlyList<string> CompletionFields { get; } = new[]
        {
            "afdelingen.afdelingnaam",
            "groepen.groepsnaam",
            "functie",
            "skills",
            "achternaam",
            "voornaam",
            "voorvoegselAchternaam",
            "identificatie",
            "emails.email",
            "telefoonnummers.telefoonnummer"
        };

        public ObjectenMedewerkerClient(ObjectenClient objectenClient, string objecttypeUrl)
        {
            _objectenClient = objectenClient;
            _objecttypeUrl = objecttypeUrl;
        }

        public async IAsyncEnumerable<KissEnvelope> Get([EnumeratorCancellation] CancellationToken token)
        {
            await foreach (var item in _objectenClient.GetObjecten(_objecttypeUrl, token))
            {
                var data = item.Data;
                if (!data.TryGetProperty("identificatie", out var idProp) || idProp.ValueKind != JsonValueKind.String)
                    continue;

                var title = string.Join(' ', GetStringValues(data, s_nameProps));

                var objectMeta = data.TryGetProperty("functie", out var functieProp) && functieProp.ValueKind == JsonValueKind.String
                    ? functieProp.GetString()
                    : null;

                yield return new KissEnvelope(data, title, objectMeta, $"smoelenboek_{idProp.GetString()}");
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
        }
    }
}
