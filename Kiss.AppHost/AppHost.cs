using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

var postgresUser = builder.AddParameter("postgres-user");
var postgresPassword = builder.AddParameter("postgres-password", secret: true);
var postgresDbName = builder.AddParameter("postgres-db");

var postgres = builder.AddPostgres("postgres", postgresUser, postgresPassword)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var kissDb = postgres.AddDatabase("kiss-bff-db", builder.Configuration["Parameters:postgres-db"] ?? "kiss.bff-test");

// ──────────────────────────────────────────────────────────────────────────────
// Kiss.Bff — ASP.NET Core BFF/API
// ──────────────────────────────────────────────────────────────────────────────
var bff = builder.AddProject<Projects.Kiss_Bff>("kiss-bff")
    .WaitFor(postgres)
    // PostgreSQL — individual env vars to match existing code
    .WithEnvironment("POSTGRES_HOST", postgres.Resource.PrimaryEndpoint.Property(EndpointProperty.Host))
    .WithEnvironment("POSTGRES_PORT", postgres.Resource.PrimaryEndpoint.Property(EndpointProperty.Port))
    .WithEnvironment("POSTGRES_USER", postgresUser)
    .WithEnvironment("POSTGRES_PASSWORD", postgresPassword)
    .WithEnvironment("POSTGRES_DB", postgresDbName)
    ;

// ──────────────────────────────────────────────────────────────────────────────
// Kiss.Elastic.Sync — Elasticsearch synchronization worker
// ──────────────────────────────────────────────────────────────────────────────
var elasticSyncArgs = new List<string>();
var elasticSync = builder.AddProject<Projects.Kiss_Elastic_Sync>("kiss-elastic-sync")
    .WithExplicitStart()
    .WithArgs(ctx =>
    {
        foreach (var arg in elasticSyncArgs)
        {
            ctx.Args.Add(arg);
        }
    })
    .WithCommand(
        name: "run-sync",
        displayName: "Run Elastic Sync",
        executeCommand: async context =>
        {
            var interaction = context.ServiceProvider.GetRequiredService<IInteractionService>();

            var inputs = new List<InteractionInput>
            {
                new()
                {
                    Name = "source",
                    Label = "Source type",
                    InputType = InputType.Choice,
                    Required = true,
                    Options =
                    [
                        new("products", "Products (default)"),
                        new("vac", "VAC"),
                        new("smoelenboek", "Smoelenboek"),
                        new("sharepoint", "SharePoint"),
                        new("domain", "Domain (crawl a URL)"),
                    ]
                },
                new()
                {
                    Name = "domainUrl",
                    Label = "Domain URL",
                    InputType = InputType.Text,
                    Required = false,
                    Placeholder = "https://www.example.nl"
                },
                new()
                {
                    Name = "domainSourceName",
                    Label = "Domain source name (optional)",
                    InputType = InputType.Text,
                    Required = false,
                    Placeholder = "engine-crawler"
                }
            };

            var result = await interaction.PromptInputsAsync(
                "Run Kiss Elastic Sync",
                "Select the source to sync. If you choose 'Domain', also provide the URL.",
                inputs);

            if (result.Canceled)
                return CommandResults.Canceled();

            var source = result.Data[0].Value ?? "products";
            var domainUrl = result.Data[1].Value;
            var domainSourceName = result.Data[2].Value;

            elasticSyncArgs.Clear();
            switch (source)
            {
                case "domain":
                    if (string.IsNullOrWhiteSpace(domainUrl))
                        return CommandResults.Failure("A URL is required when using the 'domain' source.");
                    elasticSyncArgs.Add("domain");
                    elasticSyncArgs.Add(domainUrl);
                    if (!string.IsNullOrWhiteSpace(domainSourceName))
                        elasticSyncArgs.Add(domainSourceName);
                    break;
                case "products":
                    // no args = default (products)
                    break;
                default:
                    elasticSyncArgs.Add(source);
                    break;
            }

            var commandService = context.ServiceProvider.GetRequiredService<ResourceCommandService>();
            var startResult = await commandService.ExecuteCommandAsync(
                resourceId: "kiss-elastic-sync",
                commandName: "start",
                cancellationToken: context.CancellationToken);

            return startResult.Success
                ? CommandResults.Success()
                : CommandResults.Failure($"Failed to start: {startResult.Message}");
        },
        commandOptions: new CommandOptions
        {
            IconName = "PlayCircleSparkle",
            IconVariant = IconVariant.Filled,
            IsHighlighted = true,
            Description = "Start the Elastic Sync process with a chosen source. This is the preferred way to run it."
        })
    ;

// ──────────────────────────────────────────────────────────────────────────────
// Frontend — Vite/Vue 3 SPA
// ──────────────────────────────────────────────────────────────────────────────
var frontend = builder.AddViteApp("frontend", "../frontend")
    .WithHttpsDeveloperCertificate()
    .WithEnvironment("BROWSER", "none")
    .WithEndpoint("http", e => e.Port = 59874)
    .WithEnvironment("BFF_URL", bff.GetEndpoint("https"))
    .WithEnvironment("VITE_API_REFRESH_INTERVAL_MS", "20000")
    .WithReference(bff)
    .WaitFor(bff)
    ;

builder.Build().Run();
