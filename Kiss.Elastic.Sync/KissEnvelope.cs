using System.Text.Json;

namespace Kiss.Elastic.Sync
{
    public readonly record struct KissEnvelope(in JsonElement Object, in string? Title, in string? ObjectMeta, in string Id)
    {
        public void WriteTo(Utf8JsonWriter jsonWriter, string bron)
        {
            jsonWriter.WriteStartObject();

            jsonWriter.WriteString("id", Id);

            jsonWriter.WriteString("title", Title);

            jsonWriter.WriteString("object_meta", ObjectMeta);

            jsonWriter.WriteString("object_bron", bron);

            jsonWriter.WritePropertyName("object");
            Object.WriteTo(jsonWriter);

            jsonWriter.WriteEndObject();
        }
    }
}
