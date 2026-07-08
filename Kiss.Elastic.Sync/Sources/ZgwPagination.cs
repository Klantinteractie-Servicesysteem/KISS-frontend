using System.Text.Json;

namespace Kiss.Elastic.Sync.Sources
{
    public static class ZgwPagination
    {
        public static bool TryParseZgwPagination(this JsonDocument jsonDoc, out (IEnumerable<JsonElement> Records, string? Next) result)
        {
            if (jsonDoc.RootElement.ValueKind == JsonValueKind.Array)
            {
                result = (jsonDoc.RootElement.EnumerateArray(), null);
                return true;
            }

            if (!jsonDoc.RootElement.TryGetProperty("results", out var resultsProp) || resultsProp.ValueKind != JsonValueKind.Array)
            {
                result = default;
                return false;
            }

            var next = jsonDoc.RootElement.TryGetProperty("next", out var nextProp) && nextProp.ValueKind == JsonValueKind.String
                    ? nextProp.GetString()
                    : null;

            var records = resultsProp.EnumerateArray();

            result = (records, next);
            return true;
        }
    }
}
