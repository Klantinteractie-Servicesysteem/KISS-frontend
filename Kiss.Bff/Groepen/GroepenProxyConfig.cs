using System.Net.Http.Headers;
using Yarp.ReverseProxy.Transforms;

namespace Kiss.Bff.Groepen
{
    public static class GroepenExtensions
    {
        public static IServiceCollection AddGroepenProxy(this IServiceCollection services, string destination, string token, string objectTypeUrl)
            => services.AddSingleton<IKissProxyRoute>(new GroepenProxyConfig(destination, token, objectTypeUrl));
    }

    public class GroepenProxyConfig : IKissProxyRoute
    {
        private readonly AuthenticationHeaderValue _authHeader;

        public GroepenProxyConfig(string destination, string token, string objectTypeUrl)
        {
            Destination = destination;
            ObjectTypeUrl = objectTypeUrl;
            _authHeader = new AuthenticationHeaderValue("Token", token);
        }

        public string Route => "groepen";

        public string Destination { get; }
        public string ObjectTypeUrl { get; }


        public ValueTask ApplyRequestTransform(RequestTransformContext context)
        {
            ApplyHeaders(context.ProxyRequest.Headers);
            var request = context.HttpContext.Request;
            var isObjectsEndpoint = request.Path.Value?.AsSpan().TrimEnd('/').EndsWith("objects") ?? false;
            if (request.Method == HttpMethods.Get && isObjectsEndpoint)
            {
                context.Query.Collection["type"] = new(ObjectTypeUrl);
            }
            return new();
        }

        public void ApplyHeaders(HttpRequestHeaders headers)
        {
            headers.Authorization = _authHeader;
            headers.Add("Content-Crs", "EPSG:4326");
        }
    }
}
