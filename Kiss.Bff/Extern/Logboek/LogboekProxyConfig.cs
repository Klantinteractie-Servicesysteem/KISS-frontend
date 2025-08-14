using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Yarp.ReverseProxy.Transforms;

namespace Kiss.Bff.Extern.Logboek
{
    public static class LogboekExtensions
    {
        public static IServiceCollection AddLogboekProxy(this IServiceCollection services, string destination, string token, string objectTypeUrl, string typeVersion)
        {
            return services.AddSingleton(s =>
            {
                var authorizationService = s.GetRequiredService<IAuthorizationService>();
                var policyProvider = s.GetRequiredService<IAuthorizationPolicyProvider>();

                return new LogboekProxyConfig(destination, token, objectTypeUrl, typeVersion, authorizationService, policyProvider);
            })
                .AddSingleton<IKissProxyRoute>(s => s.GetRequiredService<LogboekProxyConfig>());
        }
    }

    public class LogboekProxyConfig : IKissProxyRoute
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IAuthorizationPolicyProvider _policyProvider;
        private readonly string _token;

        public LogboekProxyConfig(string destination, string token, string objectTypeUrl, string typeVersion,
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

        public string Route => "logboek";

        public string Destination { get; }
        public string ObjectTypeUrl { get; }
        public string TypeVersion { get; }

        public async ValueTask ApplyRequestTransform(RequestTransformContext context)
        {
            var policy = await _policyProvider.GetPolicyAsync(Policies.RedactiePolicy);
            if (policy == null)
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            var authResult = await _authorizationService.AuthorizeAsync(context.HttpContext.User, null, policy);
            if (!authResult.Succeeded)
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

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
