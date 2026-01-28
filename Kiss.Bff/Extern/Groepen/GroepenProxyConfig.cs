using System.Net.Http.Headers;
using Kiss.Bff.Config.Permissions;
using Yarp.ReverseProxy.Transforms;

namespace Kiss.Bff.Groepen
{
    public static class GroepenExtensions
    {
        public static IServiceCollection AddGroepenProxy(this IServiceCollection services, string destination, string token, string objectTypeUrl, string clientId, string clientSecret)
            => services.AddSingleton<IKissProxyRoute>(new GroepenProxyConfig(destination, token, objectTypeUrl, clientId, clientSecret));
    }

    public class GroepenProxyConfig : IKissProxyRoute
    {

        public GroepenProxyConfig(string destination, string token, string objectTypeUrl, string clientId, string clientSecret)
        {
            Destination = destination;
            ObjectTypeUrl = objectTypeUrl;
            _authHeaderProvider = new AuthenticationHeaderProvider(token, clientId, clientSecret);
        }

        public string Route => "groepen";

        public string Destination { get; }
        public string ObjectTypeUrl { get; }
        public RequirePermissionTo[]? RequirePermissions => [RequirePermissionTo.contactformulierengroepen];

        private readonly AuthenticationHeaderProvider _authHeaderProvider;

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
