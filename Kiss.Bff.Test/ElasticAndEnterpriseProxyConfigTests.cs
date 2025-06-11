using Microsoft.Extensions.DependencyInjection;
using Yarp.ReverseProxy.Configuration;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class ElasticAndEnterpriseProxyConfigTests
    {
        private ProxyConfigProvider _proxyConfigProvider = null!;
        private readonly string _enterpriseSearchUrl = "http://enterprise-search.local";
        private readonly string _elasticsearchUrl = "http://elasticsearch.local";

        [TestInitialize]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IKissProxyRoute>(new EnterpriseSearchProxyConfig(_enterpriseSearchUrl, "test-api-key"));
            services.AddSingleton<IKissProxyRoute>(new ElasticsearchProxyConfig(_elasticsearchUrl, "test-user", "test-pass"));
 
            var serviceProvider = services.BuildServiceProvider();
            var proxyRoutes = serviceProvider.GetServices<IKissProxyRoute>();
            _proxyConfigProvider = new ProxyConfigProvider(proxyRoutes);
        }

        [TestMethod]
        public void EnterpriseSearch_ShouldOnlyAllowPostToSearchExplainEndpoint()
        {
            var config = _proxyConfigProvider.GetConfig();
            var enterpriseSearchRoute = config.Routes.FirstOrDefault(r => r.RouteId == "enterprisesearch-search-explain");

            Assert.IsNotNull(enterpriseSearchRoute);
            Assert.AreEqual("POST", enterpriseSearchRoute?.Match?.Methods?.Single());
            Assert.AreEqual("/api/enterprisesearch/api/as/v1/engines/{engine}/search_explain", enterpriseSearchRoute?.Match.Path);
        }

        [TestMethod]
        public void EnterpriseSearch_ShouldNotAllowOtherHttpMethods()
        {
            var config = _proxyConfigProvider.GetConfig();
            var enterpriseSearchRoute = config.Routes.FirstOrDefault(r => r.RouteId == "enterprisesearch-search-explain");

            Assert.IsNotNull(enterpriseSearchRoute);
            var methods = enterpriseSearchRoute.Match.Methods;
            Assert.AreEqual(1, methods?.Count);
            Assert.IsTrue(methods?.Contains("POST"));
            Assert.IsFalse(methods?.Contains("GET"));
            Assert.IsFalse(methods?.Contains("PUT"));
            Assert.IsFalse(methods?.Contains("DELETE"));
        }

        [TestMethod]
        public void EnterpriseSearch_ShouldNotAllowOtherEndpoints()
        {
            var config = _proxyConfigProvider.GetConfig();
            
            Assert.IsNull(config.Routes.FirstOrDefault(r => r.RouteId == "enterprisesearch"));
            Assert.IsNull(config.Routes.FirstOrDefault(r => r.RouteId.Contains("enterprisesearch") && r.RouteId != "enterprisesearch-search-explain"));
        }

        [TestMethod]
        public void EnterpriseSearch_ShouldHaveCorrectPathTransform()
        {
            var config = _proxyConfigProvider.GetConfig();
            var enterpriseSearchRoute = config.Routes.FirstOrDefault(r => r.RouteId == "enterprisesearch-search-explain");

            Assert.IsNotNull(enterpriseSearchRoute);
            var pathTransform = enterpriseSearchRoute?.Transforms?.FirstOrDefault(t => t.ContainsKey("PathRemovePrefix"));
            Assert.IsNotNull(pathTransform);
            Assert.AreEqual("/api/enterprisesearch", pathTransform["PathRemovePrefix"]);
        }

        [TestMethod]
        public void EnterpriseSearch_ShouldRemoveCookieHeader()
        {
            var config = _proxyConfigProvider.GetConfig();
            var enterpriseSearchRoute = config.Routes.FirstOrDefault(r => r.RouteId == "enterprisesearch-search-explain");

            Assert.IsNotNull(enterpriseSearchRoute);
            var cookieTransform = enterpriseSearchRoute?.Transforms?.FirstOrDefault(t => t.ContainsKey("RequestHeaderRemove"));
            Assert.IsNotNull(cookieTransform);
            Assert.AreEqual("Cookie", cookieTransform["RequestHeaderRemove"]);
        }

        [TestMethod]
        public void Elasticsearch_ShouldOnlyAllowPostToSearchEndpoint()
        {
            var config = _proxyConfigProvider.GetConfig();
            var elasticsearchRoute = config.Routes.FirstOrDefault(r => r.RouteId == "elasticsearch-search");

            Assert.IsNotNull(elasticsearchRoute);
            Assert.AreEqual("POST", elasticsearchRoute?.Match?.Methods?.Single());
            Assert.AreEqual("/api/elasticsearch/{index}/_search", elasticsearchRoute?.Match.Path);
        }

        [TestMethod]
        public void Elasticsearch_ShouldNotAllowOtherHttpMethods()
        {
            var config = _proxyConfigProvider.GetConfig();
            var elasticsearchRoute = config.Routes.FirstOrDefault(r => r.RouteId == "elasticsearch-search");

            Assert.IsNotNull(elasticsearchRoute);
            var methods = elasticsearchRoute.Match.Methods;
            Assert.AreEqual(1, methods?.Count);
            Assert.IsTrue(methods?.Contains("POST"));
            Assert.IsFalse(methods?.Contains("GET"));
            Assert.IsFalse(methods?.Contains("PUT"));
            Assert.IsFalse(methods?.Contains("DELETE"));
        }

        [TestMethod]
        public void Elasticsearch_ShouldNotAllowOtherEndpoints()
        {
            var config = _proxyConfigProvider.GetConfig();
            
            Assert.IsNull(config.Routes.FirstOrDefault(r => r.RouteId == "elasticsearch"));
            Assert.IsNull(config.Routes.FirstOrDefault(r => r.RouteId.Contains("elasticsearch") && r.RouteId != "elasticsearch-search"));
        }

        [TestMethod]
        public void Elasticsearch_ShouldHaveCorrectPathTransform()
        {
            var config = _proxyConfigProvider.GetConfig();
            var elasticsearchRoute = config.Routes.FirstOrDefault(r => r.RouteId == "elasticsearch-search");

            Assert.IsNotNull(elasticsearchRoute);
            var pathTransform = elasticsearchRoute?.Transforms?.FirstOrDefault(t => t.ContainsKey("PathRemovePrefix"));
            Assert.IsNotNull(pathTransform);
            Assert.AreEqual("/api/elasticsearch", pathTransform["PathRemovePrefix"]);
        }

        [TestMethod]
        public void Elasticsearch_ShouldRemoveCookieHeader()
        {
            var config = _proxyConfigProvider.GetConfig();
            var elasticsearchRoute = config.Routes.FirstOrDefault(r => r.RouteId == "elasticsearch-search");

            Assert.IsNotNull(elasticsearchRoute);
            var cookieTransform = elasticsearchRoute?.Transforms?.FirstOrDefault(t => t.ContainsKey("RequestHeaderRemove"));
            Assert.IsNotNull(cookieTransform);
            Assert.AreEqual("Cookie", cookieTransform["RequestHeaderRemove"]);
        }

        [TestMethod]
        public void BothServices_ShouldHaveCorrectRouteCount()
        {
            var config = _proxyConfigProvider.GetConfig();
            
            Assert.AreEqual(2, config.Routes.Count);
            
            var routeIds = config.Routes.Select(r => r.RouteId).ToList();
            Assert.IsTrue(routeIds.Contains("enterprisesearch-search-explain"));
            Assert.IsTrue(routeIds.Contains("elasticsearch-search"));
        }

        [TestMethod]
        public void BothServices_ShouldHaveCorrectClusterCount()
        {
            var config = _proxyConfigProvider.GetConfig();
            
            Assert.AreEqual(2, config.Clusters.Count);
            
            var clusterIds = config.Clusters.Select(c => c.ClusterId).ToList();
            Assert.IsTrue(clusterIds.Contains("enterprisesearch"));
            Assert.IsTrue(clusterIds.Contains("elasticsearch"));
        }

        [TestMethod]
        public void BothServices_ShouldHaveDangerousCertificateConfig()
        {
            var config = _proxyConfigProvider.GetConfig();
            var enterpriseSearchCluster = config.Clusters.FirstOrDefault(c => c.ClusterId == "enterprisesearch");
            var elasticsearchCluster = config.Clusters.FirstOrDefault(c => c.ClusterId == "elasticsearch");

            Assert.IsNotNull(enterpriseSearchCluster?.HttpClient);
            Assert.IsTrue(enterpriseSearchCluster.HttpClient.DangerousAcceptAnyServerCertificate);
            
            Assert.IsNotNull(elasticsearchCluster?.HttpClient);
            Assert.IsTrue(elasticsearchCluster.HttpClient.DangerousAcceptAnyServerCertificate);
        }

        [TestMethod]
        public void BothServices_ShouldHaveCorrectDestinations()
        {
            var config = _proxyConfigProvider.GetConfig();
            var enterpriseSearchCluster = config.Clusters.FirstOrDefault(c => c.ClusterId == "enterprisesearch");
            var elasticsearchCluster = config.Clusters.FirstOrDefault(c => c.ClusterId == "elasticsearch");

            Assert.IsNotNull(enterpriseSearchCluster);
            Assert.AreEqual(_enterpriseSearchUrl, enterpriseSearchCluster?.Destinations?["enterprisesearch"].Address);
            
            Assert.IsNotNull(elasticsearchCluster);
            Assert.AreEqual(_elasticsearchUrl, elasticsearchCluster?.Destinations?["elasticsearch"].Address);
        }

        [TestMethod]
        public void EnterpriseSearch_PathPattern_ShouldMatchExpectedFormat()
        {
            var config = _proxyConfigProvider.GetConfig();
            var enterpriseSearchRoute = config.Routes.FirstOrDefault(r => r.RouteId == "enterprisesearch-search-explain");

            Assert.IsNotNull(enterpriseSearchRoute);
            var path = enterpriseSearchRoute.Match.Path;
            
            Assert.IsTrue(path?.StartsWith("/api/enterprisesearch/api/as/v1/engines/"));
            Assert.IsTrue(path?.EndsWith("/search_explain"));
            Assert.IsTrue(path?.Contains("{engine}"));
        }

        [TestMethod]
        public void Elasticsearch_PathPattern_ShouldMatchExpectedFormat()
        {
            var config = _proxyConfigProvider.GetConfig();
            var elasticsearchRoute = config.Routes.FirstOrDefault(r => r.RouteId == "elasticsearch-search");

            Assert.IsNotNull(elasticsearchRoute);
            var path = elasticsearchRoute.Match.Path;
            
            Assert.IsTrue(path?.StartsWith("/api/elasticsearch/"));
            Assert.IsTrue(path?.EndsWith("/_search"));
            Assert.IsTrue(path?.Contains("{index}"));
        }
    }
} 
