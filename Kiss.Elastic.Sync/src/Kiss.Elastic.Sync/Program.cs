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

using var elasticClient = ElasticBulkClient.Create();
using var enterpriseClient = ElasticEnterpriseSearchClient.Create();

if (args.Length == 2 && args[0] == "domain")
{
    var url = args[1];
    if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
    {
        throw new Exception();
    }
    Console.WriteLine("start adding domain");
    await enterpriseClient.AddDomain(uri, cancelSource.Token);
    await elasticClient.UpdateMappingForCrawlEngine(cancelSource.Token);
    await enterpriseClient.CrawlDomain(uri, cancelSource.Token);
    Console.WriteLine("Finished adding domain");
    return;
}

using var sourceClient = SourceFactory.CreateClient(args);
Console.WriteLine("Start syncing source " + sourceClient.Source);
var indexName = Helpers.GenerateValidIndexName(sourceClient.Source);

var records = sourceClient.Get(cancelSource.Token);
await elasticClient.IndexBulk(records, sourceClient.Source, indexName, sourceClient.CompletionFields, cancelSource.Token);
await enterpriseClient.AddIndexEngineAsync(indexName, cancelSource.Token);
Console.WriteLine("Finished indexing source " + sourceClient.Source);
