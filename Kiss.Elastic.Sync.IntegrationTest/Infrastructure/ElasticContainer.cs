using Testcontainers.Elasticsearch;

namespace Kiss.Elastic.Sync.IntegrationTest.Infrastructure
{
    [CollectionDefinition(nameof(ElasticCollection))]
    public class ElasticCollection : ICollectionFixture<ElasticFixture>;

    public class ElasticFixture : IAsyncLifetime
    {
        private static readonly Dictionary<string, string> s_env = new()
        {
            ["xpack.security.enabled"] = "false"
        };

        private readonly ElasticsearchContainer _container = new ElasticsearchBuilder()
            .WithImage("elasticsearch:8.9.0")
            .WithEnvironment(s_env)
            .Build();

        public async Task DisposeAsync() => await _container.DisposeAsync();

        public async Task InitializeAsync() => await _container.StartAsync();

        public Uri BaseUri => new UriBuilder(
            Uri.UriSchemeHttp,
            _container.Hostname,
            _container.GetMappedPublicPort(ElasticsearchBuilder.ElasticsearchHttpsPort)
        ).Uri;
    }
}
