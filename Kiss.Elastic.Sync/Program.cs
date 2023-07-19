using Kiss.Elastic.Sync;
using Kiss.Elastic.Sync.Sources;

#if DEBUG
//if (!args.Any())
//{
//    args = new[] { "domain", "https://www.deventer.nl" };
//}
#endif
using var cancelSource = new CancellationTokenSource();
AppDomain.CurrentDomain.ProcessExit += (_, _) => cancelSource.CancelSafely();
if (args.Length == 2 && args[0] == "domain")
{
    var url = args[1];
    if(!Uri.TryCreate(url, UriKind.Absolute, out var uri))
    {
        throw new Exception();
    }
    using var enterprise = ElasticEnterpriseSearchClient.Create();
    using var updater = EngineMappingUpdater.Create();
    Console.WriteLine("start adding domain");
    await enterprise.AddDomain(uri, cancelSource.Token);
    Console.WriteLine("finished adding domain");
    Console.WriteLine("Start updating engine");
    await updater.UpdateMappingForCrawlEngine(cancelSource.Token);
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
await enterpriseClient.AddIndexEngineAsync(indexName, cancelSource.Token);
Console.WriteLine("Finished indexing source " + sourceClient.Source);
