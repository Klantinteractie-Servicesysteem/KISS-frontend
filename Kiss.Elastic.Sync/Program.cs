using Kiss.Elastic.Sync;
using Kiss.Elastic.Sync.Sources;

using var cancelSource = new CancellationTokenSource();
AppDomain.CurrentDomain.ProcessExit += (_, _) => cancelSource.CancelSafely();

using var elasticClient = ElasticBulkClient.Create();

using var sourceClient = SourceFactory.CreateClient(args);
Console.WriteLine("Start syncing source " + sourceClient.Source);
var indexName = Helpers.GenerateValidIndexName(sourceClient.Source);

var records = sourceClient.Get(cancelSource.Token);
await elasticClient.IndexBulk(records, sourceClient.Source, indexName, sourceClient.CompletionFields, cancelSource.Token);
Console.WriteLine("Finished indexing source " + sourceClient.Source);
