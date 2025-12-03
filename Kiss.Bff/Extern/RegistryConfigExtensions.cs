namespace Kiss.Bff.Extern
{
    public static class RegistryConfigExtensions
    {
        public static IServiceCollection AddRegistryConfig(this IServiceCollection services, IConfiguration configuration)
        {
            var registries = GetRegistryConfiguration(configuration).ToList();

            // check if we have all the configuration we need
            var statusMessage = Validate(registries);

            if (!string.IsNullOrWhiteSpace(statusMessage))
            {
                throw new Exception(statusMessage);
            }

            var registryConfig = new RegistryConfig { Systemen = registries };

            services.AddSingleton(registryConfig);

            return services;
        }

        /// <summary>
        /// Haalt de registry-configuratie op uit de applicatie-instellingen.
        /// Verwacht een lijst onder <c>REGISTERS</c> met relevante API-sleutels en base URL's.
        /// Gooit een fout als verplichte velden ontbreken.
        /// </summary>
        private static IEnumerable<RegistrySystem> GetRegistryConfiguration(IConfiguration configuration)
        {
            var configs = configuration.GetSection("REGISTERS")?.Get<IEnumerable<Dictionary<string, string>>>() ?? [];

            var index = 0;
            foreach (var item in configs)
            {
                string? GetValue(string key) => item.TryGetValue(key, out var value) ? value : default;
                var registryVersion = Enum.TryParse<RegistryVersion>(GetValue("REGISTRY_VERSION"), out var versie)
                    ? versie
                    : RegistryVersion.OpenKlant2;

                var isDefault = bool.TryParse(GetValue("IS_DEFAULT"), out var defaultValue) && defaultValue;

                var useExperimentalQueries = bool.TryParse(GetValue("ZAAKSYSTEEM_USE_EXPERIMENTAL_QUERIES"), out var useExperimental)
                    && useExperimental;

                ZaaksysteemRegistry? zaaksysteem = null;

                // Er zijn twee mogelijke configuraties voor zaaksystemen.
                // - met 1 baseurl en de afonderlijke api's op paden (baseurl/zaken/api/v1, baseurl/catalogi/api/v1, etc)
                // - met een url per  afonderlijke api (zaken.zaaksysteem.nl, catalogi.zaaksysteem.nl, etc)

                var zaakysteemBaseUrl = GetValue("ZAAKSYSTEEM_BASE_URL");

                var zakenBaseUrl = GetValue("ZAAKSYSTEEM_ZAKEN_BASE_URL");
                var catalogiBaseUrl = GetValue("ZAAKSYSTEEM_CATALOGI_BASE_URL");
                var documentenBaseUrl = GetValue("ZAAKSYSTEEM_DOCUMENTEN_BASE_URL");


                var hasZakenBaseUrl = string.IsNullOrWhiteSpace(zakenBaseUrl);
                var hasCatalogiBaseUrl = string.IsNullOrWhiteSpace(catalogiBaseUrl);
                var hasDocumentenBaseUrl = string.IsNullOrWhiteSpace(documentenBaseUrl);

                if (!string.IsNullOrWhiteSpace(zaakysteemBaseUrl) && (!hasZakenBaseUrl || !hasCatalogiBaseUrl || !hasDocumentenBaseUrl))
                {
                    throw new Exception("Fout: ambigue zaaksysteem configuratie. gebruik of 1 baseurl voor alle zaaksysteem api's, of aparte baseurl's voor zaken, catalogi en documenten");
                }

                if ((hasZakenBaseUrl || hasCatalogiBaseUrl || hasDocumentenBaseUrl) && (!hasZakenBaseUrl || !hasCatalogiBaseUrl || !hasDocumentenBaseUrl))
                {
                    throw new Exception("Fout: als je een configuratie gebruikt met verschillende baseurl's voor de zaaksysteem api's, dan moeten er exact drie aanwezig zijn voor zaken, documente, en catalogi");
                }
                ;


                if (!string.IsNullOrWhiteSpace(zaakysteemBaseUrl))
                {
                    zaaksysteem = new ZaaksysteemRegistry
                    {
                        ClientSecret = GetValue("ZAAKSYSTEEM_API_KEY"),
                        ClientId = GetValue("ZAAKSYSTEEM_API_CLIENT_ID"),
                        DeeplinkUrl = GetValue("ZAAKSYSTEEM_DEEPLINK_URL"),
                        DeeplinkProperty = GetValue("ZAAKSYSTEEM_DEEPLINK_PROPERTY"),
                        ZakenBaseUrl = $"{zaakysteemBaseUrl.TrimEnd('/')}/zaken/api/v1",
                        CatalogiBaseUrl = $"{zaakysteemBaseUrl.TrimEnd('/')}/catalogi/api/v1",
                        DocumentenBaseUrl = $"{zaakysteemBaseUrl.TrimEnd('/')}/documenten/api/v1",
                        UseExperimentalQueries = useExperimentalQueries
                    };
                }


                if (!string.IsNullOrWhiteSpace(zakenBaseUrl) && !string.IsNullOrWhiteSpace(catalogiBaseUrl) && !string.IsNullOrWhiteSpace(documentenBaseUrl))
                {
                    zaaksysteem = new ZaaksysteemRegistry
                    {
                        ClientSecret = GetValue("ZAAKSYSTEEM_API_KEY"),
                        ClientId = GetValue("ZAAKSYSTEEM_API_CLIENT_ID"),
                        DeeplinkUrl = GetValue("ZAAKSYSTEEM_DEEPLINK_URL"),
                        DeeplinkProperty = GetValue("ZAAKSYSTEEM_DEEPLINK_PROPERTY"),
                        ZakenBaseUrl = zakenBaseUrl,
                        CatalogiBaseUrl = catalogiBaseUrl,
                        DocumentenBaseUrl = documentenBaseUrl,
                        UseExperimentalQueries = useExperimentalQueries
                    };
                }



                if (registryVersion == RegistryVersion.OpenKlant2)
                {
                    var klantinteractieBaseUrl = GetValue("KLANTINTERACTIE_BASE_URL") ?? throw new Exception($"Fout: REGISTERS__{index}__KLANTINTERACTIE_BASE_URL ontbreekt voor OpenKlant2 configuratie");


                    yield return new RegistrySystem
                    {
                        IsDefault = isDefault,
                        RegistryVersion = registryVersion,
                        Identifier = klantinteractieBaseUrl,
                        KlantinteractieRegistry = new KlantinteractieRegistry
                        {
                            BaseUrl = klantinteractieBaseUrl,
                            Token = GetValue("KLANTINTERACTIE_TOKEN")
                        },
                        ZaaksysteemRegistry = zaaksysteem,
                    };
                }
                else if (registryVersion == RegistryVersion.OpenKlant1)
                {
                    var contactmomentenBaseUrl = GetValue("CONTACTMOMENTEN_BASE_URL") ?? throw new Exception($"Fout: REGISTERS__{index}__CONTACTMOMENTEN_BASE_URL ontbreekt voor OpenKlant1 configuratie");
                    var interneTaakBaseUrl = GetValue("INTERNE_TAAK_BASE_URL");
                    var interneTaakObjectTypeUrl = GetValue("INTERNE_TAAK_OBJECT_TYPE_URL");

                    yield return new RegistrySystem
                    {
                        IsDefault = isDefault,
                        RegistryVersion = registryVersion,
                        Identifier = contactmomentenBaseUrl,
                        ContactmomentRegistry = new ContactmomentRegistry
                        {
                            BaseUrl = contactmomentenBaseUrl,
                            ClientId = GetValue("CONTACTMOMENTEN_API_CLIENT_ID"),
                            ClientSecret = GetValue("CONTACTMOMENTEN_API_KEY"),
                        },
                        InterneTaakRegistry = string.IsNullOrWhiteSpace(interneTaakBaseUrl) || string.IsNullOrWhiteSpace(interneTaakObjectTypeUrl)
                            ? null
                            : new InternetaakRegistry
                            {
                                BaseUrl = interneTaakBaseUrl,
                                ClientId = GetValue("INTERNE_TAAK_CLIENT_ID"),
                                ClientSecret = GetValue("INTERNE_TAAK_CLIENT_SECRET"),
                                Token = GetValue("INTERNE_TAAK_TOKEN"),
                                ObjectTypeUrl = interneTaakObjectTypeUrl,
                                ObjectTypeVersion = GetValue("INTERNE_TAAK_TYPE_VERSION") ?? "1"
                            },
                        KlantRegistry = new KlantRegistry
                        {
                            BaseUrl = GetValue("KLANTEN_BASE_URL"),
                            ClientId = GetValue("KLANTEN_CLIENT_ID"),
                            ClientSecret = GetValue("KLANTEN_CLIENT_SECRET"),
                        },
                        ZaaksysteemRegistry = zaaksysteem,
                    };
                }
                index++;
            }
        }


        /// <summary>
        /// Checks if the configuration of the registries is sufficient
        /// </summary>
        /// <param name="systemen"></param>
        /// <returns>An error message if applicable, or an empty string if all is well</returns>
        private static string Validate(IReadOnlyList<RegistrySystem> systemen)
        {
            if (!systemen.Any())
            {
                return "FOUT: Er zijn geen registraties geconfigureerd. Controleer of de configuratie correct is ingesteld. Voor OpenKlant2 moet ten minste een KlantinteractieRegistry aanwezig zijn. Voor OpenKlant1 moeten Contactmomenten, Klanten en Interne Taken correct geconfigureerd zijn.";
            }

            for (int i = 0; i < systemen.Count; i++)
            {
                var systeem = systemen[i];
                switch (systeem.RegistryVersion)
                {
                    case RegistryVersion.OpenKlant2:
                        if (string.IsNullOrWhiteSpace(systeem.KlantinteractieRegistry?.BaseUrl))
                        {
                            return $"FOUT: REGISTERS__{i}__KLANTINTERACTIE_BASE_URL ontbreekt voor OpenKlant2 configuratie.";
                        }
                        if (string.IsNullOrWhiteSpace(systeem.KlantinteractieRegistry?.Token))
                        {
                            return $"FOUT: REGISTERS__{i}__KLANTINTERACTIE_TOKEN ontbreekt voor OpenKlant2 configuratie.";
                        }
                        break;

                    case RegistryVersion.OpenKlant1:
                        if (string.IsNullOrWhiteSpace(systeem.ContactmomentRegistry?.BaseUrl))
                        {
                            return $"FOUT: REGISTERS__{i}__CONTACTMOMENTEN_BASE_URL ontbreekt voor OpenKlant1/eSuite configuratie.";
                        }
                        if (string.IsNullOrWhiteSpace(systeem.ContactmomentRegistry?.ClientId) ||
                            string.IsNullOrWhiteSpace(systeem.ContactmomentRegistry?.ClientSecret))
                        {
                            return $"FOUT: REGISTERS__{i}__CONTACTMOMENTEN_API_CLIENT_ID en/of REGISTERS__{i}__CONTACTMOMENTEN_API_KEY ontbreken voor OpenKlant1/eSuite configuratie.";
                        }

                        if (systeem.InterneTaakRegistry != null)
                        {
                            if (string.IsNullOrWhiteSpace(systeem.InterneTaakRegistry.BaseUrl))
                            {
                                return $"FOUT: REGISTERS__{i}__INTERNE_TAAK_BASE_URL ontbreekt voor OpenKlant1/eSuite configuratie.";
                            }
                            if (string.IsNullOrWhiteSpace(systeem.InterneTaakRegistry.ObjectTypeUrl))
                            {
                                return $"FOUT: REGISTERS__{i}__INTERNE_TAAK_OBJECT_TYPE_URL ontbreekt voor OpenKlant1/eSuite configuratie.";
                            }
                            bool heeftToken = !string.IsNullOrWhiteSpace(systeem.InterneTaakRegistry.Token);
                            bool heeftClientIdEnSecret =
                                !string.IsNullOrWhiteSpace(systeem.InterneTaakRegistry.ClientId) &&
                                !string.IsNullOrWhiteSpace(systeem.InterneTaakRegistry.ClientSecret);

                            if (!heeftToken && !heeftClientIdEnSecret)
                            {
                                return $"FOUT: REGISTERS__{i}__INTERNE_TAAK_TOKEN of REGISTERS__{i}__INTERNE_TAAK_CLIENT_ID/CLIENT_SECRET ontbreken voor OpenKlant1/eSuite configuratie.";
                            }
                        }

                        if (string.IsNullOrWhiteSpace(systeem.KlantRegistry?.BaseUrl))
                        {
                            return $"FOUT: REGISTERS__{i}__KLANTEN_BASE_URL ontbreekt voor OpenKlant1/eSuite configuratie.";
                        }
                        if (string.IsNullOrWhiteSpace(systeem.KlantRegistry?.ClientId) ||
                            string.IsNullOrWhiteSpace(systeem.KlantRegistry?.ClientSecret))
                        {
                            return $"FOUT: REGISTERS__{i}__KLANTEN_CLIENT_ID en/of REGISTERS__{i}__KLANTEN_CLIENT_SECRET ontbreken voor OpenKlant1/eSuite configuratie.";
                        }
                        break;
                }
            }

            var defaultRegistersCount = systemen.Count(r => r.IsDefault);

            return defaultRegistersCount switch
            {
                > 1 => "FOUT: Meerdere default registers ingesteld.",
                0 => "FOUT: Geen default register ingesteld.",
                _ => ""
            };
        }
    }
}
