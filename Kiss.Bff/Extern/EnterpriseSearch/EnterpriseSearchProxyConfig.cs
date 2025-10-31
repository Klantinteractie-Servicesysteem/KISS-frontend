using System.Net.Http.Headers;
using Kiss.Bff;
using Kiss;
using Microsoft.AspNetCore.Authorization;
using Yarp.ReverseProxy.Transforms;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EnterpriseSearchExtensions
    {
        public static IServiceCollection AddEnterpriseSearch(this IServiceCollection services, string baseUrl, string apiKey)
        {
            services.AddSingleton<IKissProxyRoute>(new EnterpriseSearchProxyConfig(baseUrl, apiKey));
            return services;
        }
    }

    public class EnterpriseSearchProxyConfig : IKissProxyRoute
    {
        public const string ROUTE = "enterprisesearch";

        private readonly string _apiKey;

        public EnterpriseSearchProxyConfig(string destination, string apiKey)
        {
            Destination = destination;
            _apiKey = apiKey;

        }

        public string Route => ROUTE;

        public string Destination { get; }

        public ValueTask ApplyRequestTransform(RequestTransformContext context)
        {

            context.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            return new ValueTask();
        }
    }
}
