using System.Text.Json;
using System.Text.Json.Nodes;
using Kiss.Bff.Extern.ElasticSearch;

public class ElasticsearchService
{
    public readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the ElasticsearchService
    /// </summary>
    /// <param name="httpClient">HTTP client configured for Elasticsearch communication</param>
    public ElasticsearchService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Executes an Elasticsearch search request with request/response transformations
    /// </summary>
    /// <param name="url">The Elasticsearch endpoint URL</param>
    /// <param name="elasticQuery">The query object to send to Elasticsearch</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Transformed Elasticsearch response</returns>
    public async Task<ElasticResponse> Search(string url, JsonObject elasticQuery, CancellationToken cancellationToken)
    {
        ApplyRequestTransform(elasticQuery);
        var response = await _httpClient.PostAsJsonAsync(url, elasticQuery, cancellationToken);

        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadFromJsonAsync<ElasticResponse>(cancellationToken);
        ApplyResponseTransform(responseBody);

        return responseBody;
    }


    /// <summary>
    /// Transform the request query based on user role
    /// Applies field exclusions for Kennisbank users
    /// </summary>
    private void ApplyRequestTransform(JsonObject query)
    {
        // TODO: implement code to remove any fields that are not allowed for the role the current user has.
    }

    /// <summary>
    /// Transform the response body if needed
    /// Currently passes through unchanged, but can be extended for response filtering
    /// </summary>
    private ElasticResponse ApplyResponseTransform(ElasticResponse? responseBody)
    {
        // TODO: implement code to remove any fields that are not allowed for the role the current user has.
        // For now, just pass through
        return responseBody;
    }
}