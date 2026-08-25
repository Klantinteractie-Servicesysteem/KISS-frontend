using Kiss.Elastic.Sync.KennisApi;

namespace Kiss.Elastic.Sync.Sources
{
    public static class SourceFactory
    {
        public static IKissSourceClient CreateClient(string[]? args) => (args?.FirstOrDefault()?.ToLowerInvariant()) switch
        {
            "vac" => GetVacClient(),
            "smoelenboek" => GetMedewerkerClient(),
            "sharepoint" => GetSharePointClient(),
            "kennis-api-artikel" => GetKennisApiKennisartikelClient(),
            "kennis-api-vac" => GetKennisApiVacClient(),
            _ => GetProductClient(),
        };

        private static SdgProductClient GetProductClient()
        {
            var sdgBaseUrl = Helpers.GetRequiredEnvironmentVariable("SDG_OBJECTEN_BASE_URL");
            var sdgApiKey = Helpers.GetOptionalEnvironmentVariable("SDG_OBJECTEN_TOKEN");
            var objectenClientId = Helpers.GetOptionalEnvironmentVariable("SDG_OBJECTEN_CLIENT_ID");
            var objectenClientSecret = Helpers.GetOptionalEnvironmentVariable("SDG_OBJECTEN_CLIENT_SECRET");
            var typeurl = Helpers.GetRequiredEnvironmentVariable("SDG_OBJECT_TYPE_URL");

            if (!Uri.TryCreate(sdgBaseUrl, UriKind.Absolute, out var sdgBaseUri))
            {
                throw new Exception("sdg base url is niet valide: " + sdgBaseUrl);
            }

            var objecten = new ObjectenClient(sdgBaseUri, sdgApiKey, objectenClientId, objectenClientSecret);

            return new SdgProductClient(objecten, typeurl);
        }

        private static ObjectenMedewerkerClient GetMedewerkerClient()
        {
            var objectenBaseUrl = Helpers.GetOptionalEnvironmentVariable("MEDEWERKER_OBJECTEN_BASE_URL");
            var objectenToken = Helpers.GetOptionalEnvironmentVariable("MEDEWERKER_OBJECTEN_TOKEN");
            var objectenClientId = Helpers.GetOptionalEnvironmentVariable("MEDEWERKER_OBJECTEN_CLIENT_ID");
            var objectenClientSecret = Helpers.GetOptionalEnvironmentVariable("MEDEWERKER_OBJECTEN_CLIENT_SECRET");
            var typeurl = Helpers.GetRequiredEnvironmentVariable("MEDEWERKER_OBJECT_TYPE_URL");

            if (!Uri.TryCreate(objectenBaseUrl, UriKind.Absolute, out var objectenBaseUri))
            {
                throw new Exception("objecten base url is niet valide: " + objectenBaseUrl);
            }

            var objecten = new ObjectenClient(objectenBaseUri, objectenToken, objectenClientId, objectenClientSecret);

            return new ObjectenMedewerkerClient(objecten, typeurl);
        }

        private static ObjectenVacClient GetVacClient()
        {
            var objectenBaseUrl = Helpers.GetRequiredEnvironmentVariable("VAC_OBJECTEN_BASE_URL");
            var objectenToken = Helpers.GetOptionalEnvironmentVariable("VAC_OBJECTEN_TOKEN");
            var objectenClientId = Helpers.GetOptionalEnvironmentVariable("VAC_OBJECTEN_CLIENT_ID");
            var objectenClientSecret = Helpers.GetOptionalEnvironmentVariable("VAC_OBJECTEN_CLIENT_SECRET");
            var typeurl = Helpers.GetRequiredEnvironmentVariable("VAC_OBJECT_TYPE_URL");

            if (!Uri.TryCreate(objectenBaseUrl, UriKind.Absolute, out var objectenBaseUri))
            {
                throw new Exception("objecten base url is niet valide: " + objectenBaseUrl);
            }

            var objecten = new ObjectenClient(objectenBaseUri, objectenToken, objectenClientId, objectenClientSecret);

            return new ObjectenVacClient(objecten, typeurl);
        }

        private static SharePointPageSourceClient GetSharePointClient()
        {
            var tenantId = Helpers.GetRequiredEnvironmentVariable("SHAREPOINT_TENANT_ID");
            var clientId = Helpers.GetRequiredEnvironmentVariable("SHAREPOINT_CLIENT_ID");
            var clientSecret = Helpers.GetRequiredEnvironmentVariable("SHAREPOINT_CLIENT_SECRET");
            var siteUrl = Helpers.GetRequiredEnvironmentVariable("SHAREPOINT_SITE_URL");
            var sourceName = Helpers.GetRequiredEnvironmentVariable("SHAREPOINT_SOURCE_NAME");

            var sharePointClient = new SharePoint.SharePointClient(tenantId, clientId, clientSecret, siteUrl);

            return new SharePointPageSourceClient(sharePointClient, sourceName);
        }

        private static KennisApiClient CreateKennisApiClient()
        {
            var baseUrl = Helpers.GetRequiredEnvironmentVariable("KENNISAPI_BASE_URL");
            var clientId = Helpers.GetRequiredEnvironmentVariable("KENNISAPI_CLIENT_ID");
            var clientSecret = Helpers.GetRequiredEnvironmentVariable("KENNISAPI_CLIENT_SECRET");
            var publicationId = Helpers.GetOptionalEnvironmentVariable("KENNISAPI_PUBLICATION_ID");

            return !Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri)
                ? throw new Exception("Kennis API base url is niet valide: " + baseUrl)
                : new KennisApiClient(baseUri, clientId, clientSecret, publicationId);
        }

        private static KennisApiKennisartikelClient GetKennisApiKennisartikelClient()
        {
            return new KennisApiKennisartikelClient(CreateKennisApiClient());
        }

        private static KennisApiVacClient GetKennisApiVacClient()
        {
            return new KennisApiVacClient(CreateKennisApiClient());
        }
    }
}
