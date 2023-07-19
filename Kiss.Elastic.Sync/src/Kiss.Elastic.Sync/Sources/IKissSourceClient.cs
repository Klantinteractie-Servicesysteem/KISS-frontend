using Kiss.Elastic.Sync.Mapping;

namespace Kiss.Elastic.Sync.Sources
{
    public interface IKissSourceClient : IDisposable
    {
        IAsyncEnumerable<KissEnvelope> Get(CancellationToken token);
        string Source { get; }
        CompletionMapping Mapping { get; }
    }
}
