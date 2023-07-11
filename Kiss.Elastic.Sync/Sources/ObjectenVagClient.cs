using System.Runtime.CompilerServices;
using System.Text.Json;
using Kiss.Elastic.Sync.Objecten;

namespace Kiss.Elastic.Sync.Sources
{
    internal sealed class ObjectenVagClient : IKissSourceClient
    {
        private readonly ObjectenClient _objectenClient;
        private readonly ObjectTypesClient _objectTypesClient;

        public ObjectenVagClient(ObjectenClient objectenClient, ObjectTypesClient objectTypesClient)
        {
            _objectenClient = objectenClient;
            _objectTypesClient = objectTypesClient;
        }

        public void Dispose()
        {
            _objectenClient.Dispose();
            _objectTypesClient.Dispose();
        }

        public async IAsyncEnumerable<KissEnvelope> Get([EnumeratorCancellation] CancellationToken token)
        {
            await foreach (var typeUrl in _objectTypesClient.GetObjectTypeUrls("vac", token))
            {
                await foreach (var item in _objectenClient.GetObjecten(typeUrl, token))
                {
                    if (!item.TryGetProperty("identifier", out var idProp) || idProp.ValueKind != JsonValueKind.String)
                        continue;

                    var id = $"vag_{idProp.GetString()}";
                    var title = item.TryGetProperty("title", out var titleProp) && titleProp.ValueKind == JsonValueKind.String
                        ? titleProp.GetString()
                        : "";
                    yield return new KissEnvelope(item, title, "", id);
                }
            }
        }
    }
}
