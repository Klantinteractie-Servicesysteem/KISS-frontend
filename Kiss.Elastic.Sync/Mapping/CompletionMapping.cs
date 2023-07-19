using System.Text.Json;
using System.Text.Json.Nodes;

namespace Kiss.Elastic.Sync.Mapping
{
    public class CompletionMapping
    {
        private static readonly Lazy<JsonElement> s_defaultMapping = new(GetFieldMapping);

        private static JsonElement GetFieldMapping()
        {
            using var str = Helpers.GetEmbedded("field.json") ?? Stream.Null;
            using var doc = JsonDocument.Parse(str);
            return doc.RootElement.Clone();
        }

        private readonly Dictionary<string, CompletionMapping> _dict = new();

        public CompletionMapping this[string index]
        {
            get => _dict[index];
            set => _dict[index] = value;
        }

        private class Value : CompletionMapping
        {
            public Value(bool includeInCompletion)
            {
                IncludeInCompletion = includeInCompletion;
            }

            public bool IncludeInCompletion { get; }
        }

        public static implicit operator CompletionMapping(bool include) => new Value(include);

        public JsonObject ToJsonObject() => this is Value value
                ? JsonObject.Create(s_defaultMapping.Value) ?? new JsonObject()
                : new JsonObject
                {
                    ["properties"] = new JsonObject(_dict.Select(x => new KeyValuePair<string, JsonNode?>(x.Key, x.Value.ToJsonObject())))
                };
    }
}
