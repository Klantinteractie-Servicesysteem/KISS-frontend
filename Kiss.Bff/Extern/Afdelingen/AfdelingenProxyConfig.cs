using System.Net.Http.Headers;
using Kiss.Bff.Config.Permissions;
using Yarp.ReverseProxy.Transforms;

namespace Kiss.Bff.Afdelingen
{
    public static class AfdelingenExtensions
    {
        public static IServiceCollection AddAfdelingenProxy(this IServiceCollection services, string destination, string token, string objectTypeUrl, string clientId, string clientSecret)
            => services.AddSingleton<IKissProxyRoute>(new AfdelingenProxyConfig(destination, token, objectTypeUrl, clientId, clientSecret));
    }

    public class AfdelingenProxyConfig : IKissProxyRoute
    {
        private readonly AuthenticationHeaderProvider _authHeaderProvider;

        public AfdelingenProxyConfig(string destination, string token, string objectTypeUrl, string clientId, string clientSecret)
        {
            Destination = destination;
            ObjectTypeUrl = objectTypeUrl;

            _authHeaderProvider = new AuthenticationHeaderProvider(token, clientId, clientSecret);
        }

        public string Route => "afdelingen";

        public string Destination { get; }
        public string ObjectTypeUrl { get; }
        public RequirePermissionTo[]? RequirePermissions => [RequirePermissionTo.afdelingen];


        public ValueTask ApplyRequestTransform(RequestTransformContext context)
        {
            ApplyHeaders(context.ProxyRequest.Headers, context.HttpContext.User);
            var request = context.HttpContext.Request;
            var isObjectsEndpoint = request.Path.Value?.AsSpan().TrimEnd('/').EndsWith("objects") ?? false;
            if (request.Method == HttpMethods.Get && isObjectsEndpoint)
            {
                context.Query.Collection["type"] = new(ObjectTypeUrl);
            }
            return new();
        }

        public void ApplyHeaders(HttpRequestHeaders headers, System.Security.Claims.ClaimsPrincipal user)
        {
            _authHeaderProvider.ApplyAuthorizationHeader(headers, user);
            headers.Add("Content-Crs", "EPSG:4326");
        }
    }
}
