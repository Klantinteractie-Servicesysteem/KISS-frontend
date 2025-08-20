using System.Net.Http.Headers;
using Yarp.ReverseProxy.Transforms;

namespace Kiss.Bff.Extern.Logboek
{
    public static class LogboekExtensions
    {
        public static IServiceCollection AddLogboekProxy(this IServiceCollection services, string destination, string token, string objectTypeUrl, string typeVersion)
        {
            return services.AddSingleton<IKissProxyRoute>(new LogboekProxyConfig(destination, token, objectTypeUrl, typeVersion));
        }
    }

    public class LogboekProxyConfig : IKissProxyRoute
    {
        private readonly string _token;

        public LogboekProxyConfig(string destination, string token, string objectTypeUrl, string typeVersion)
        {
            Destination = destination;
            ObjectTypeUrl = objectTypeUrl;
            TypeVersion = typeVersion ?? "1";
            _token = token;
        }

        public string Route => "logboek";

        public string Destination { get; }
        public string ObjectTypeUrl { get; }
        public string TypeVersion { get; }

        public ValueTask ApplyRequestTransform(RequestTransformContext context)
        {
            var request = context.HttpContext.Request;

            if (request.Method != HttpMethods.Get)
            {
                // only allow get, the client will by definition never create a logbook entry
                context.HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                return new();
            }

            ApplyHeaders(context.ProxyRequest.Headers);

            var isObjectsEndpoint = request.Path.Value?.AsSpan().TrimEnd('/').EndsWith("objects") ?? false;
            if (isObjectsEndpoint)
            {
                context.Query.Collection["type"] = new(ObjectTypeUrl);
                context.Query.Collection["typeVersion"] = new(TypeVersion);
            }

            return new();
        }

        public void ApplyHeaders(HttpRequestHeaders headers)
        {
            headers.Authorization = new AuthenticationHeaderValue("Token", _token);
            headers.Add("Content-Crs", "EPSG:4326");
        }
    }
}
