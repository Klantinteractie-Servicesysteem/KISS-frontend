using Microsoft.Extensions.DependencyInjection;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class EnterpriseProxyConfigTests
    {
        private ProxyConfigProvider _proxyConfigProvider = null!;
        private readonly string _enterpriseSearchUrl = "http://enterprise-search.local";

        [TestInitialize]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IKissProxyRoute>(new EnterpriseSearchProxyConfig(_enterpriseSearchUrl, "test-api-key"));

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
        public void EnterpriseSearch_ShouldHaveCorrectRouteCount()
        {
            var config = _proxyConfigProvider.GetConfig();

            Assert.AreEqual(1, config.Routes.Count);

            var routeIds = config.Routes.Select(r => r.RouteId).ToList();
            Assert.IsTrue(routeIds.Contains("enterprisesearch-search-explain"));
        }

        [TestMethod]
        public void EnterpriseSearch_ShouldHaveCorrectClusterCount()
        {
            var config = _proxyConfigProvider.GetConfig();

            Assert.AreEqual(1, config.Clusters.Count);

            var clusterIds = config.Clusters.Select(c => c.ClusterId).ToList();
            Assert.IsTrue(clusterIds.Contains("enterprisesearch"));
        }

        [TestMethod]
        public void EnterpriseSearch_ShouldHaveDangerousCertificateConfig()
        {
            var config = _proxyConfigProvider.GetConfig();
            var enterpriseSearchCluster = config.Clusters.FirstOrDefault(c => c.ClusterId == "enterprisesearch");

            Assert.IsNotNull(enterpriseSearchCluster?.HttpClient);
            Assert.IsTrue(enterpriseSearchCluster.HttpClient.DangerousAcceptAnyServerCertificate);
        }

        [TestMethod]
        public void EnterpriseSearch_ShouldHaveCorrectDestinations()
        {
            var config = _proxyConfigProvider.GetConfig();
            var enterpriseSearchCluster = config.Clusters.FirstOrDefault(c => c.ClusterId == "enterprisesearch");

            Assert.IsNotNull(enterpriseSearchCluster);
            Assert.AreEqual(_enterpriseSearchUrl, enterpriseSearchCluster?.Destinations?["enterprisesearch"].Address);
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
    }
}
