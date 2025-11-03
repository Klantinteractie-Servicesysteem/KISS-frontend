namespace Kiss.Elastic.Sync.Sources
{
    public static class SourceFactory
    {
        public static IKissSourceClient CreateClient(string? source) => (source?.ToLowerInvariant()) switch
        {
            "vac" => GetVacClient(),
            "smoelenboek" => GetMedewerkerClient(),
            "sharepoint" => GetSharePointClient(),
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
            var pageUrl = Helpers.GetRequiredEnvironmentVariable("SHAREPOINT_PAGE_URL");

            var sharePointClient = new SharePoint.SharePointClient(tenantId, clientId, clientSecret, siteUrl);

            return new SharePointPageSourceClient(sharePointClient, pageUrl);
        }
    }
}
