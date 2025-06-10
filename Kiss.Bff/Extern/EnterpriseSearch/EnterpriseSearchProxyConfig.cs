using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using Duende.IdentityModel.Client;
using Kiss.Bff;
using Yarp.ReverseProxy.Transforms;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EnterpriseSearchExtensions
    {
        public static IServiceCollection AddEnterpriseSearch(this IServiceCollection services, string baseUrl, string apiKey, string enterpriseSearchExplainPattern)
        {
            services.AddSingleton<IKissProxyRoute>(new EnterpriseSearchProxyConfig(baseUrl, apiKey, enterpriseSearchExplainPattern));
            return services;
        }
    }

    public class EnterpriseSearchProxyConfig : IKissProxyRoute
    {
        public const string ROUTE = "enterprisesearch";

        private readonly string _apiKey;

        private readonly Regex _regexEnterpriseSearchExplain;

        public EnterpriseSearchProxyConfig(string destination, string apiKey,string enterpriseSearchExplainPattern)
        {
            Destination = destination;
            _apiKey = apiKey;

            _regexEnterpriseSearchExplain = new Regex(enterpriseSearchExplainPattern);
        }

        public string Route => ROUTE;

        public string Destination { get; }

        public ValueTask ApplyRequestTransform(RequestTransformContext context)
        {
            var path = context.HttpContext.Request.Path.ToString();
            var method = context.HttpContext.Request.Method;

            var isAllowedEnterpriseSearch = _regexEnterpriseSearchExplain.IsMatch(path);

            if (method == "POST" &&  isAllowedEnterpriseSearch)
            {
                context.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            }
            else
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;

                if (context.HttpContext.Response.Body.CanWrite)
                {
                    var responseBytes = Encoding.UTF8.GetBytes("Path is not allowed");
                    context.HttpContext.Response.Body.Write(responseBytes, 0, responseBytes.Length);
                }
            }
            return new ValueTask();
        }
    }
}
