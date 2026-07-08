using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Kiss.Bff
{

    public static class ZgwTokenExtensions
    {
        public static IServiceCollection AddZgwTokenProvider(this IServiceCollection services, string apiKey, string clientId)
        {
            services.AddSingleton(new ZgwTokenProvider(apiKey, clientId));
            return services;
        }
    }

    //documentatie: https://open-zaak.readthedocs.io/en/latest/client-development/authentication.html
    public class ZgwTokenProvider
    {
        private readonly string _apiKey;
        private readonly string _clientId;

        public ZgwTokenProvider(string apiKey, string clientId)
        {
            _apiKey = apiKey;
            _clientId = clientId;
        }


        public string GenerateToken(ClaimsPrincipal? claimsPrincipal)
        {

            var secretKey = _apiKey; // "een sleutel van minimaal 16 karakters";
            var client_id = _clientId;
            var iss = _clientId;
            var user_id = claimsPrincipal.GetUserName() ?? string.Empty;
            var user_representation = claimsPrincipal?.Identity?.Name ?? string.Empty;
            var now = DateTime.UtcNow;
            // one minute leeway to account for clock differences between machines
            var issuedAt = now.AddMinutes(-1);

            var claims = new Dictionary<string, object>
                {
                    { "client_id", client_id },
                    { "iss", iss },
                    { "user_id", user_id},
                    { "user_representation", user_representation }
                };

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                IssuedAt = issuedAt,
                NotBefore = issuedAt,
                Claims = claims,
                Subject = new ClaimsIdentity(),
                Expires = now.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);



        }
    }
}
