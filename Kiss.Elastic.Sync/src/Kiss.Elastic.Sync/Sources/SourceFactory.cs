using Kiss.Elastic.Sync.Objecten;

namespace Kiss.Elastic.Sync.Sources
{
    public static class SourceFactory
    {
        public static IKissSourceClient CreateClient(string? source) => (source?.ToLowerInvariant()) switch
        {
            "vac" => GetVacClient(),
            "smoelenboek" or null => GetMedewerkerClient(),
            _ => GetProductClient(),
        };


        private static SdgProductClient GetProductClient()
        {
            var sdgBaseUrl = Helpers.GetEnvironmentVariable("SDG_BASE_URL");
            var sdgApiKey = Helpers.GetEnvironmentVariable("SDG_API_KEY");

            if (!Uri.TryCreate(sdgBaseUrl, UriKind.Absolute, out var sdgBaseUri))
            {
                throw new Exception("sdg base url is niet valide: " + sdgBaseUrl);
            }
            return new SdgProductClient(sdgBaseUri, sdgApiKey);
        }

        private static ObjectenMedewerkerClient GetMedewerkerClient()
        {
            var objectenBaseUrl = Helpers.GetEnvironmentVariable("MEDEWERKER_OBJECTEN_BASE_URL");
            var objectenToken = Helpers.GetEnvironmentVariable("MEDEWERKER_OBJECTEN_TOKEN");

            if (!Uri.TryCreate(objectenBaseUrl, UriKind.Absolute, out var objectenBaseUri))
            {
                throw new Exception("objecten base url is niet valide: " + objectenBaseUrl);
            }

            var objectTypesBaseUrl = Helpers.GetEnvironmentVariable("MEDEWERKER_OBJECTTYPES_BASE_URL");
            var objectTypesToken = Helpers.GetEnvironmentVariable("MEDEWERKER_OBJECTTYPES_TOKEN");

            if (!Uri.TryCreate(objectTypesBaseUrl, UriKind.Absolute, out var objectTypesBaseUri))
            {
                throw new Exception("objecttypes base url is niet valide: " + objectTypesBaseUrl);
            }

            var objecten = new ObjectenClient(objectenBaseUri, objectenToken);
            var types = new ObjectTypesClient(objectTypesBaseUri, objectTypesToken);

            return new ObjectenMedewerkerClient(objecten, types);
        }

        private static ObjectenVacClient GetVacClient()
        {
            var objectenBaseUrl = Helpers.GetEnvironmentVariable("VAC_OBJECTEN_BASE_URL");
            var objectenToken = Helpers.GetEnvironmentVariable("VAC_OBJECTEN_TOKEN");

            if (!Uri.TryCreate(objectenBaseUrl, UriKind.Absolute, out var objectenBaseUri))
            {
                throw new Exception("objecten base url is niet valide: " + objectenBaseUrl);
            }

            var objectTypesBaseUrl = Helpers.GetEnvironmentVariable("VAC_OBJECTTYPES_BASE_URL");
            var objectTypesToken = Helpers.GetEnvironmentVariable("VAC_OBJECTTYPES_TOKEN");

            if (!Uri.TryCreate(objectTypesBaseUrl, UriKind.Absolute, out var objectTypesBaseUri))
            {
                throw new Exception("objecttypes base url is niet valide: " + objectTypesBaseUrl);
            }

            var objecten = new ObjectenClient(objectenBaseUri, objectenToken);
            var types = new ObjectTypesClient(objectTypesBaseUri, objectTypesToken);

            return new ObjectenVacClient(objecten, types);
        }
    }
}
