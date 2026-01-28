using Kiss.Bff;
using Kiss.Bff.Config.Permissions;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Model;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace Kiss.Bff
{
    public interface IKissProxyRoute
    {
        string Route { get; }
        string Destination { get; }
        RequirePermissionTo[]? RequirePermissions => null;

        ValueTask ApplyRequestTransform(RequestTransformContext context);
    }

    public interface IKissHttpClientMiddleware
    {
        bool IsEnabled(string? clusterId);
        Task<HttpResponseMessage> SendAsync(SendRequestMessageAsync next, HttpRequestMessage request, CancellationToken cancellationToken);
    }

    public delegate Task<HttpResponseMessage> SendRequestMessageAsync(HttpRequestMessage request, CancellationToken cancellationToken);
}

namespace Microsoft.AspNetCore.Mvc
{
    public sealed class ProxyResult : IActionResult
    {
        public ProxyResult(Func<HttpRequestMessage> requestFactory)
        {
            RequestFactory = requestFactory;
        }

        public Func<HttpRequestMessage> RequestFactory { get; }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            var factory = context.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();

            var token = context.HttpContext.RequestAborted;
            var proxiedResponse = context.HttpContext.Response;

            using var client = factory.CreateClient("default");
            using var request = RequestFactory();

            if (request.Content is JsonContent)
            {
                await request.Content.LoadIntoBufferAsync();
            }

            using var responseMessage = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);

            proxiedResponse.StatusCode = (int)responseMessage.StatusCode;

            foreach (var item in responseMessage.Headers)
            {
                // deze header geeft aan of de content 'chunked' is. maar die waarde kunnen we niet overnemen,
                // die is namelijk afhankelijk van hoe we zelf hieronder de eigen response opbouwen.
                if (item.Key.Equals("transfer-encoding", StringComparison.OrdinalIgnoreCase)) continue;
                proxiedResponse.Headers[item.Key] = new(item.Value.ToArray());
            }

            foreach (var item in responseMessage.Content.Headers)
            {
                proxiedResponse.Headers[item.Key] = new(item.Value.ToArray());
            }

            await using var stream = await responseMessage.Content.ReadAsStreamAsync(token);
            await stream.CopyToAsync(proxiedResponse.Body, token);
        }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    public static class KissProxyExtensions
    {
        public static IServiceCollection AddKissProxy(this IServiceCollection services)
        {
            services.AddReverseProxy();
            services.AddSingleton<IProxyConfigProvider, ProxyConfigProvider>();
            services.AddSingleton<ITransformProvider, KissTransformProvider>();
            services.AddTransient<IForwarderHttpClientFactory, KissHttpClientFactory>();
            return services;
        }

        public static IEndpointRouteBuilder MapKissProxy(this IEndpointRouteBuilder builder)
        {
            builder.MapReverseProxy();
            return builder;
        }
    }

    public class KissTransformProvider : ITransformProvider
    {
        private readonly IKissProxyRoute[] _proxyRoutes;

        public KissTransformProvider(IEnumerable<IKissProxyRoute> proxyRoutes)
        {
            _proxyRoutes = proxyRoutes.ToArray();
        }

        public void Apply(TransformBuilderContext context)
        {
            var match = _proxyRoutes.FirstOrDefault(x => x.Route == context?.Cluster?.ClusterId);
            if (match != null)
            {
                context.AddRequestTransform(match.ApplyRequestTransform);
            }
        }

        public void ValidateCluster(TransformClusterValidationContext context)
        {
        }

        public void ValidateRoute(TransformRouteValidationContext context)
        {
        }
    }

    public class ProxyConfigProvider : IProxyConfigProvider
    {
        private readonly SimpleProxyConfig _config;

        public ProxyConfigProvider(IEnumerable<IKissProxyRoute> proxyRoutes)
        {
            var allRoutes = new List<RouteConfig>();
            var clusters = new List<ClusterConfig>();

            foreach (var proxyRoute in proxyRoutes)
            {
                // Create cluster for each proxy route
                clusters.Add(new ClusterConfig
                {
                    ClusterId = proxyRoute.Route,
                    Destinations = new Dictionary<string, DestinationConfig>
                    {
                        [proxyRoute.Route] = new DestinationConfig
                        {
                            Address = proxyRoute.Destination
                        }
                    },
                    // TODO: discuss if we need to get a valid certificate for Enterprise Search
                    HttpClient = proxyRoute.Route == EnterpriseSearchProxyConfig.ROUTE
                     ? new HttpClientConfig
                     {
                         DangerousAcceptAnyServerCertificate = true
                     }
                     : null
                });

                // TODO: Expose dedicated API controllers to handle Enterprise Search and Elasticsearch endpoints
                // Create specific routes based on the proxy route type
                if (proxyRoute.Route == EnterpriseSearchProxyConfig.ROUTE)
                {
                    // Only allow POST to search_explain endpoint
                    allRoutes.Add(new RouteConfig
                    {
                        RouteId = $"{proxyRoute.Route}-search-explain",
                        ClusterId = proxyRoute.Route,
                        Match = new RouteMatch
                        {
                            Path = "/api/enterprisesearch/api/as/v1/engines/{engine}/search_explain",
                            Methods = new[] { "POST" }
                        },
                        AuthorizationPolicy = Kiss.Policies.KcmOrKennisbankPolicy,
                        Transforms = new[]
                        {
                            new Dictionary<string, string>
                            {
                                ["PathRemovePrefix"] = "/api/enterprisesearch",
                            },
                            new Dictionary<string, string>
                            {
                                ["RequestHeaderRemove"] = "Cookie",
                            }
                        }
                    });
                }
                else
                {
                    // For all other proxy routes, use the original wildcard pattern
                    allRoutes.Add(new RouteConfig
                    {
                        RouteId = proxyRoute.Route,
                        ClusterId = proxyRoute.Route,
                        Match = new RouteMatch { Path = $"/api/{proxyRoute.Route.Trim('/')}/{{*any}}" },
                        AuthorizationPolicy = RequirePermissionAttribute.GetPermissionsStringValue(proxyRoute.RequirePermissions),
                        Transforms = new[]
                        {
                            new Dictionary<string, string>
                            {
                                ["PathRemovePrefix"] = $"/api/{proxyRoute.Route.Trim('/')}",
                            },
                            new Dictionary<string, string>
                            {
                                ["RequestHeaderRemove"] = "Cookie",
                            }
                        }
                    });
                }
            }

            _config = new SimpleProxyConfig(allRoutes, clusters);
        }

        public IProxyConfig GetConfig() => _config;

        private class SimpleProxyConfig : IProxyConfig
        {
            private readonly CancellationTokenSource _cts = new();

            public SimpleProxyConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
            {
                Routes = routes ?? throw new ArgumentNullException(nameof(routes));
                Clusters = clusters ?? throw new ArgumentNullException(nameof(clusters));
                ChangeToken = new CancellationChangeToken(_cts.Token);
            }

            public IReadOnlyList<RouteConfig> Routes { get; }

            public IReadOnlyList<ClusterConfig> Clusters { get; }

            public IChangeToken ChangeToken { get; }
        }
    }

    public class KissHttpClientFactory : ForwarderHttpClientFactory
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public KissHttpClientFactory(IHttpContextAccessor httpContextAccessor, IServiceScopeFactory serviceScopeFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override HttpMessageHandler WrapHandler(ForwarderHttpClientContext context, HttpMessageHandler handler)
            => new KissDelegatingHandler(_httpContextAccessor, _serviceScopeFactory) { InnerHandler = handler };
    }

    public class KissDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public KissDelegatingHandler(IHttpContextAccessor httpContextAccessor, IServiceScopeFactory serviceScopeFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var context = _httpContextAccessor.HttpContext;

            var clusterId = context?.Features.Get<IReverseProxyFeature>()?.Cluster.Config.ClusterId;

            // if we are in a request, re-use scoped services
            if (context != null) return await SendAsync(clusterId, context.RequestServices, request, cancellationToken);

            // if we are not in a request, create a scope here
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            return await SendAsync(clusterId, scope.ServiceProvider, request, cancellationToken);
        }

        private Task<HttpResponseMessage> SendAsync(string? clusterId, IServiceProvider services, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var middlewares = services
                .GetServices<IKissHttpClientMiddleware>()
                .Where(x => x.IsEnabled(clusterId));

            SendRequestMessageAsync inner = base.SendAsync;

            var sendAsync = middlewares.Aggregate(inner, (next, middleware) =>
            {
                return (req, token) => middleware.SendAsync(next, req, token);
            });

            return sendAsync(request, cancellationToken);
        }
    }
}


