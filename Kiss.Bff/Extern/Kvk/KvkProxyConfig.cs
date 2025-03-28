using Kiss;
using Kiss.Bff;
using Yarp.ReverseProxy.Transforms;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class KvkExtensions
    {
        public static IServiceCollection AddKvk(this IServiceCollection services, string baseUrl, string? apiKey, string? userHeaderName, Dictionary<string, string>? customHeaders)
        {
            services.AddSingleton<IKissProxyRoute>(new KvkProxyConfig(baseUrl, apiKey, userHeaderName, customHeaders));
            return services;
        }
    }

    public sealed class KvkProxyConfig(
        string destination,
        string? apiKey,
        string? userHeaderName,
        Dictionary<string, string>? customHeaders) : IKissProxyRoute
    {
        public string Route => "kvk";
        public string Destination { get; } = destination;

        public ValueTask ApplyRequestTransform(RequestTransformContext context)
        {
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                context.ProxyRequest.Headers.Add("apikey", apiKey);
            }

            if (!string.IsNullOrWhiteSpace(userHeaderName))
            {
                context.ProxyRequest.Headers.Add(userHeaderName, context.HttpContext.User.GetUserName());
            }

            if (customHeaders != null)
            {
                foreach (var (key, value) in customHeaders)
                {
                    if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
                    {
                        context.ProxyRequest.Headers.Add(key, value);
                    }
                }
            }

            return new();
        }
    }
}
