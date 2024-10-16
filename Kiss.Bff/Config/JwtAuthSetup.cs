using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Kiss;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class JwtAuthSetupExtensions
    {
        public static IServiceCollection AddJwtAuth(this IServiceCollection services, Action<JwtAuthOptions> configureOptions)
        {
            var jwtOptions = new JwtAuthOptions();
            configureOptions(jwtOptions);

            jwtOptions.SecretKey = jwtOptions.SecretKey ?? string.Empty;

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.ExternSysteemPolicy, policy =>
                {
                    policy.RequireRole("ExternSysteem");
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                });
            });

            return services;
        }
    }

    public class JwtAuthOptions
    {
        public string SecretKey { get; set; } = string.Empty;
    }
}
