﻿using System.Net.Http.Headers;
using Yarp.ReverseProxy.Transforms;

namespace Kiss.Bff.InterneTaak
{
    public static class InterneTaakExtensions
    {
        public static IServiceCollection AddInterneTaakProxy(this IServiceCollection services, string destination, string token, string objectTypeUrl)
            => services.AddSingleton(new InterneTaakProxyConfig(destination, token, objectTypeUrl))
            .AddSingleton<IKissProxyRoute>(s => s.GetRequiredService<InterneTaakProxyConfig>());
    }

    public class InterneTaakProxyConfig : IKissProxyRoute
    {
        private readonly AuthenticationHeaderValue _authHeader;

        public InterneTaakProxyConfig(string destination, string token, string objectTypeUrl)
        {
            Destination = destination;
            ObjectTypeUrl = objectTypeUrl;
            _authHeader = new AuthenticationHeaderValue("Token", token);
        }

        public string Route => "internetaak";

        public string Destination { get; }
        public string ObjectTypeUrl { get; }


        public ValueTask ApplyRequestTransform(RequestTransformContext context)
        {
            ApplyHeaders(context.ProxyRequest.Headers);
            var request = context.HttpContext.Request;
            var isObjectsEndpoint = request.Path.Value?.AsSpan().TrimEnd('/').EndsWith("objects") ?? false;
            if (request.Method == HttpMethods.Get && isObjectsEndpoint)
            {
                context.Query.Collection["type"] = new(ObjectTypeUrl);
            }
            return new();
        }

        public void ApplyHeaders(HttpRequestHeaders headers)
        {
            headers.Authorization = _authHeader;
            headers.Add("Content-Crs", "EPSG:4326");
        }
    }
}
