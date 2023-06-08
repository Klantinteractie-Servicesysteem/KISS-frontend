namespace Kiss.Elastic.Sync.Sources
{
	public interface IKissSourceClient : IDisposable
	{
		IAsyncEnumerable<KissEnvelope> Get(CancellationToken token);
		string Type { get; }
	}
}
