using System.Security.Claims;
using Kiss.Bff.Beheer.Data;

namespace Kiss.Bff.Beheer.Verwerking
{
    public static class VerwerkingMiddleware
    {
        public static IServiceCollection AddVerwerkingMiddleware(this IServiceCollection services) => services.AddScoped<IKissHttpClientMiddleware, VerwerkingsHttpClientMiddleware>();

        public class VerwerkingsHttpClientMiddleware : IKissHttpClientMiddleware
        {
            private readonly BeheerDbContext _db;
            private readonly ClaimsPrincipal _user;
            private readonly ILogger<VerwerkingsHttpClientMiddleware> _logger;

            public VerwerkingsHttpClientMiddleware(BeheerDbContext db, ClaimsPrincipal user, ILogger<VerwerkingsHttpClientMiddleware> logger)
            {
                _db = db;
                _user = user;
                _logger = logger;
            }

            // we don't need to log elasticsearch calls
            public bool IsEnabled(string? clusterId) => clusterId != EnterpriseSearchProxyConfig.ROUTE;

            public async Task<HttpResponseMessage> SendAsync(SendRequestMessageAsync next, HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var result = await next(request, cancellationToken);

                // no need to log unsuccessful calls
                if (!result.IsSuccessStatusCode) return result;

                try
                {
                    var apiEndpoint = new UriBuilder(request.RequestUri!)
                    {
                        // query string could contain sensitive data
                        Query = string.Empty
                    }.Uri.ToString();

                    var userId = _user?.GetId();
                    var logging = new VerwerkingsLog { ApiEndpoint = apiEndpoint, Method = request.Method.Method, UserId = userId };
                    await _db.AddAsync(logging, cancellationToken);
                    await _db.SaveChangesAsync(cancellationToken);
                }
                // the request should not fail if we can't log (for now)
                catch (Exception e)
                {
                    _logger.LogError(e, "Could not save VerwerkingsLog");
                }

                return result;
            }
        }
    }
}
