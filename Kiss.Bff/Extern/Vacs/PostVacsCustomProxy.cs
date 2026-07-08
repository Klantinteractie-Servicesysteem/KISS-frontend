using System.Text.Json.Nodes;
using Kiss.Bff.Config.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Vacs
{
    [ApiController]
    public class PostVacsCustomProxy : ControllerBase
    {
        private readonly VacsProxyConfig _config;

        public PostVacsCustomProxy(VacsProxyConfig config)
        {
            _config = config;
        }

        [HttpPost]
        [RequirePermission(RequirePermissionTo.vacsbeheer)]
        [Route("api/vacs/api/{version}/objects")]
        public IActionResult Post([FromRoute] string version, [FromBody] JsonObject node)
        {
            node["type"] = _config.ObjectTypeUrl;

            if (node.TryGetPropertyValue("record", out var record) && record is JsonObject recordObj)
            {
                recordObj["typeVersion"] = _config.TypeVersion;
            }

            var url = $"{_config.Destination.TrimEnd('/')}/api/{version}/objects";

            return new ProxyResult(() =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(node) };
                _config.ApplyHeaders(request.Headers);
                return request;
            });
        }
    }
}
