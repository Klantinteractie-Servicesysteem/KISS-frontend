
namespace Kiss.Elastic.Sync.Sources
{
    public interface IKissSourceClient : IDisposable
    {
        IAsyncEnumerable<KissEnvelope> Get(CancellationToken token);
        string Source { get; }
        IReadOnlyList<string> CompletionFields { get; }
    }
}
