﻿using Kiss.Bff;
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

            if(request.Content is JsonContent)
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
            var routes = proxyRoutes.Select(x => new RouteConfig
            {
                RouteId = x.Route,
                ClusterId = x.Route,
                Match = new RouteMatch { Path = $"/api/{x.Route.Trim('/')}/{{*any}}" },

                Transforms = new[]
                {
                    new Dictionary<string, string>
                    {
                        ["PathRemovePrefix"] = $"/api/{x.Route.Trim('/')}",
                    },
                    new Dictionary<string, string>
                    {
                        ["RequestHeaderRemove"] = "Cookie",
                    },
                    //   new Dictionary<string, string>
                    //{
                    //    ["QueryValueParameter"] = "klant",
                    //    ["Append"] = "bar"
                    //}
                }


            }).ToArray();

            var clusters = proxyRoutes.Select(x => new ClusterConfig
            {
                ClusterId = x.Route,
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    [x.Route] = new DestinationConfig
                    {
                        Address = x.Destination
                    }
                },
                // TODO: discuss if we need to get a valid certificate for Enterprise Search
                HttpClient = x.Route == EnterpriseSearchProxyConfig.ROUTE || x.Route == ElasticsearchProxyConfig.ROUTE
                ? new HttpClientConfig
                {
                    DangerousAcceptAnyServerCertificate = true
                }
                : null

            }).ToArray();

            _config = new SimpleProxyConfig(routes, clusters);

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


