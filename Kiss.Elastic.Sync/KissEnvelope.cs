using System.Text.Json;

namespace Kiss.Elastic.Sync
{
    public readonly record struct KissEnvelope(in JsonElement Object, in string? Title, in string? ObjectMeta, in string Id, in string? Url = null)
    {
        public void WriteTo(Utf8JsonWriter jsonWriter, string bron)
        {
            jsonWriter.WriteStartObject();

            jsonWriter.WriteString("title", Title);

            jsonWriter.WriteString("object_meta", ObjectMeta);
            jsonWriter.WriteString("object_bron", bron);

            if (!string.IsNullOrWhiteSpace(Url))
            {
                jsonWriter.WriteString("url", Url);
            }

            jsonWriter.WritePropertyName(bron);
            Object.WriteTo(jsonWriter);

            jsonWriter.WriteEndObject();
        }
    }
}
