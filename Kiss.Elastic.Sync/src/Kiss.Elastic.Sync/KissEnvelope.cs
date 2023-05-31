using System.Text.Json;

namespace Kiss.Elastic.Sync
{
	public readonly record struct KissEnvelope(in JsonElement Object, in JsonElement Title, in JsonElement ObjectMeta, in string Id)
	{
		public void WriteTo(Utf8JsonWriter jsonWriter, string bron)
		{
			jsonWriter.WriteStartObject();

			jsonWriter.WriteString("id", Id);

			if (Title.ValueKind == JsonValueKind.String)
			{
				jsonWriter.WritePropertyName("title");
				Title.WriteTo(jsonWriter);
			}

			if (ObjectMeta.ValueKind == JsonValueKind.String)
			{
				jsonWriter.WritePropertyName("object_meta");
				ObjectMeta.WriteTo(jsonWriter);
			}

			jsonWriter.WriteString("object_bron", bron);

			jsonWriter.WritePropertyName("object");
			Object.WriteTo(jsonWriter);

			jsonWriter.WriteEndObject();
		}
	}
}
