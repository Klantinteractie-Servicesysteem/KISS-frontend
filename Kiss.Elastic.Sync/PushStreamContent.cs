using System.Net;

namespace Kiss.Elastic.Sync
{
    public class PushStreamContent : HttpContent
    {
        private readonly Func<Stream, Task> _handler;

        public PushStreamContent(Func<Stream, Task> handler)
        {
            _handler = handler;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            return _handler(stream);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}
