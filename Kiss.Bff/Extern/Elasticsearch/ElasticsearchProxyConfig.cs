using Duende.IdentityModel.Client;
using Kiss.Bff;
using Yarp.ReverseProxy.Transforms;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ElasticsearchExtensions
    {


        public static IServiceCollection AddElasticsearch(this IServiceCollection services, string baseUrl, string username, string password)
        {
            services.AddSingleton<IKissProxyRoute>(new ElasticsearchProxyConfig(baseUrl, username, password));
            return services;
        }
    }

    public class ElasticsearchProxyConfig : IKissProxyRoute
    {
        public const string ROUTE = "elasticsearch";

        private readonly string _username;
        private readonly string _password;



        public ElasticsearchProxyConfig(string destination, string username, string password)
        {
            Destination = destination;
            _username = username;
            _password = password;
        }

        public string Route => ROUTE;

        public string Destination { get; }


        public ValueTask ApplyRequestTransform(RequestTransformContext context)

        {
            context.ProxyRequest.SetBasicAuthentication(_username, _password);

            return new ValueTask();
        }
    }
}

