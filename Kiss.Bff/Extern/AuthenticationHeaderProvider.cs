using System.Net.Http.Headers;

namespace Kiss.Bff
{


    public class AuthenticationHeaderProvider
    {
        public readonly AuthenticationHeaderValue? AuthHeader;
        public readonly ZgwTokenProvider? TokenProvider;

        public AuthenticationHeaderProvider(string? token, string? clientId, string? clientSecret)
        {

            if (!string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(clientSecret))
            {
                TokenProvider = new ZgwTokenProvider(clientSecret, clientId);
                return;
            }

            if (!string.IsNullOrWhiteSpace(token))
            {
                AuthHeader = new AuthenticationHeaderValue("Token", token);
                return;
            }
        }

        public void ApplyAuthorizationHeader(HttpRequestHeaders headers, System.Security.Claims.ClaimsPrincipal user)
        {
            if (TokenProvider == null && AuthHeader == null)
            {
                throw new Exception("Setting up a proxy failed. A token or clientId/clientSecret combination should be provided");
            }

            if (AuthHeader != null)
            {
                headers.Authorization = AuthHeader;
            }
            else if (TokenProvider != null)
            {
                var token = TokenProvider.GenerateToken(user);
                headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

        }
    }
}
