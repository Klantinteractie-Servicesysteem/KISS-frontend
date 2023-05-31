using Kiss.Elastic.Sync;

var sdgBaseUrl = GetEnvironmentVariable("SDG_BASE_URL");
var sdgApiKey = GetEnvironmentVariable("SDG_API_KEY");
var elasticBaseUrl = GetEnvironmentVariable("ENTERPRISE_SEARCH_BASE_URL");
var elasticApiKey = GetEnvironmentVariable("ENTERPRISE_SEARCH_PRIVATE_API_KEY");
var elasticEngine = GetEnvironmentVariable("ENTERPRISE_SEARCH_ENGINE");

if (!Uri.TryCreate(sdgBaseUrl, UriKind.Absolute, out var sdgBaseUri))
{
    Console.Write("sdg base url is niet valide: ");
    Console.WriteLine(sdgBaseUrl);
    return;
}

if (!Uri.TryCreate(elasticBaseUrl, UriKind.Absolute, out var elasticBaseUri))
{
    Console.Write("elastic base url is niet valide: ");
    Console.WriteLine(elasticBaseUrl);
    return;
}

using var consoleStream = Console.OpenStandardOutput();
using var sdgClient = new SdgProductClient(sdgBaseUri, sdgApiKey);
using var elasticClient = new ElasticEnterpriseSearchClient(elasticBaseUri, elasticApiKey);
using var cancelSource = new CancellationTokenSource();
AppDomain.CurrentDomain.ProcessExit += (_, _) => cancelSource.CancelSafely();

var records = sdgClient.Get(cancelSource.Token);
await elasticClient.IndexDocumentsAsync(records, elasticEngine, "Kennisartikel", cancelSource.Token);

static string GetEnvironmentVariable(string name) => Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process) ?? throw new Exception("missing environment variable: " + name);
