# kiss-chart

![Version: 0.0.0](https://img.shields.io/badge/Version-0.0.0-informational?style=flat-square) ![Type: application](https://img.shields.io/badge/Type-application-informational?style=flat-square) ![AppVersion: 0.0.0](https://img.shields.io/badge/AppVersion-0.0.0-informational?style=flat-square)

A helm chart for Klantinteractie Service Systeem. The README of this chart is generated from the comments in values.yaml, so edit those instead of the README. Every KISS setting lists the environment variable it sets (Env), for installations without this chart.

## Values

### Afdelingen and groepen

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| settings.afdelingen.baseUrl | string | `""` | URL of the Objects API for afdelingen.<br/>Example: `https://objectenregister.mijngemeente.nl`<br/>Env: `AFDELINGEN_BASE_URL` |
| settings.afdelingen.objectTypeUrl | string | `""` | URL of the afdelingen Object Type.<br/>Example: `https://objecttypenregister.mijngemeente.nl/api/v2/objecttypes/f83fdc48-5ddb-4b1a-a347-e20092031399`<br/>Env: `AFDELINGEN_OBJECT_TYPE_URL` |
| settings.afdelingen.token | string | `""` | Token of the Objects API for afdelingen.<br/>Env: `AFDELINGEN_TOKEN` |
| settings.groepen.baseUrl | string | `""` | URL of the Objects API for groepen.<br/>Example: `https://objectenregister.mijngemeente.nl`<br/>Env: `GROEPEN_BASE_URL` |
| settings.groepen.objectTypeUrl | string | `""` | URL of the groepen Object Type.<br/>Example: `https://objecttypenregister.mijngemeente.nl/api/v2/objecttypes/f83fdc48-5ddb-4b1a-a347-e20092031399`<br/>Env: `GROEPEN_OBJECT_TYPE_URL` |
| settings.groepen.token | string | `""` | Token of the Objects API for groepen.<br/>Env: `GROEPEN_TOKEN` |

### ASP.NET Core

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| settings.aspnetcore.environment | string | `"Production"` | ASP.NET Core environment name.<br/>Env: `ASPNETCORE_ENVIRONMENT` |
| settings.aspnetcore.forwardedHeadersEnabled | bool | `true` | Process `X-Forwarded-*` headers set by the ingress / reverse proxy. Keep this `true` when KISS runs behind an ingress.<br/>Env: `ASPNETCORE_FORWARDEDHEADERS_ENABLED` |
| settings.aspnetcore.httpPorts | string | `""` | Port(s) the KISS container listens on. Leave this empty: the chart uses container port `8080` (also for the probes and the service), which is the default of the image. If you do set it, it must include `8080`, otherwise the probes fail.<br/>Env: `ASPNETCORE_HTTP_PORTS` |

### Database

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| settings.database.host | string | `""` | Host of the database used by KISS.<br/>Env: `POSTGRES_HOST` |
| settings.database.name | string | `""` | Name of the PostgreSQL database used by KISS.<br/>Env: `POSTGRES_DB` |
| settings.database.password | string | `""` | Password of the database user.<br/>Env: `POSTGRES_PASSWORD` |
| settings.database.port | int | `5432` | Port of the database used by KISS.<br/>Env: `POSTGRES_PORT` |
| settings.database.username | string | `""` | Username for KISS to access the database.<br/>Env: `POSTGRES_USER` |

### Elasticsearch

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| settings.elastic.baseUrl | string | `""` | The URL for Elasticsearch. Used by KISS and by the sync jobs.<br/>Example: `https://kiss-es-http:9200`<br/>Env: `ELASTIC_BASE_URL` |
| settings.elastic.bulkHttpTimeoutMinutes | int | `10` | The timeout in minutes for bulk indexing operations to Elasticsearch. Adjust this if you have a very large source and you experience timeouts during sync jobs.<br/>Env: `ELASTIC_BULK_HTTP_TIMEOUT_MINUTES` (only used by the sync jobs) |
| settings.elastic.excludedFieldsKennisbank | list | `[]` | Fields that a Kennisbank user is not allowed to search and view. Specify the field names as they are configured in Elasticsearch.<br/>Example: `["VAC.toelichting", "Kennisbank.vertalingen.deskMemo"]`<br/>Env: `ELASTIC_EXCLUDED_FIELDS_KENNISBANK` (comma separated) |
| settings.elastic.password | string | `""` | Password to log in to Elasticsearch.<br/>Env: `ELASTIC_PASSWORD` <details><summary>More information</summary>If you use [ECK](https://www.elastic.co/guide/en/cloud-on-k8s/current/k8s-overview.html), you can find the password of the default user with `kubectl get secret kiss-es-elastic-user -o go-template='{{.data.elastic \| base64decode}}'`</details> |
| settings.elastic.sslVerify | bool | `false` | Controls whether SSL certificate verification is enabled. Defaults to false because elasticsearch clusters are often internal and use self-signed certificates.<br/>Only used by the website crawler and the index template hook, there is no environment variable for this setting. |
| settings.elastic.username | string | `""` | Username to log in to Elasticsearch. This can be the default root user `elastic`, or a user you created yourself.<br/>Env: `ELASTIC_USERNAME` |

### Feedback on kennisartikelen

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| settings.email.enableSsl | bool | `true` | Use SSL for the connection with the mail server.<br/>Env: `EMAIL_ENABLE_SSL` |
| settings.email.host | string | `""` | Address of the mail server.<br/>Env: `EMAIL_HOST` |
| settings.email.password | string | `""` | Password for the mail server.<br/>Env: `EMAIL_PASSWORD` |
| settings.email.port | int | `25` | Port number of the mail connection.<br/>Env: `EMAIL_PORT` |
| settings.email.username | string | `""` | Username for the mail server.<br/>Env: `EMAIL_USERNAME` |
| settings.feedback.emailFrom | string | `""` | From address of the feedback email. A KCM can give feedback on a kennisartikel from KISS; this feedback is emailed to one central address.<br/>Env: `FEEDBACK_EMAIL_FROM` |
| settings.feedback.emailTo | string | `""` | Address where the feedback email should be sent.<br/>Env: `FEEDBACK_EMAIL_TO` |

### Feature switches

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| settings.groepsmailboxVerplichting | bool | `false` | Determines whether a medewerker must be linked to a contactverzoek when the selected afdeling or groep has no groepsmailbox.<br/>Env: `USE_GROEPSMAILBOX_VERPLICHTING` <details><summary>More information</summary>When `true`, the medewerker field becomes required (with a visible reason) if no email address is known for the selected afdeling or groep. A contactverzoek can then not be created without a medewerker. When `false`, the medewerker field stays optional, regardless of the groepsmailbox.</details> |
| settings.syncJobs.medewerkers.useEmail | bool | `false` | Determines whether a contactverzoek for a medewerker can only be made by email address.<br/>Env: `USE_MEDEWERKEREMAIL` <details><summary>More information</summary>When `true`, the contactverzoek form shows a field to enter the email address of a medewerker under the selected afdeling or groep. Creating a contactverzoek directly for a medewerker is then not possible. The email field is not required. When `false`, a medewerker can be picked by name from a list (the Smoelenboek).</details> |
| settings.syncJobs.vac.manageFromKiss | bool | `false` | Determines whether the navigation item for managing VACs is present in the management navigation.<br/>**Note**: when `true`, the `token` for VACs also needs write permissions, and `objectTypeVersion` must be set to the correct value.<br/>Env: `USE_VACS` |

### BRP (Haal Centraal) and KvK

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| settings.haalCentraal.apiKey | string | `""` | Key for the Haal Centraal API.<br/>Env: `HAAL_CENTRAAL_API_KEY` |
| settings.haalCentraal.baseUrl | string | `""` | URL of the Haal Centraal BRP Personen API.<br/>Example: `https://proefomgeving.haalcentraal.nl/haalcentraal/api`<br/>Env: `HAAL_CENTRAAL_BASE_URL` |
| settings.haalCentraal.customHeaders | object | `{}` | Custom headers to send to the Haal Centraal API, for instance when using a proxy such as iConnect. A value can be a string (sent with every search) or an object with a value per search type: `RaadpleegMetBurgerservicenummer`, `ZoekMetGeslachtsnaamEnGeboortedatum` or `ZoekMetPostcodeEnHuisnummer`.<br/>Env: `HAAL_CENTRAAL_CUSTOM_HEADERS__<headername>` and `HAAL_CENTRAAL_CUSTOM_HEADERS__<headername>__<zoektype>` <details><summary>Using KISS with a proxy for BRP / KvK (for example iConnect)</summary>Proxies for the BRP and KvK often require certain headers. To pass the identification of the logged in user in a header, use `userHeaderName` (here and in `settings.kvk`); the value sent is the claim configured in `settings.oidc.medewerkerIdentificatie.claim`. To pass static information, use `customHeaders` (here and in `settings.kvk`). For the BRP, a header can have a different value per search type. Example for iConnect: <pre>settings:<br/>  haalCentraal:<br/>    userHeaderName: x-gebruiker<br/>    customHeaders:<br/>      x-verwerking: Burgerzaken<br/>      x-doelbinding:<br/>        RaadpleegMetBurgerservicenummer: BRPACT-Totaal<br/>        ZoekMetGeslachtsnaamEnGeboortedatum: BRPACT-ZoekenAlgemeen<br/>        ZoekMetPostcodeEnHuisnummer: BRPACT-ZoekenAlgemeen<br/>  kvk:<br/>    customHeaders:<br/>      x-origin-oin: "00000000000000000000"</pre> The same example as environment variables: <pre>HAAL_CENTRAAL_USER_HEADER_NAME=x-gebruiker<br/>HAAL_CENTRAAL_CUSTOM_HEADERS__x-verwerking=Burgerzaken<br/>HAAL_CENTRAAL_CUSTOM_HEADERS__x-doelbinding__RaadpleegMetBurgerservicenummer=BRPACT-Totaal<br/>HAAL_CENTRAAL_CUSTOM_HEADERS__x-doelbinding__ZoekMetGeslachtsnaamEnGeboortedatum=BRPACT-ZoekenAlgemeen<br/>HAAL_CENTRAAL_CUSTOM_HEADERS__x-doelbinding__ZoekMetPostcodeEnHuisnummer=BRPACT-ZoekenAlgemeen<br/>KVK_CUSTOM_HEADERS__x-origin-oin=00000000000000000000</pre> Without helm, a header without search type (e.g. `HAAL_CENTRAAL_CUSTOM_HEADERS__x-doelbinding=BRPACT-ZoekenAlgemeen`) can be combined with a value for one specific search type; the specific value takes precedence for that search type.</details> |
| settings.haalCentraal.userHeaderName | string | `""` | Header name to include the user id to send to the Haal Centraal API, for instance when using a proxy. The value of the claim configured in `settings.oidc.medewerkerIdentificatie.claim` is sent. See `settings.haalCentraal.customHeaders` for an example.<br/>Env: `HAAL_CENTRAAL_USER_HEADER_NAME` |
| settings.kvk.apiKey | string | `""` | Key for the KvK API.<br/>Env: `KVK_API_KEY` |
| settings.kvk.baseUrl | string | `""` | URL of the KvK API to query the Handelsregister. This is the path before the version number.<br/>Example: `https://api.kvk.nl/test/api`<br/>Env: `KVK_BASE_URL` |
| settings.kvk.customHeaders | object | `{}` | Custom headers to send to the KvK API, for instance when using a proxy. See `settings.haalCentraal.customHeaders` for an example.<br/>Env: `KVK_CUSTOM_HEADERS__<headername>` |
| settings.kvk.userHeaderName | string | `""` | Header name to include the user id to send to the KvK API, for instance when using a proxy. The value of the claim configured in `settings.oidc.medewerkerIdentificatie.claim` is sent.<br/>Env: `KVK_USER_HEADER_NAME` |

### Logboek (ITA)

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| settings.logboek.baseUrl | string | `""` | URL of the Objects API where the logboek is stored.<br/>Env: `LOGBOEK_BASE_URL` <details><summary>About the logboek</summary>The logboek contains information about the handling of a contactverzoek in ITA (Interne Taak Afhandeling). KISS shows this information with the contactverzoeken of a klant. Functionally, the logboek is only used together with ITA. The chart's json schema currently requires `baseUrl`, `objectTypeUrl`, `objectTypeVersion` and `token` to be filled in. KISS itself treats the logboek as optional: only when all four `LOGBOEK_*` environment variables have a value, the logboek is shown.</details> |
| settings.logboek.objectTypeUrl | string | `""` | URL of the Logboek Object Type.<br/>Example: `https://objecttypenregister.mijngemeente.nl/api/v2/objecttypes/12345678-5ddb-4b1a-a347-e20092031399`<br/>Env: `LOGBOEK_OBJECT_TYPE_URL` |
| settings.logboek.objectTypeVersion | int | `1` | Version number of the Logboek Object Type.<br/>Env: `LOGBOEK_OBJECT_TYPE_VERSION` |
| settings.logboek.token | string | `""` | Token for the Objects API for the Logboek.<br/>Env: `LOGBOEK_TOKEN` |

### Authentication and authorization

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| settings.managementInformatie.apiKey | string | `""` | Secret that KISS uses to validate the JWT Token when requesting contactmoment details. See the [managementinformatie chapter of the KISS manual](https://kiss-klantinteractie-servicesysteem.readthedocs.io/nl/stable/manual/managementinformatie.html) on how to use this API.<br/>Env: `MANAGEMENTINFORMATIE_API_KEY` |
| settings.oidc.authority | string | `""` | URL of the OpenID Connect Identity Provider.<br/>Env: `OIDC_AUTHORITY` |
| settings.oidc.beheerderRole | string | `""` | Name of the role for a Beheerder. Must match the name of the role in the Identity Provider. Default: `Beheerder`. A user with this role can edit Skills, Kanalen, Gespreksresultaten and Contactverzoek formulieren. See the [KISS manual](https://kiss-klantinteractie-servicesysteem.readthedocs.io/nl/stable/manual/manual.html).<br/>When PABC (`settings.pabc`) is configured, the role comes from PABC, but KISS only accepts a PABC role with this name. Leave this empty (or use the default) when using PABC.<br/>Env: `OIDC_BEHEERDER_ROLE` |
| settings.oidc.clientId | string | `""` | Client ID for access to the OpenID Connect Identity Provider.<br/>Env: `OIDC_CLIENT_ID` |
| settings.oidc.clientSecret | string | `""` | Secret for the OpenID Connect Identity Provider.<br/>Env: `OIDC_CLIENT_SECRET` |
| settings.oidc.kennisbankRole | string | `""` | Name of the role for a Kennisbank employee. Must match the name of the role in the Identity Provider. Default: `Kennisbank`. A user with this role can only search the configured sources (Kennisbank, VAC, Smoelenboek, websites).<br/>When PABC (`settings.pabc`) is configured, the role comes from PABC, but KISS only accepts a PABC role with this name. Leave this empty (or use the default) when using PABC.<br/>Env: `OIDC_KENNISBANK_ROLE` |
| settings.oidc.klantcontactmedewerkerRole | string | `""` | Name of the role for a Klantcontactmedewerker (KCM). Must match the name of the role in the Identity Provider. Default: `Klantcontactmedewerker`. A user with this role can register contactmomenten and contactverzoeken, and search the sources.<br/>When PABC (`settings.pabc`) is configured, the role comes from PABC, but KISS only accepts a PABC role with this name. Leave this empty (or use the default) when using PABC.<br/>Env: `OIDC_KLANTCONTACTMEDEWERKER_ROLE` |
| settings.oidc.medewerkerIdentificatie.claim | string | `""` | The claim used to identify the medewerker in the registers. Default: `email`.<br/>Env: `OIDC_MEDEWERKER_IDENTIFICATIE_CLAIM` <details><summary>More information</summary>When writing data to e.g. Open Klant or Open Zaak, a `medewerkerIdentificatie.identificatie` is required. Municipalities use different values for this. When integrating with e.g. the e-Suite, this must contain the e-Suite username of the logged in KCM.</details> |
| settings.oidc.medewerkerIdentificatie.truncate | int,null | `nil` | Optional maximum number of characters of the claim, after which it is truncated. Must be a number (not quoted), for example `24`. Default: no truncation.<br/>Env: `OIDC_MEDEWERKER_IDENTIFICATIE_TRUNCATE` <details><summary>More information</summary>Within ZGW, a `medewerkerIdentificatie.identificatie` may not be longer than 24 characters. Use this setting to truncate the value after 24 characters.</details> |
| settings.oidc.redacteurRole | string | `""` | Name of the role for a Redacteur. Must match the name of the role in the Identity Provider. Default: `Redacteur`. A user with this role can edit Nieuws- en Werkberichten, Links and VACs (if configured). See the [KISS manual](https://kiss-klantinteractie-servicesysteem.readthedocs.io/nl/stable/manual/manual.html).<br/>When PABC (`settings.pabc`) is configured, the role comes from PABC, but KISS only accepts a PABC role with this name. Leave this empty (or use the default) when using PABC.<br/>Env: `OIDC_REDACTEUR_ROLE` |

### Organisation

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| settings.organisatieIds | list | `[]` | RSIN of the organisation that registers the contactmomenten. Several ZGW APIs, including the Klanten and Contactmomenten APIs, require the RSIN of the own organisation when registering specific objects.<br/>Env: `ORGANISATIE_IDS` (separated by `/`) |

### PABC

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| settings.pabc.apiKey | string | `""` | API key for authentication with PABC (sent as `X-API-KEY` header).<br/>Env: `PABC_API_KEY` |
| settings.pabc.baseUrl | string | `""` | Base URL of the PABC API, without trailing slash. Example: `https://pabc.mijngemeente.nl`.<br/>Setting both `baseUrl` and `apiKey` activates the PABC integration.<br/>Env: `PABC_BASE_URL` <details><summary>About PABC (Platform Autorisatie Beheer Component)</summary>The integration with <a href="https://github.com/Platform-Autorisatie-Beheer-Component/PABC-API">PABC</a> is optional. When both `baseUrl` and `apiKey` are set, the integration is activated. The application name `kiss` is hardcoded in KISS.<br/><br/>When PABC is active, the roles of a user come from PABC instead of the role claims of the Identity Provider. KISS only accepts PABC application roles whose names match the role names in `settings.oidc` (`klantcontactmedewerkerRole`, `redacteurRole`, `beheerderRole`, `kennisbankRole`). Leave those settings empty (KISS then uses the default names `Klantcontactmedewerker`, `Redacteur`, `Beheerder` and `Kennisbank`), or make sure the application roles in PABC have exactly the configured names.<br/><br/>Set up PABC first, and only then deploy KISS with the PABC configuration. Otherwise no user gets a KISS application role. See the <a href="https://kiss-klantinteractie-servicesysteem.readthedocs.io/nl/stable/installation/pabc.html">PABC documentation</a> for setup instructions.</details> |

### Registers

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| settings.registers | list | `[]` | Sets of registers for klantcontacten and zaken. Each set consists of a klantregister, contactmomentregister, contactverzoekregister (interne taak) and a zaaksysteem that belong together. The file `values.schema.json` of this chart describes the possible configurations.<br/>Env: `REGISTERS__<n>__…`, where the first item gets `__0__`, the second `__1__`, and so on. <details><summary>How the sets of registers work</summary><ul> <li>There must be <b>exactly one</b> set with `isDefault: true`. Klanten, contactmomenten and contactverzoeken are created in the default set, unless the context shows that the data belongs in a specific register. For example: if a contactmoment is about a zaak from a certain zaaksysteem, the contactmoment is stored in the contactmomentregister of the same set as that zaaksysteem.</li> <li>A set uses either <b>Open Klant 2</b> (`klantinteractie`) or the <b>e-Suite / Open Klant 1</b> (`contactmomenten`, `klanten` and `interneTaak`). Use Open Klant 1 only to integrate with the e-Suite. The chart derives `REGISTERS__<n>__REGISTRY_VERSION` (`OpenKlant2` or `OpenKlant1`) from this. Without helm, set it yourself: when it is not set, KISS assumes `OpenKlant2`.</li> <li>Secrets marked <b>min. 32 characters</b> below must be at least 32 characters long.</li> </ul></details> <details><summary>Fields of each set</summary><ul> <li>`isDefault` (Env: `REGISTERS__<n>__IS_DEFAULT`): `true` for exactly one set.</li> <li>`zaaksysteem.zakenBaseUrl` (Env: `REGISTERS__<n>__ZAAKSYSTEEM_ZAKEN_BASE_URL`): full base URL of the Zaken API, including the version. Example: `https://zaken.somezaaksysteem.nl/api/v1`</li> <li>`zaaksysteem.catalogiBaseUrl` (Env: `REGISTERS__<n>__ZAAKSYSTEEM_CATALOGI_BASE_URL`): full base URL of the Catalogi API, including the version. Example: `https://catalogi.somezaaksysteem.nl/api/v1`</li> <li>`zaaksysteem.documentenBaseUrl` (Env: `REGISTERS__<n>__ZAAKSYSTEEM_DOCUMENTEN_BASE_URL`): full base URL of the Documenten API, including the version. Example: `https://documenten.somezaaksysteem.nl/api/v1`</li> <li>`zaaksysteem.clientId` (Env: `REGISTERS__<n>__ZAAKSYSTEEM_API_CLIENT_ID`): client ID for the ZGW APIs.</li> <li>`zaaksysteem.clientSecret` (Env: `REGISTERS__<n>__ZAAKSYSTEEM_API_KEY`): secret for the ZGW APIs <b>(min. 32 characters)</b>.</li> <li>`zaaksysteem.deeplink.url` (Env: `REGISTERS__<n>__ZAAKSYSTEEM_DEEPLINK_URL`): optional. Base URL to deeplink to a zaak in the zaaksysteem, for example `https://zaaksysteem.mijngemeente.nl/mp/zaak/`. Always use this <b>in combination with</b> `deeplink.property`. When both are set, the zaak details screen in KISS shows a link that opens the zaak in the zaaksysteem. This only works for zaaksystemen with a fixed URL for zaak details, where only one property of the zaak (e.g. the zaaknummer) has to be appended to that URL.</li> <li>`zaaksysteem.deeplink.property` (Env: `REGISTERS__<n>__ZAAKSYSTEEM_DEEPLINK_PROPERTY`): optional. The property of the zaak whose value is appended to `deeplink.url`, for example `identificatie`. Always use this <b>in combination with</b> `deeplink.url`.</li> <li>`zaaksysteem.useExperimentalQueries` (Env: `REGISTERS__<n>__ZAAKSYSTEEM_USE_EXPERIMENTAL_QUERIES`): optional, `true` or `false`. Feature switch for the experimental features introduced in <a href="https://open-zaak.readthedocs.io/en/stable/development/changelog.html#id9">Open Zaak 1.20.0</a>.</li> <li>`zaaksysteem.usePabc` (Env: `REGISTERS__<n>__ZAAKSYSTEEM_USE_PABC`): optional, default `true`. Whether PABC zaaktype filtering (see `settings.pabc`) is applied to this zaaksysteem. Set to `false` for zaaksystemen with their own authorization (e.g. the e-Suite).</li> <li><b>Open Klant 2:</b></li> <li>`klantinteractie.baseUrl` (Env: `REGISTERS__<n>__KLANTINTERACTIE_BASE_URL`): URL of the Klantinteracties API of the Open Klant 2 register, <b>without</b> `/api/v1` (KISS adds that itself). Example: `https://klantinteractieregister.mijngemeente.nl/klantinteracties`</li> <li>`klantinteractie.token` (Env: `REGISTERS__<n>__KLANTINTERACTIE_TOKEN`): token for the Klantinteracties API.</li> <li><b>e-Suite / Open Klant 1:</b></li> <li>`contactmomenten.baseUrl` (Env: `REGISTERS__<n>__CONTACTMOMENTEN_BASE_URL`): URL of the Contactmomenten API. Example: `https://contactmomentenregister.mijngemeente.nl`</li> <li>`contactmomenten.clientId` (Env: `REGISTERS__<n>__CONTACTMOMENTEN_API_CLIENT_ID`): client ID for the Contactmomenten API.</li> <li>`contactmomenten.clientSecret` (Env: `REGISTERS__<n>__CONTACTMOMENTEN_API_KEY`): key for the Contactmomenten API <b>(min. 32 characters)</b>.</li> <li>`klanten.baseUrl` (Env: `REGISTERS__<n>__KLANTEN_BASE_URL`): URL of the Klanten API. Example: `https://klantenregister.mijngemeente.nl/klanten`</li> <li>`klanten.clientId` (Env: `REGISTERS__<n>__KLANTEN_CLIENT_ID`): client ID for the Klanten API.</li> <li>`klanten.clientSecret` (Env: `REGISTERS__<n>__KLANTEN_CLIENT_SECRET`): secret for the Klanten API <b>(min. 32 characters)</b>.</li> <li>`interneTaak.baseUrl` (Env: `REGISTERS__<n>__INTERNE_TAAK_BASE_URL`): URL of the Objects API for Interne Taken. Example: `https://objectenregister.mijngemeente.nl`</li> <li>`interneTaak.clientId` (Env: `REGISTERS__<n>__INTERNE_TAAK_CLIENT_ID`): client ID for the Objects API for Interne Taken. Usually a token is used; a client ID and secret are needed for example with the e-Suite (podiumd-adapter). Use either a client ID and secret, or a token (`REGISTERS__<n>__INTERNE_TAAK_TOKEN`, see `extraEnvVars`), not both. If both are set, KISS uses the client ID and secret.</li> <li>`interneTaak.clientSecret` (Env: `REGISTERS__<n>__INTERNE_TAAK_CLIENT_SECRET`): client secret for the Objects API for Interne Taken.</li> <li>`interneTaak.objectTypeUrl` (Env: `REGISTERS__<n>__INTERNE_TAAK_OBJECT_TYPE_URL`): URL of the Interne Taak Object Type. Example: `https://objecttypenregister.mijngemeente.nl/api/v2/objecttypes/f83fdc48-5ddb-4b1a-a347-e20092031399`</li> <li>`interneTaak.objectTypeVersion` (Env: `REGISTERS__<n>__INTERNE_TAAK_TYPE_VERSION`): version number of the Interne Taak Object Type, for example `2`. KISS writes Interne Taken to the Objects API and must send the version of the object type. Which version is the most recent can differ per municipality.</li> </ul></details> <details><summary>Example (Open Klant 2 and Open Zaak)</summary><pre>settings:<br/>  registers:<br/>    - isDefault: true<br/>      klantinteractie:<br/>        baseUrl: "https://klantinteractieregister.mijngemeente.nl/klantinteracties"<br/>        token: "..."<br/>      zaaksysteem:<br/>        zakenBaseUrl: "https://openzaak.mijngemeente.nl/zaken/api/v1"<br/>        catalogiBaseUrl: "https://openzaak.mijngemeente.nl/catalogi/api/v1"<br/>        documentenBaseUrl: "https://openzaak.mijngemeente.nl/documenten/api/v1"<br/>        clientId: "..."<br/>        clientSecret: "..."</pre></details> <details><summary>Migrating from the deprecated ZAAKSYSTEEM_BASE_URL</summary>Older versions of KISS used one variable, `REGISTERS__<n>__ZAAKSYSTEEM_BASE_URL`, for all ZGW APIs. This chart only supports the three separate URLs. KISS itself still accepts the old variable, but don't use the old and new format together. To migrate, replace the single URL by the three base URLs, <b>including the version number</b>. If every API runs on its own subdomain, use for example `https://zaken.mijnzaaksysteem.nl/api/v1`, `https://catalogi.mijnzaaksysteem.nl/api/v1` and `https://documenten.mijnzaaksysteem.nl/api/v1`. If all APIs run on the same URL, use for example `https://openzaak.mijngemeente.nl/zaken/api/v1`, `https://openzaak.mijngemeente.nl/catalogi/api/v1` and `https://openzaak.mijngemeente.nl/documenten/api/v1`.</details> |

### Sync jobs

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| settings.syncJobs.crawlerImage.pullPolicy | string | `"IfNotPresent"` | Pull policy for the Open Crawler image |
| settings.syncJobs.crawlerImage.repository | string | `"docker.elastic.co/integrations/crawler"` | The image repository for the Elastic Open Crawler |
| settings.syncJobs.crawlerImage.tag | string | `"1.0.0"` | The tag for the Open Crawler image |
| settings.syncJobs.image.pullPolicy | string | `"IfNotPresent"` | Pull policy for the `kiss-elastic-sync` image |
| settings.syncJobs.image.repository | string | `"ghcr.io/klantinteractie-servicesysteem/kiss-elastic-sync"` | The image repository for the `kiss-elastic-sync` sync jobs |
| settings.syncJobs.image.tag | string | `""` | The tag for the `kiss-elastic-sync` image to use for sync jobs. Consider leaving this empty because it will default to the recommended version to use with the chart. |
| settings.syncJobs.indexTemplateImage.pullPolicy | string | `"IfNotPresent"` | Pull policy for the index-template hook image |
| settings.syncJobs.indexTemplateImage.repository | string | `"curlimages/curl"` | Image used by the pre-install/pre-upgrade hook that registers the `search-website*` index template in Elasticsearch (needs curl). |
| settings.syncJobs.indexTemplateImage.tag | string | `"8.20.0"` | The tag for the index-template hook image |
| settings.syncJobs.kennisbank.baseUrl | string | `""` | URL of the Objects API for Kennisartikelen (PDC producten).<br/>Example: `https://objectenregister.mijngemeente.nl`<br/>Env: `SDG_OBJECTEN_BASE_URL` |
| settings.syncJobs.kennisbank.historyLimit | int | `1` | Number of finished jobs to keep. |
| settings.syncJobs.kennisbank.objectTypeUrl | string | `""` | URL of the Kennisartikel Object Type.<br/>Example: `https://objecttypenregister.mijngemeente.nl/api/v2/objecttypes/f83fdc48-5ddb-4b1a-a347-e20092031399`<br/>Env: `SDG_OBJECT_TYPE_URL` |
| settings.syncJobs.kennisbank.resources | object | `{}` | Resources of the sync job container. |
| settings.syncJobs.kennisbank.schedule | string | `"*/59 * * * *"` | Cron schedule of the sync job. |
| settings.syncJobs.kennisbank.token | string | `""` | Token for the Objects API for Kennisartikelen.<br/>Env: `SDG_OBJECTEN_TOKEN` |
| settings.syncJobs.medewerkers.baseUrl | string | `""` | URL of the Objects API for medewerkers.<br/>Example: `https://objectenregister.mijngemeente.nl`<br/>Env: `MEDEWERKER_OBJECTEN_BASE_URL` |
| settings.syncJobs.medewerkers.clientId | string | `""` | Client ID for the Objects API for medewerkers. Leave `token` empty if you use a Client ID and Client Secret. Use this if you're connecting with the podiumd-adapter.<br/>Env: `MEDEWERKER_OBJECTEN_CLIENT_ID` |
| settings.syncJobs.medewerkers.clientSecret | string | `""` | Client Secret for the Objects API for medewerkers. Leave `token` empty if you use a Client ID and Client Secret. Use this if you're connecting with the podiumd-adapter.<br/>Env: `MEDEWERKER_OBJECTEN_CLIENT_SECRET` |
| settings.syncJobs.medewerkers.historyLimit | int | `1` | Number of finished jobs to keep. |
| settings.syncJobs.medewerkers.objectTypeUrl | string | `""` | URL of the Medewerker Object Type.<br/>Example: `https://objecttypenregister.mijngemeente.nl/api/v2/objecttypes/f83fdc48-5ddb-4b1a-a347-e20092031399`<br/>Env: `MEDEWERKER_OBJECT_TYPE_URL` |
| settings.syncJobs.medewerkers.resources | object | `{}` | Resources of the sync job container. |
| settings.syncJobs.medewerkers.schedule | string | `"*/59 * * * *"` | Cron schedule of the sync job. |
| settings.syncJobs.medewerkers.token | string | `""` | Token for the Objects API for medewerkers (Smoelenboek). Leave `clientId` and `clientSecret` empty if you use a token, never use both (if both are set, the client ID and secret are used). Don't use this if you're connecting with the podiumd-adapter.<br/>Env: `MEDEWERKER_OBJECTEN_TOKEN` |
| settings.syncJobs.sharepoint | list | `[]` | SharePoint sync jobs configuration. One sync job is created per item. <details><summary>Fields of each item</summary><ul> <li>`sourceName` (Env: `SHAREPOINT_SOURCE_NAME`): <b>required.</b> A human friendly name for the source, as shown in the search bar in KISS. Example: <i>ICATT SharePoint Test Site</i>.</li> <li>`siteUrl` (Env: `SHAREPOINT_SITE_URL`): <b>required.</b> Full URL of the SharePoint site to crawl, for example `https://my-tenant.sharepoint.com/sites/my-site`. If this is a hub site, or the site has subsites, all underlying / related sites are indexed as well. Both 'modern' site pages and traditional wiki pages are supported.</li> <li>`clientId` (Env: `SHAREPOINT_CLIENT_ID`): <b>required.</b> Client ID (application ID) of the SharePoint app registration. The app registration needs the API permission `Sites.Read.All`. Example: `0166786a-df05-4e1c-a75c-7c3317002ad8`</li> <li>`clientSecret` (Env: `SHAREPOINT_CLIENT_SECRET`): <b>required.</b> Client secret of the SharePoint app registration.</li> <li>`tenantId` (Env: `SHAREPOINT_TENANT_ID`): <b>required.</b> Azure AD tenant ID (GUID) of the SharePoint app registration. Example: `bd7f67dd-fc8c-4a9d-a1aa-c2e4d687b18f`</li> <li>`schedule`: cron schedule of the sync job. Default `*/59 * * * *`</li> <li>`historyLimit`: number of finished jobs to keep. Default `1`</li> <li>`resources`: resources of the sync job container.</li> </ul><b>Reserved source names:</b> don't use `Smoelenboek`, `VAC` or `Kennisbank` as `sourceName`, unless you don't use the corresponding source (a source is not used when it has no configuration). `Smoelenboek` belongs to `settings.syncJobs.medewerkers`, `VAC` to `settings.syncJobs.vac` and `Kennisbank` to `settings.syncJobs.kennisbank`.</details> |
| settings.syncJobs.vac.baseUrl | string | `""` | URL of the Objects API for VACs.<br/>Example: `https://objectenregister.mijngemeente.nl`<br/>Env: `VAC_OBJECTEN_BASE_URL` |
| settings.syncJobs.vac.historyLimit | int | `1` | Number of finished jobs to keep. |
| settings.syncJobs.vac.objectTypeUrl | string | `""` | URL of the VAC Object Type.<br/>Example: `https://objecttypenregister.mijngemeente.nl/api/v2/objecttypes/f83fdc48-5ddb-4b1a-a347-e20092031399`<br/>Env: `VAC_OBJECT_TYPE_URL` |
| settings.syncJobs.vac.objectTypeVersion | int | `1` | Version number of the VAC Object Type. When `manageFromKiss` is `true`, KISS writes VACs to the Objects API and must send the version of the object type. Which version is the most recent can differ per municipality.<br/>Env: `VAC_OBJECT_TYPE_VERSION` |
| settings.syncJobs.vac.resources | object | `{}` | Resources of the sync job container. |
| settings.syncJobs.vac.schedule | string | `"*/59 * * * *"` | Cron schedule of the sync job. |
| settings.syncJobs.vac.token | string | `""` | Token for the Objects API for VACs. When `manageFromKiss` is `true`, this token needs read and write permissions.<br/>Env: `VAC_OBJECTEN_TOKEN` |
| settings.syncJobs.website | list | `[]` | Website crawler jobs configuration. Websites are crawled by the [Elastic Open Crawler](https://www.elastic.co/guide/en/elasticsearch/reference/current/es-connectors-crawler.html); one crawler job is created per item. These settings are written to the crawler configuration, there are no KISS environment variables for them. <details><summary>Fields of each item</summary><ul> <li>`domain`: <b>required.</b> Full URL of the website to crawl.</li> <li>`sourceName`: <b>required.</b> A human friendly name for the source, as shown in the search bar in KISS. Example: <i>ICATT Test Site</i>.</li> <li>`outputIndex`: Elasticsearch index to write to. Default: `search-` followed by the `sourceName` in lower case, with spaces replaced by `-`.</li> <li>`excludeTags`: HTML tags whose content is not indexed. Default `["header", "footer", "nav"]`</li> <li>`crawlRules`: <a href="https://github.com/elastic/crawler/blob/main/docs/features/CRAWL_RULES.md">crawl rules</a> of the Open Crawler, to include or exclude parts of the website.</li> <li>`maxCrawlDepth`: maximum crawl depth.</li> <li>`maxExtractedLinksCount`: maximum number of links extracted per page.</li> <li>`maxUniqueUrlCount`: maximum number of unique URLs crawled.</li> <li>`purgeCrawlEnabled`: remove pages from the index that were not found in the last crawl.</li> <li>`fullHtmlExtractionEnabled`: store the full HTML of each page.</li> <li>`binaryContentExtractionEnabled`: extract and index the content of PDF and Word documents.</li> <li>`userAgent`: user agent of the crawler. Default `Kiss-Elastic-Crawler`</li> <li>`threadsPerCrawl`: number of threads per crawl. Default `1`</li> <li>`schedule`: cron schedule of the crawler job. Default `*/59 * * * *`</li> <li>`historyLimit`: number of finished jobs to keep. Default `1`</li> <li>`resources`: resources of the crawler container.</li> </ul><b>Reserved source names:</b> don't use `Smoelenboek`, `VAC` or `Kennisbank` as `sourceName`, unless you don't use the corresponding source (a source is not used when it has no configuration). `Smoelenboek` belongs to `settings.syncJobs.medewerkers`, `VAC` to `settings.syncJobs.vac` and `Kennisbank` to `settings.syncJobs.kennisbank`.<br/><br/>It is advisable to consult your website administrator about fine-tuning the crawler, for example about the robots.txt, a KISS specific sitemap.xml or additional filters.</details> |

### Other Values

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| affinity | object | `{}` |  |
| autoscaling.enabled | bool | `false` |  |
| autoscaling.maxReplicas | int | `100` |  |
| autoscaling.minReplicas | int | `1` |  |
| autoscaling.targetCPUUtilizationPercentage | int | `80` |  |
| autoscaling.targetMemoryUtilizationPercentage | int | `80` |  |
| extraEnvVars | list | `[]` | Array with extra environment variables to add <details><summary>Settings without a chart value</summary>KISS supports a few environment variables that this chart doesn't expose as a value. If you need them, set them here. When both a client ID and secret and a token are set, KISS uses the client ID and secret.<ul> <li>`AFDELINGEN_CLIENT_ID`, `AFDELINGEN_CLIENT_SECRET`: client ID and secret for the Objects API for afdelingen. The chart requires `settings.afdelingen.token` to be filled in; when the client ID and secret are also set, KISS uses those.</li> <li>`GROEPEN_CLIENT_ID`, `GROEPEN_CLIENT_SECRET`: client ID and secret for the Objects API for groepen. The chart requires `settings.groepen.token` to be filled in; when the client ID and secret are also set, KISS uses those.</li> <li>`SDG_OBJECTEN_CLIENT_ID`, `SDG_OBJECTEN_CLIENT_SECRET`: client ID and secret for the Objects API for Kennisartikelen (used by the sync job), instead of `settings.syncJobs.kennisbank.token`.</li> <li>`VAC_OBJECTEN_CLIENT_ID`, `VAC_OBJECTEN_CLIENT_SECRET`: client ID and secret for the Objects API for VACs (used by the sync job only), instead of `settings.syncJobs.vac.token`.</li> <li>`REGISTERS__<n>__INTERNE_TAAK_TOKEN`: token for the Objects API for Interne Taken, instead of `interneTaak.clientId` and `interneTaak.clientSecret`. Leave those two empty when you use the token.</li> </ul></details> |
| extraIngress | list | `[]` | Specify extra ingresses, for example if you have multiple ingress classes |
| extraVolumeMounts | list | `[]` | Optionally specify extra list of additional volumeMounts, for example to trust extra ca certificates. |
| extraVolumes | list | `[]` | Optionally specify extra list of additional volumes, for example to trust extra ca certificates. |
| fullnameOverride | string | `""` |  |
| image.pullPolicy | string | `"IfNotPresent"` |  |
| image.repository | string | `"ghcr.io/klantinteractie-servicesysteem/kiss-frontend"` |  |
| image.tag | string | The semantic version of the chart | Image tag. Consider leaving this empty because it defaults to the recommended version of the image to use with this chart. |
| imagePullSecrets | list | `[]` |  |
| ingress.annotations | object | `{}` |  |
| ingress.className | string | `""` |  |
| ingress.enabled | bool | `false` |  |
| ingress.hosts | list | `[]` | ingress hosts |
| ingress.tls | list | `[]` |  |
| livenessProbe.failureThreshold | int | `6` |  |
| livenessProbe.initialDelaySeconds | int | `60` |  |
| livenessProbe.periodSeconds | int | `10` |  |
| livenessProbe.successThreshold | int | `1` |  |
| livenessProbe.timeoutSeconds | int | `5` |  |
| nameOverride | string | `""` |  |
| nodeSelector | object | `{}` |  |
| pdb.create | bool | `false` |  |
| pdb.maxUnavailable | string | `""` |  |
| pdb.minAvailable | int | `1` |  |
| podAnnotations | object | `{}` |  |
| podLabels | object | `{}` |  |
| podSecurityContext.fsGroup | int | `1000` |  |
| readinessProbe.failureThreshold | int | `6` |  |
| readinessProbe.initialDelaySeconds | int | `30` |  |
| readinessProbe.periodSeconds | int | `10` |  |
| readinessProbe.successThreshold | int | `1` |  |
| readinessProbe.timeoutSeconds | int | `5` |  |
| replicaCount | int | `2` |  |
| resources | object | `{}` |  |
| securityContext.allowPrivilegeEscalation | bool | `false` |  |
| securityContext.capabilities.drop[0] | string | `"ALL"` |  |
| securityContext.readOnlyRootFilesystem | bool | `true` |  |
| securityContext.runAsNonRoot | bool | `true` |  |
| securityContext.runAsUser | int | `10000` |  |
| service.port | int | `80` |  |
| service.type | string | `"ClusterIP"` |  |
| serviceAccount.annotations | object | `{}` |  |
| serviceAccount.automountServiceAccountToken | bool | `true` |  |
| serviceAccount.create | bool | `true` |  |
| serviceAccount.name | string | `""` |  |
| tolerations | list | `[]` |  |

----------------------------------------------
Autogenerated from chart metadata using [helm-docs v1.14.2](https://github.com/norwoodj/helm-docs/releases/v1.14.2)
