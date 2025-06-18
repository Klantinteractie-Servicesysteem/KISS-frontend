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

            foreach (var item in configs)
            {
                string? GetValue(string key) => item.TryGetValue(key, out var value) ? value : default;
                var registryVersion = Enum.TryParse<RegistryVersion>(GetValue("REGISTRY_VERSION"), out var versie)
                    ? versie
                    : RegistryVersion.OpenKlant2;

                var isDefault = bool.TryParse(GetValue("IS_DEFAULT"), out var defaultValue) && defaultValue;

                var zaakysteemBaseUrl = GetValue("ZAAKSYSTEEM_BASE_URL");
                var zaaksysteem = !string.IsNullOrWhiteSpace(zaakysteemBaseUrl)
                    ? new ZaaksysteemRegistry
                    {
                        BaseUrl = zaakysteemBaseUrl,
                        ClientSecret = GetValue("ZAAKSYSTEEM_API_KEY"),
                        ClientId = GetValue("ZAAKSYSTEEM_API_CLIENT_ID"),
                        DeeplinkUrl = GetValue("ZAAKSYSTEEM_DEEPLINK_URL"),
                        DeeplinkProperty = GetValue("ZAAKSYSTEEM_DEEPLINK_PROPERTY"),
                        ZakenBaseUrl = GetValue("ZAAKSYSTEEM_ZAKEN_BASE_URL"),
                        CatalogiBaseUrl = GetValue("ZAAKSYSTEEM_CATALOGI_BASE_URL"),
                        DocumentenBaseUrl = GetValue("ZAAKSYSTEEM_DOCUMENTEN_BASE_URL"),
                    }
                    : null;

                if (registryVersion == RegistryVersion.OpenKlant2)
                {
                    var klantinteractieBaseUrl = GetValue("KLANTINTERACTIE_BASE_URL") ?? throw new Exception("Fout: base url ontbreekt voor klantinteractie");


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
                    var contactmomentenBaseUrl = GetValue("CONTACTMOMENTEN_BASE_URL") ?? throw new Exception("Fout: base url ontbreekt voor contactmomenten");
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

            foreach (var systeem in systemen)
            {
                switch (systeem.RegistryVersion)
                {
                    case RegistryVersion.OpenKlant2:
                        if (string.IsNullOrWhiteSpace(systeem.KlantinteractieRegistry?.BaseUrl))
                        {
                            return "FOUT: Bij OpenKlant2 moet voor het KlantinteractieRegister een BaseUrl geconfigureerd worden.";
                        }
                        if (string.IsNullOrWhiteSpace(systeem.KlantinteractieRegistry?.Token))
                        {
                            return "FOUT: Bij OpenKlant2 moet voor het KlantinteractieRegister een Token geconfigureerd worden.";
                        }
                        break;

                    case RegistryVersion.OpenKlant1:
                        if (string.IsNullOrWhiteSpace(systeem.ContactmomentRegistry?.BaseUrl))
                        {
                            return "FOUT: Bij OpenKlant1/eSuite moet ContactmomentRegistry een BaseUrl hebben.";
                        }
                        if (string.IsNullOrWhiteSpace(systeem.ContactmomentRegistry?.ClientId) ||
                            string.IsNullOrWhiteSpace(systeem.ContactmomentRegistry?.ClientSecret))
                        {
                            return "FOUT: Bij OpenKlant1/eSuite moet ContactmomentRegistry een ClientId en ClientSecret hebben.";
                        }

                        if (systeem.InterneTaakRegistry != null)
                        {
                            if (string.IsNullOrWhiteSpace(systeem.InterneTaakRegistry.BaseUrl))
                            {
                                return "FOUT: Bij OpenKlant1/eSuite moet InterneTaakRegistry een BaseUrl hebben.";
                            }
                            if (string.IsNullOrWhiteSpace(systeem.InterneTaakRegistry.ObjectTypeUrl))
                            {
                                return "FOUT: Bij OpenKlant1/eSuite moet InterneTaakRegistry een ObjectTypeUrl hebben.";
                            }
                            bool heeftToken = !string.IsNullOrWhiteSpace(systeem.InterneTaakRegistry.Token);
                            bool heeftClientIdEnSecret =
                                !string.IsNullOrWhiteSpace(systeem.InterneTaakRegistry.ClientId) &&
                                !string.IsNullOrWhiteSpace(systeem.InterneTaakRegistry.ClientSecret);

                            if (!heeftToken && !heeftClientIdEnSecret)
                            {
                                return "FOUT: Bij OpenKlant1/eSuite moet InterneTaakRegistry óf een Token hebben, óf een ClientId en ClientSecret (voor eSuite).";
                            }
                        }

                        if (string.IsNullOrWhiteSpace(systeem.KlantRegistry?.BaseUrl))
                        {
                            return "FOUT: Bij OpenKlant1/eSuite moet KlantRegistry een BaseUrl hebben.";
                        }
                        if (string.IsNullOrWhiteSpace(systeem.KlantRegistry?.ClientId) ||
                            string.IsNullOrWhiteSpace(systeem.KlantRegistry?.ClientSecret))
                        {
                            return "FOUT: Bij OpenKlant1/eSuite moet KlantRegistry een ClientId en ClientSecret hebben.";
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
