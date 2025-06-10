using System.Text;
using System.Text.RegularExpressions;
using Duende.IdentityModel.Client;
using Kiss.Bff;
using Yarp.ReverseProxy.Transforms;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ElasticsearchExtensions
    {


        public static IServiceCollection AddElasticsearch(this IServiceCollection services, string baseUrl, string username, string password, string elasticsearchSearchPattern)
        {
            services.AddSingleton<IKissProxyRoute>(new ElasticsearchProxyConfig(baseUrl, username, password, elasticsearchSearchPattern));
            return services;
        }
    }

    public class ElasticsearchProxyConfig : IKissProxyRoute
    {
        public const string ROUTE = "elasticsearch";

        private readonly string _username;
        private readonly string _password;
         
        private readonly Regex _regexElasticsearchSearch;




        public ElasticsearchProxyConfig(string destination, string username, string password, string elasticsearchSearchPattern)
        {
            Destination = destination;
            _username = username;
            _password = password;
            _regexElasticsearchSearch = new Regex(elasticsearchSearchPattern);
        }

        public string Route => ROUTE;

        public string Destination { get; }


        public ValueTask ApplyRequestTransform(RequestTransformContext context)
        {
            var path = context.HttpContext.Request.Path.ToString();
            var method = context.HttpContext.Request.Method;

            var isAllowedElasticsearch = _regexElasticsearchSearch.IsMatch(path);

            if (method == "POST" && isAllowedElasticsearch )
            {
                context.ProxyRequest.SetBasicAuthentication(_username, _password);
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
