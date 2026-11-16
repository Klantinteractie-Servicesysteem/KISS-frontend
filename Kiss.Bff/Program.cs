using System.Security.Claims;
using Duende.IdentityModel.Client;
using Kiss.Bff;
using Kiss.Bff.Afdelingen;
using Kiss.Bff.Beheer.Data;
using Kiss.Bff.Beheer.Verwerking;
using Kiss.Bff.Config;
using Kiss.Bff.Extern;
using Kiss.Bff.Extern.Elasticsearch;
using Kiss.Bff.Extern.Logboek;
using Kiss.Bff.Extern.Pabc;
using Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem;
using Kiss.Bff.Groepen;
using Kiss.Bff.Intern.Seed.Features;
using Kiss.Bff.Vacs;
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

    var authority = builder.Configuration.GetOrEmpty(AuthorityKey);

    if (string.IsNullOrWhiteSpace(authority))
    {
        Log.Fatal("Environment variable {variableKey} is missing", AuthorityKey);
    }

    builder.Services.AddKissAuth(options =>
    {
        options.Authority = authority;
        options.ClientId = builder.Configuration.GetOrEmpty("OIDC_CLIENT_ID");
        options.ClientSecret = builder.Configuration.GetOrEmpty("OIDC_CLIENT_SECRET");
        options.KlantcontactmedewerkerRole = builder.Configuration["OIDC_KLANTCONTACTMEDEWERKER_ROLE"];
        options.RedacteurRole = builder.Configuration["OIDC_REDACTEUR_ROLE"];
        options.BeheerderRole = builder.Configuration["OIDC_BEHEERDER_ROLE"];
        options.KennisbankRole = builder.Configuration["OIDC_KENNISBANK_ROLE"];
        options.MedewerkerIdentificatieClaimType = builder.Configuration["OIDC_MEDEWERKER_IDENTIFICATIE_CLAIM"];
        if (int.TryParse(builder.Configuration["OIDC_MEDEWERKER_IDENTIFICATIE_TRUNCATE"], out var truncate))
        {
            options.TruncateMedewerkerIdentificatie = truncate;
        }
        options.JwtTokenAuthenticationSecret = builder.Configuration["MANAGEMENTINFORMATIE_API_KEY"];
    });

    //builder.Services.AddJwtAuth(options =>
    //{
    //    options.SecretKey = builder.Configuration["MANAGEMENTINFORMATIE_API_KEY"];
    //});

    builder.Services.AddKissProxy();
    builder.Services.AddKvk(builder.Configuration.GetOrEmpty("KVK_BASE_URL"), builder.Configuration["KVK_API_KEY"], builder.Configuration["KVK_USER_HEADER_NAME"], builder.Configuration.GetSection("KVK_CUSTOM_HEADERS")?.Get<Dictionary<string, string>>());
    builder.Services.AddHaalCentraal(builder.Configuration);
    builder.Services.AddZgwTokenProvider(builder.Configuration.GetOrEmpty("ZAKEN_API_KEY"), builder.Configuration.GetOrEmpty("ZAKEN_API_CLIENT_ID"));

    builder.Services.AddHttpClient();

    builder.Services.AddRegistryConfig(builder.Configuration);

    var connStr = $"Username={builder.Configuration["POSTGRES_USER"]};Password={builder.Configuration["POSTGRES_PASSWORD"]};Host={builder.Configuration["POSTGRES_HOST"]};Database={builder.Configuration["POSTGRES_DB"]};Port={builder.Configuration["POSTGRES_PORT"]}";
    builder.Services.AddDbContext<BeheerDbContext>(o => o.UseNpgsql(connStr));
    if (int.TryParse(builder.Configuration["EMAIL_PORT"], out var emailPort))
    {
        builder.Services.AddSmtpClient(
            builder.Configuration.GetOrEmpty("EMAIL_HOST"),
            emailPort,
            builder.Configuration.GetOrEmpty("EMAIL_USERNAME"),
            builder.Configuration.GetOrEmpty("EMAIL_PASSWORD"),
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

    builder.Services.AddMemoryCache();
    builder.Services.AddHttpClient<ElasticsearchService>(httpClient => { httpClient.BaseAddress = new Uri(builder.Configuration.GetRequired("ELASTIC_BASE_URL")); httpClient.SetBasicAuthentication(builder.Configuration.GetOrEmpty("ELASTIC_USERNAME"), builder.Configuration.GetOrEmpty("ELASTIC_PASSWORD")); }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator });
    builder.Services.AddAfdelingenProxy(builder.Configuration.GetOrEmpty("AFDELINGEN_BASE_URL"), builder.Configuration.GetOrEmpty("AFDELINGEN_TOKEN"), builder.Configuration.GetOrEmpty("AFDELINGEN_OBJECT_TYPE_URL"), builder.Configuration.GetOrEmpty("AFDELINGEN_CLIENT_ID"), builder.Configuration.GetOrEmpty("AFDELINGEN_CLIENT_SECRET"));
    builder.Services.AddGroepenProxy(builder.Configuration.GetOrEmpty("GROEPEN_BASE_URL"), builder.Configuration.GetOrEmpty("GROEPEN_TOKEN"), builder.Configuration.GetOrEmpty("GROEPEN_OBJECT_TYPE_URL"), builder.Configuration.GetOrEmpty("GROEPEN_CLIENT_ID"), builder.Configuration.GetOrEmpty("GROEPEN_CLIENT_SECRET"));
    builder.Services.AddVacsProxy(builder.Configuration.GetOrEmpty("VAC_OBJECTEN_BASE_URL"), builder.Configuration.GetOrEmpty("VAC_OBJECTEN_TOKEN"), builder.Configuration.GetOrEmpty("VAC_OBJECT_TYPE_URL"), builder.Configuration.GetOrEmpty("VAC_OBJECT_TYPE_VERSION"));
    builder.Services.AddLogboekProxy(builder.Configuration.GetOrEmpty("LOGBOEK_BASE_URL"), builder.Configuration.GetOrEmpty("LOGBOEK_TOKEN"), builder.Configuration.GetOrEmpty("LOGBOEK_OBJECT_TYPE_URL"), builder.Configuration.GetOrEmpty("LOGBOEK_OBJECT_TYPE_VERSION"));

    builder.Services.AddPabcClient(builder.Configuration);

    builder.Host.UseSerilog((ctx, services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext());

    builder.Services.AddScoped<BerichtenService>();
    builder.Services.AddScoped<SkillsService>();
    builder.Services.AddScoped<LinksService>();
    builder.Services.AddScoped<GespreksresultatenService>();
    builder.Services.AddScoped<ZaaksysteemClient>();

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
    app.MapHealthChecks("/healthz").AllowAnonymous();
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
    Log.Fatal(ex, "Unhandled exception");
    throw;
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}

public partial class Program { }
