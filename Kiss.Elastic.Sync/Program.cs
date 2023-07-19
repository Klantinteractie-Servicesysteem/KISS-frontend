using Kiss.Elastic.Sync;
using Kiss.Elastic.Sync.Sources;

using var cancelSource = new CancellationTokenSource();
AppDomain.CurrentDomain.ProcessExit += (_, _) => cancelSource.CancelSafely();

if (args.Length == 2 && args[0] == "engine")
{
    var engine = args[1];
    using var updater = EngineMappingUpdater.Create();
    Console.WriteLine("Start updating engine");
    await updater.UpdateMappingForEngine("deventer-engine", cancelSource.Token);
    Console.WriteLine("");
    Console.WriteLine("Finished updating engine");
    return;
}

var source = args.FirstOrDefault();

using var elasticClient = ElasticBulkClient.Create();
using var enterpriseClient = ElasticEnterpriseSearchClient.Create();
using var sourceClient = SourceFactory.CreateClient(source);
Console.WriteLine("Start syncing source " + sourceClient.Source);

var records = sourceClient.Get(cancelSource.Token);
var indexName = await elasticClient.IndexBulk(records, sourceClient.Source, sourceClient.Mapping, cancelSource.Token);
await enterpriseClient.AddEngineAsync(indexName, cancelSource.Token);
Console.WriteLine();
Console.WriteLine("Finished indexing source " + sourceClient.Source);
