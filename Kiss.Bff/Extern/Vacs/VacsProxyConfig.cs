using System.Net.Http.Headers;
using Kiss.Bff.Config.Permissions;
using Microsoft.AspNetCore.Authorization;
using Yarp.ReverseProxy.Transforms;

namespace Kiss.Bff.Vacs
{
    public static class VacsExtensions
    {
        public static IServiceCollection AddVacsProxy(this IServiceCollection services, string destination, string token, string objectTypeUrl, string typeVersion)
        {
            return services.AddSingleton(s =>
            {
                var authorizationService = s.GetRequiredService<IAuthorizationService>();
                var policyProvider = s.GetRequiredService<IAuthorizationPolicyProvider>();

                return new VacsProxyConfig(destination, token, objectTypeUrl, typeVersion, authorizationService, policyProvider);
            })
                .AddSingleton<IKissProxyRoute>(s => s.GetRequiredService<VacsProxyConfig>());
        }
    }

    public class VacsProxyConfig : IKissProxyRoute
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IAuthorizationPolicyProvider _policyProvider;
        private readonly string _token;

        public VacsProxyConfig(string destination, string token, string objectTypeUrl, string typeVersion,
            IAuthorizationService authorizationService,
            IAuthorizationPolicyProvider policyProvider)
        {
            Destination = destination;
            ObjectTypeUrl = objectTypeUrl;
            TypeVersion = typeVersion ?? "1";
            _authorizationService = authorizationService;
            _policyProvider = policyProvider;
            _token = token;
        }

        public string Route => "vacs";

        public string Destination { get; }
        public string ObjectTypeUrl { get; }
        public string TypeVersion { get; }
        public RequirePermissionTo[]? RequirePermissions => [RequirePermissionTo.vacsbeheer];

        public async ValueTask ApplyRequestTransform(RequestTransformContext context)
        {
            ApplyHeaders(context.ProxyRequest.Headers);

            var request = context.HttpContext.Request;
            var isObjectsEndpoint = request.Path.Value?.AsSpan().TrimEnd('/').EndsWith("objects") ?? false;
            if (request.Method == HttpMethods.Get && isObjectsEndpoint)
            {
                context.Query.Collection["type"] = new(ObjectTypeUrl);
            }
        }

        public void ApplyHeaders(HttpRequestHeaders headers)
        {
            headers.Authorization = new AuthenticationHeaderValue("Token", _token);
            headers.Add("Content-Crs", "EPSG:4326");
        }
    }
}
