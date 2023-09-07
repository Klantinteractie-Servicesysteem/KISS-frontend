using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kiss.Bff.Test.Config
{
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string TestScheme = "Test";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IHttpContextAccessor httpContextAccessor)
            : base(options, logger, encoder, clock)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var keyValuePairs = (_httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("claims", out var claimsStr) ?? false)
                ? claimsStr.SelectMany(x=> JsonSerializer.Deserialize<IEnumerable<KeyValuePair<string,string>>>(x) ?? Enumerable.Empty<KeyValuePair<string,string>>())
                : Enumerable.Empty<KeyValuePair<string, string>>();

            var claims = keyValuePairs.Select(x => new Claim(x.Key, x.Value)).ToArray();

            if (!claims.Any())
            {
                return Task.FromResult(AuthenticateResult.Fail("unauthorized"));
            }

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, TestScheme);

            var result = AuthenticateResult.Success(ticket);

            return Task.FromResult(result);
        }
    }

    public static class TestAuthExtensions
    {
        public static void AddTestAuth(this IServiceCollection services) => services.AddAuthentication(defaultScheme: TestAuthHandler.TestScheme)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.TestScheme, options => { });

        public static HttpClient WithClaims(this HttpClient httpClient, params (string, string)[] claims)
        {
            httpClient.DefaultRequestHeaders.Add("claims", JsonSerializer.Serialize(claims.Select(x => new KeyValuePair<string,string>(x.Item1, x.Item2))));
            return httpClient.WithAuthHeader();
        }

        public static HttpClient AsKlantcontactmedewerker(this HttpClient httpClient) => httpClient.WithRoles("Klantcontactmedewerker");

        public static HttpClient WithRoles(this HttpClient httpClient, params string[] roles)
            => httpClient.WithClaims(roles.Select(r => (ClaimTypes.Role, r)).ToArray());

        private static HttpClient WithAuthHeader( this HttpClient httpClient) 
        {
            if (httpClient.DefaultRequestHeaders.Authorization == null)
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "TestScheme");
            }
            return httpClient;
        }

    }
}
