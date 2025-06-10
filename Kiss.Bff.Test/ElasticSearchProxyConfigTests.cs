using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using Yarp.ReverseProxy.Transforms;
using Microsoft.Extensions.DependencyInjection;

namespace Kiss.Bff.Test
{
 
    [TestClass]
    public class ElasticSearchProxyConfigTests
    {
        private const int MethodNotAllowed = StatusCodes.Status405MethodNotAllowed;
         
        private ElasticsearchProxyConfig? _config;

        private readonly string _username = "dummyUser";
        private readonly string _password = "dummyPass";
        private readonly string _baseUrl = "http://localhost";
         
        private readonly string _pattern = @"^/api/elasticsearch/.+/_search$";

        [TestInitialize]
        public void Init()
        {
           
            _config = new ElasticsearchProxyConfig(_baseUrl, _username, _password);
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
        [DataRow("/api/elasticsearch/my_index/_search")]
        [DataRow("/api/elasticsearch/another-index/_search")]
        [DataRow("/api/elasticsearch/index.with.dots-v1/_search")] 
        public async Task Valid_Elastic_Path_Allows_Proxy(string path)
        {
            var ctx = CreateContext("POST", path);
            await _config.ApplyRequestTransform(ctx);

            Assert.IsNotNull(ctx.ProxyRequest.Headers.Authorization, "Authorization header should be added.");
            Assert.AreEqual("Basic", ctx.ProxyRequest.Headers.Authorization?.Scheme);
            Assert.AreNotEqual(MethodNotAllowed, ctx.HttpContext.Response.StatusCode, "Status code should not be 405 for valid requests.");
        }

        [DataTestMethod]
        [DataRow("GET")]
        [DataRow("PUT")]
        [DataRow("DELETE")]
        [DataRow("post")] 
        public async Task Invalid_Methods_Return_405(string method)
        {
            var ctx = CreateContext(method, "/api/elasticsearch/any-index/_search");
            await _config.ApplyRequestTransform(ctx);

            Assert.AreEqual(MethodNotAllowed, ctx.HttpContext.Response.StatusCode);
        }

        [DataTestMethod]
        [DataRow("/api/elasticsearch/my_index/search", "Near miss on path")]
        [DataRow("/api/elasticsearch//_search", "Empty index name")]
        [DataRow("/api/ELASTICSEARCH/my_index/_search", "Incorrect casing")]
        [DataRow("/api/elasticsearch/my_index/_search?pretty=true", "Path with query string")]
        [DataRow("/api/enterprisesearch/api/as/v1/engines/my_engine/search_explain", "Path for a different service")]
        public async Task Invalid_Elastic_Path_Edge_Cases_Return_405(string path, string message)
        {
            var ctx = CreateContext("POST", path);
            await _config.ApplyRequestTransform(ctx);

            Assert.AreEqual(MethodNotAllowed, ctx.HttpContext.Response.StatusCode, message);
        }
    }
}
