using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Yarp.ReverseProxy.Transforms;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class EnterpriseSearchProxyConfigTests
    {
        private const int MethodNotAllowed = StatusCodes.Status405MethodNotAllowed;

        private EnterpriseSearchProxyConfig? _config;

        private readonly string _username = "dummyUser";
        private readonly string _password = "dummyPass";
        private readonly string _baseUrl = "http://localhost";

        private readonly string _pattern = @"^/api/enterprisesearch/api/as/v1/engines/.+/search_explain$";

        [TestInitialize]
        public void Init()
        {
            _config = new EnterpriseSearchProxyConfig(_baseUrl, _username, _pattern);
        }

        private static RequestTransformContext CreateContext(string method, string path)
        {
            var context = new DefaultHttpContext();
            context.Request.Method = method;
            context.Request.Path = path;
            context.Response.Body = new MemoryStream();

            return new RequestTransformContext
            {
                HttpContext = context,
                ProxyRequest = new HttpRequestMessage()
            };
        }

        [DataTestMethod]
        [DataRow("/api/enterprisesearch/api/as/v1/engines/my_engine/search_explain")]
        [DataRow("/api/enterprisesearch/api/as/v1/engines/another-engine-with-hyphens/search_explain")]
        public async Task Valid_Enterprise_Path_Allows_Proxy(string path)
        {
            var ctx = CreateContext("POST", path);
            await _config.ApplyRequestTransform(ctx);

            Assert.IsNotNull(ctx.ProxyRequest.Headers.Authorization, "Authorization header should be added.");
            Assert.AreEqual("Bearer", ctx.ProxyRequest.Headers.Authorization?.Scheme);
            Assert.AreNotEqual(MethodNotAllowed, ctx.HttpContext.Response.StatusCode, "Status code should not be 405 for valid requests.");
        }

        [DataTestMethod]
        [DataRow("GET")]
        [DataRow("PUT")]
        [DataRow("DELETE")]
        [DataRow("post")]
        public async Task Invalid_Methods_Return_405(string method)
        {
            var ctx = CreateContext(method, "/api/enterprisesearch/api/as/v1/engines/any-engine/search_explain");
            await _config.ApplyRequestTransform(ctx);

            Assert.AreEqual(MethodNotAllowed, ctx.HttpContext.Response.StatusCode);
        }

        [DataTestMethod]
        [DataRow("/api/enterprisesearch/api/as/v1/engines/my_engine/search", "Near miss on path")]
        [DataRow("/api/enterprisesearch/api/as/v1/engines//search_explain", "Empty engine name")]
        [DataRow("/api/ENTERPRISESEARCH/api/as/v1/engines/my_engine/search_explain", "Incorrect casing")]
        [DataRow("/api/enterprisesearch/api/as/v1/engines/my_engine/search_explain?debug=true", "Path with query string")]
        [DataRow("/api/elasticsearch/my_index/_search", "Path for a different service")]
        public async Task Invalid_Enterprise_Path_Edge_Cases_Return_405(string path, string message)
        {
            var ctx = CreateContext("POST", path);
            await _config.ApplyRequestTransform(ctx);

            Assert.AreEqual(MethodNotAllowed, ctx.HttpContext.Response.StatusCode, message);
        }
    }

}
