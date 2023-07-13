using Kiss.Elastic.Sync;
using Kiss.Elastic.Sync.Sources;
var source = args.FirstOrDefault()?.ToLower();

using IKissSourceClient sourceClient = SourceFactory.CreateClient(source, out source);
Console.WriteLine("Start syncing source " + source);

using var elasticClient = ElasticBulkClient.Create();
using var cancelSource = new CancellationTokenSource();
AppDomain.CurrentDomain.ProcessExit += (_, _) => cancelSource.CancelSafely();

var records = sourceClient.Get(cancelSource.Token);
await elasticClient.IndexBulk(records, source, cancelSource.Token);
Console.WriteLine("Finished indexing source " + source);