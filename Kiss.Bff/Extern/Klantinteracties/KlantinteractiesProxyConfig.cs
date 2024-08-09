using Kiss.Bff.Afdelingen;
using Kiss.Bff.ZaakGerichtWerken;
using System.Net.Http.Headers;
using Yarp.ReverseProxy.Transforms;

namespace Kiss.Bff.Extern.Klantinteracties
{
    public static class KlantinteractiesExtensions
    {
        public static IServiceCollection AddKlantinteracties(this IServiceCollection services,
            string destination,
            string secret) => services.AddSingleton<IKissProxyRoute>(new KlantinteractiesProxyConfig(destination, secret));
    }

    public class KlantinteractiesProxyConfig: IKissProxyRoute
    {
        private readonly AuthenticationHeaderProvider _authProvider;

        public KlantinteractiesProxyConfig(string destination, string secret)
        {
            Destination = destination;
            _authProvider = new AuthenticationHeaderProvider(secret, "", "");
        }

        public string Route => "klantinteracties";

        public string Destination { get; }

        public ValueTask ApplyRequestTransform(RequestTransformContext context)
        {
            _authProvider.ApplyAuthorizationHeader(context.ProxyRequest.Headers, context.HttpContext.User);
            return new();
        }
    }
}
