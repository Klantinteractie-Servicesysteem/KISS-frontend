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
            await foreach (var typeUrl in _objectTypesClient.GetObjectTypeUrls("VAC", token))
            {
                await foreach (var item in _objectenClient.GetObjecten(typeUrl, token))
                {
                    var id = $"vag_{item.Id.GetString()}";
                    var title = item.Data.TryGetProperty("vraag", out var titleProp) && titleProp.ValueKind == JsonValueKind.String
                        ? titleProp.GetString()
                        : "";
                    yield return new KissEnvelope(item.Data, title, null, id);
                }
            }
        }
    }
}
