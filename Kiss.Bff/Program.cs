using System.Security.Claims;
using Kiss.Bff.Beheer.Data;
using Kiss.Bff.Beheer.Verwerking;
using Kiss.Bff.Config;
using Kiss.Bff.ZaakGerichtWerken;
using Kiss.Bff.ZaakGerichtWerken.Contactmomenten;
using Kiss.Bff.ZaakGerichtWerken.Klanten;
using Kiss.Bff.Zaken;
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
    builder.Services.AddKissAuth(
        builder.Configuration["OIDC_AUTHORITY"],
        builder.Configuration["OIDC_CLIENT_ID"],
        builder.Configuration["OIDC_CLIENT_SECRET"],
        builder.Configuration["OIDC_KLANTCONTACTMEDEWERKER_ROLE"],
        builder.Configuration["OIDC_REDACTEUR_ROLE"]
    );
    builder.Services.AddKissProxy();
    builder.Services.AddKvk(builder.Configuration["KVK_BASE_URL"], builder.Configuration["KVK_API_KEY"]);
    builder.Services.AddHaalCentraal(builder.Configuration["HAAL_CENTRAAL_BASE_URL"], builder.Configuration["HAAL_CENTRAAL_API_KEY"]);

    builder.Services.AddZgwTokenProvider(builder.Configuration["ZAKEN_API_KEY"], builder.Configuration["ZAKEN_API_CLIENT_ID"]);
    builder.Services.AddZaken(builder.Configuration["ZAKEN_BASE_URL"]);
    builder.Services.AddDocumenten(builder.Configuration["ZAKEN_BASE_URL"]);


    var connStr = $"Username={builder.Configuration["POSTGRES_USER"]};Password={builder.Configuration["POSTGRES_PASSWORD"]};Host={builder.Configuration["POSTGRES_HOST"]};Database={builder.Configuration["POSTGRES_DB"]};Port={builder.Configuration["POSTGRES_PORT"]}";
    builder.Services.AddDbContext<BeheerDbContext>(o => o.UseNpgsql(connStr));
    builder.Services.AddEnterpriseSearch(builder.Configuration["ENTERPRISE_SEARCH_BASE_URL"], builder.Configuration["ENTERPRISE_SEARCH_PUBLIC_API_KEY"]);

    builder.Services.AddKlantenProxy(builder.Configuration["KLANTEN_BASE_URL"], builder.Configuration["KLANTEN_CLIENT_ID"], builder.Configuration["KLANTEN_CLIENT_SECRET"]);
    builder.Services.AddContactmomentenProxy(builder.Configuration["CONTACTMOMENTEN_BASE_URL"], builder.Configuration["CONTACTMOMENTEN_API_CLIENT_ID"], builder.Configuration["CONTACTMOMENTEN_API_KEY"]);

    builder.Services.AddSmtpClient(
        builder.Configuration["EMAIL_HOST"],
        int.Parse(builder.Configuration["EMAIL_PORT"]),
        builder.Configuration["EMAIL_USERNAME"],
        builder.Configuration["EMAIL_PASSWORD"],
        bool.TryParse(builder.Configuration["EMAIL_ENABLE_SSL"], out var enableSsl) && enableSsl
    );

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
        await db.Database.MigrateAsync(app.Lifetime.ApplicationStopping);
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
