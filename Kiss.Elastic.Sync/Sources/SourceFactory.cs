namespace Kiss.Elastic.Sync.Sources
{
	public static class SourceFactory
	{
		public static IKissSourceClient CreateClient(string? source, out string outputSource)
		{
			switch (source?.ToLowerInvariant())
			{
				case "smoelenboek":
					outputSource = "Smoelenboek";
					return GetMedewerkerClient();
				default:
					outputSource = "Kennisartikel";
					return GetProductClient();
			}
		}


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
			var objectenBaseUrl = Helpers.GetEnvironmentVariable("OBJECTEN_BASE_URL");
			var objectTypesBaseUrl = Helpers.GetEnvironmentVariable("OBJECTTYPES_BASE_URL");
			var objectenToken = Helpers.GetEnvironmentVariable("OBJECTEN_TOKEN");
			var objectTypesToken = Helpers.GetEnvironmentVariable("OBJECTTYPES_TOKEN");

			if (!Uri.TryCreate(objectenBaseUrl, UriKind.Absolute, out var objectenBaseUri))
			{
				throw new Exception("objecten base url is niet valide: " + objectenBaseUrl);
			}
			if (!Uri.TryCreate(objectTypesBaseUrl, UriKind.Absolute, out var objectTypesBaseUri))
			{
				throw new Exception("objecttypes base url is niet valide: " + objectTypesBaseUrl);
			}
			return new ObjectenMedewerkerClient(objectenBaseUri, objectenToken, objectTypesBaseUri, objectTypesToken);
		}
	}
}
