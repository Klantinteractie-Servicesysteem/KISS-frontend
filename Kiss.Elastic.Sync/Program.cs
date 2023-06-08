using Kiss.Elastic.Sync;
using Kiss.Elastic.Sync.Sources;


var elasticBaseUrl = GetEnvironmentVariable("ENTERPRISE_SEARCH_BASE_URL");
var elasticApiKey = GetEnvironmentVariable("ENTERPRISE_SEARCH_PRIVATE_API_KEY");
var elasticEngine = GetEnvironmentVariable("ENTERPRISE_SEARCH_ENGINE");

if (!Uri.TryCreate(elasticBaseUrl, UriKind.Absolute, out var elasticBaseUri))
{
    Console.Write("elastic base url is niet valide: ");
    Console.WriteLine(elasticBaseUrl);
    return;
}

var source = args.FirstOrDefault()?.ToLower();

using var consoleStream = Console.OpenStandardOutput();
using IKissSourceClient sourceClient = source switch 
{ 
    "medewerkers" => GetMedewerkerClient(),
    _=> GetProductClient()
};
using var elasticClient = new ElasticEnterpriseSearchClient(elasticBaseUri, elasticApiKey);
using var cancelSource = new CancellationTokenSource();
AppDomain.CurrentDomain.ProcessExit += (_, _) => cancelSource.CancelSafely();

var records = sourceClient.Get(cancelSource.Token);
await elasticClient.IndexDocumentsAsync(records, elasticEngine, sourceClient.Type, cancelSource.Token);

static string GetEnvironmentVariable(string name) => Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process) ?? throw new Exception("missing environment variable: " + name);

static SdgProductClient GetProductClient()
{
	var sdgBaseUrl = GetEnvironmentVariable("SDG_BASE_URL");
	var sdgApiKey = GetEnvironmentVariable("SDG_API_KEY");

	if (!Uri.TryCreate(sdgBaseUrl, UriKind.Absolute, out var sdgBaseUri))
	{
		throw new Exception("sdg base url is niet valide: " + sdgBaseUrl);
	}
	return new SdgProductClient(sdgBaseUri, sdgApiKey);
}

static ObjectenMedewerkerClient GetMedewerkerClient()
{
	var objectenBaseUrl = GetEnvironmentVariable("OBJECTEN_BASE_URL");
	var objectTypesBaseUrl = GetEnvironmentVariable("OBJECTTYPES_BASE_URL");
	var objectenToken = GetEnvironmentVariable("OBJECTEN_TOKEN");
	var objectTypesToken = GetEnvironmentVariable("OBJECTTYPES_TOKEN");

	if (!Uri.TryCreate(objectenBaseUrl, UriKind.Absolute, out var objectenBaseUri))
	{
		throw new Exception("objecten base url is niet valide: " + objectenBaseUrl);
	}
	if (!Uri.TryCreate(objectTypesBaseUrl, UriKind.Absolute, out var objectTypesBaseUri))
	{
		throw new Exception("objecttypes base url is niet valide: " + objectTypesBaseUrl);
	}
	return new ObjectenMedewerkerClient(objectenBaseUri, objectenToken, objectTypesBaseUri, objectTypesToken);
}