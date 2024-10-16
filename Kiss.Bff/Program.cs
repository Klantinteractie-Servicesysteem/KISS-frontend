using System.Security.Claims;
using Kiss.Bff.Afdelingen;
using Kiss.Bff.Beheer.Data;
using Kiss.Bff.Beheer.Verwerking;
using Kiss.Bff.Config;
using Kiss.Bff.Extern.Klantinteracties;
using Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem.Shared;
using Kiss.Bff.Groepen;
using Kiss.Bff.InterneTaak;
using Kiss.Bff;
using Kiss.Bff.ZaakGerichtWerken.Contactmomenten;
using Kiss.Bff.ZaakGerichtWerken.Klanten;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    builder.WebHost.ConfigureKestrel(x =>
    {
        x.AddServerHeader = false;
    });

    // Add services to the container.
    builder.Services.AddControllers();

    const string AuthorityKey = "OIDC_AUTHORITY";

    var authority = builder.Configuration[AuthorityKey];

    if (string.IsNullOrWhiteSpace(authority))
    {
        Log.Fatal("Environment variable {variableKey} is missing", AuthorityKey);
    }

    builder.Services.AddKissAuth(options => 
    {
        options.Authority = authority;
        options.ClientId = builder.Configuration["OIDC_CLIENT_ID"];
        options.ClientSecret = builder.Configuration["OIDC_CLIENT_SECRET"];
        options.KlantcontactmedewerkerRole = builder.Configuration["OIDC_KLANTCONTACTMEDEWERKER_ROLE"];
        options.RedacteurRole = builder.Configuration["OIDC_REDACTEUR_ROLE"];
        options.MedewerkerIdentificatieClaimType = builder.Configuration["OIDC_MEDEWERKER_IDENTIFICATIE_CLAIM"];
        if(int.TryParse(builder.Configuration["OIDC_MEDEWERKER_IDENTIFICATIE_TRUNCATE"], out var truncate))
        {
            options.TruncateMedewerkerIdentificatie = truncate;
        }
    });

    builder.Services.AddJwtAuth(options =>
    {
        options.SecretKey = builder.Configuration["CONTACTMOMENTDETAILS_API_KEY"];
    });

    builder.Services.AddKissProxy();
    builder.Services.AddKvk(builder.Configuration["KVK_BASE_URL"], builder.Configuration["KVK_API_KEY"]);
    builder.Services.AddHaalCentraal(builder.Configuration["HAAL_CENTRAAL_BASE_URL"], builder.Configuration["HAAL_CENTRAAL_API_KEY"]);
    builder.Services.AddZgwTokenProvider(builder.Configuration["ZAKEN_API_KEY"], builder.Configuration["ZAKEN_API_CLIENT_ID"]);

    builder.Services.AddHttpClient();

    builder.Services.AddZaaksystemen(builder.Configuration);

    var connStr = $"Username={builder.Configuration["POSTGRES_USER"]};Password={builder.Configuration["POSTGRES_PASSWORD"]};Host={builder.Configuration["POSTGRES_HOST"]};Database={builder.Configuration["POSTGRES_DB"]};Port={builder.Configuration["POSTGRES_PORT"]}";
    builder.Services.AddDbContext<BeheerDbContext>(o => o.UseNpgsql(connStr));
    builder.Services.AddEnterpriseSearch(builder.Configuration["ENTERPRISE_SEARCH_BASE_URL"], builder.Configuration["ENTERPRISE_SEARCH_PRIVATE_API_KEY"]);

    builder.Services.AddKlantenProxy(builder.Configuration["KLANTEN_BASE_URL"], builder.Configuration["KLANTEN_CLIENT_ID"], builder.Configuration["KLANTEN_CLIENT_SECRET"]);
    builder.Services.AddContactmomentenProxy(builder.Configuration["CONTACTMOMENTEN_BASE_URL"], builder.Configuration["CONTACTMOMENTEN_API_CLIENT_ID"], builder.Configuration["CONTACTMOMENTEN_API_KEY"]);

    if(int.TryParse(builder.Configuration["EMAIL_PORT"], out var emailPort)) 
    {
        builder.Services.AddSmtpClient(
            builder.Configuration["EMAIL_HOST"],
            emailPort,
            builder.Configuration["EMAIL_USERNAME"],
            builder.Configuration["EMAIL_PASSWORD"],
            bool.TryParse(builder.Configuration["EMAIL_ENABLE_SSL"], out var enableSsl) && enableSsl
        );
    }

    builder.Services.AddDataProtection()
        .PersistKeysToDbContext<BeheerDbContext>()
        .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration()
        {
            EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC, // default
            ValidationAlgorithm = ValidationAlgorithm.HMACSHA256, // default
        });

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped(s => s.GetService<IHttpContextAccessor>()?.HttpContext?.User ?? new ClaimsPrincipal());
    builder.Services.AddVerwerkingMiddleware();

    builder.Services.AddHttpClient("default").AddHttpMessageHandler(s => new KissDelegatingHandler(s.GetRequiredService<IHttpContextAccessor>(), s.GetRequiredService<IServiceScopeFactory>()));

    builder.Services.AddHealthChecks();

    builder.Services.AddElasticsearch(builder.Configuration["ELASTIC_BASE_URL"], builder.Configuration["ELASTIC_USERNAME"], builder.Configuration["ELASTIC_PASSWORD"]);
    builder.Services.AddInterneTaakProxy(builder.Configuration["INTERNE_TAAK_BASE_URL"], builder.Configuration["INTERNE_TAAK_TOKEN"], builder.Configuration["INTERNE_TAAK_OBJECT_TYPE_URL"], builder.Configuration["INTERNE_TAAK_CLIENT_ID"], builder.Configuration["INTERNE_TAAK_CLIENT_SECRET"], builder.Configuration["INTERNE_TAAK_TYPE_VERSION"]);
    builder.Services.AddAfdelingenProxy(builder.Configuration["AFDELINGEN_BASE_URL"], builder.Configuration["AFDELINGEN_TOKEN"], builder.Configuration["AFDELINGEN_OBJECT_TYPE_URL"], builder.Configuration["AFDELINGEN_CLIENT_ID"], builder.Configuration["AFDELINGEN_CLIENT_SECRET"]);
    builder.Services.AddGroepenProxy(builder.Configuration["GROEPEN_BASE_URL"], builder.Configuration["GROEPEN_TOKEN"], builder.Configuration["GROEPEN_OBJECT_TYPE_URL"], builder.Configuration["GROEPEN_CLIENT_ID"], builder.Configuration["GROEPEN_CLIENT_SECRET"]);

    builder.Services.AddKlantinteracties(builder.Configuration["KLANTINTERACTIES_BASE_URL"], builder.Configuration["KLANTINTERACTIES_TOKEN"]);

    builder.Host.UseSerilog((ctx, services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext());

    var app = builder.Build(); 


    // Configure the HTTP request pipeline.

    app.UseHttpsRedirection();

    app.UseSerilogRequestLogging();

    app.UseKissStaticFiles();
    app.UseKissSecurityHeaders();

    app.UseKissAuthMiddlewares();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapKissAuthEndpoints();
    app.MapControllers();
    app.MapKissProxy();
    app.MapHealthChecks("/healthz");
    app.MapFallbackToIndexHtml();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<BeheerDbContext>();
        if (db.Database.IsRelational())
        {
            await db.Database.MigrateAsync(app.Lifetime.ApplicationStopping);
        }
    }

    app.Run();
}
catch (Exception ex)
{
    var type = ex.GetType().Name;
    if (type.Equals("StopTheHostException", StringComparison.Ordinal))
    {
        throw;
    }
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}

public partial class Program { }
