# kiss-chart

![Version: 0.0.0](https://img.shields.io/badge/Version-0.0.0-informational?style=flat-square) ![Type: application](https://img.shields.io/badge/Type-application-informational?style=flat-square) ![AppVersion: 0.0.0](https://img.shields.io/badge/AppVersion-0.0.0-informational?style=flat-square)

A helm chart for Klantinteractie Service Systeem.

## Values

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| affinity | object | `{}` |  |
| autoscaling.enabled | bool | `false` |  |
| autoscaling.maxReplicas | int | `100` |  |
| autoscaling.minReplicas | int | `1` |  |
| autoscaling.targetCPUUtilizationPercentage | int | `80` |  |
| autoscaling.targetMemoryUtilizationPercentage | int | `80` |  |
| extraEnvVars | list | `[]` | Array with extra environment variables to add |
| extraIngress | list | `[]` | Specify extra ingresses, for example if you have multiple ingress classes |
| extraVolumeMounts | list | `[]` | Optionally specify extra list of additional volumeMounts, for example to trust extra ca certificates. |
| extraVolumes | list | `[]` | Optionally specify extra list of additional volumes, for example to trust extra ca certificates. |
| fullnameOverride | string | `""` |  |
| image.pullPolicy | string | `"IfNotPresent"` |  |
| image.repository | string | `"ghcr.io/klantinteractie-servicesysteem/kiss-frontend"` |  |
| image.tag | string | `"latest"` |  |
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
| settings.afdelingen | object | `{"baseUrl":"","clientId":"","clientSecret":"","objectTypeUrl":"","token":""}` | afdelingen configuration |
| settings.afdelingen.baseUrl | string | `""` | URL of the Objects API for departments. |
| settings.afdelingen.clientId | string | `""` | Client ID for the Objects API for departments. Leave `token` empty if you use a Client ID and Client Secret. |
| settings.afdelingen.clientSecret | string | `""` | Client Secret for the Objects API for departments. Leave `token` empty if you use a Client ID and Client Secret. |
| settings.afdelingen.objectTypeUrl | string | `""` | URL of the Department Object Type |
| settings.afdelingen.token | string | `""` | Token of the Objects API for departments. Leave `clientId` and `clientSecret` empty if you use a token. |
| settings.aspnetcore.environment | string | `"Production"` |  |
| settings.aspnetcore.forwardedHeadersEnabled | bool | `true` |  |
| settings.aspnetcore.httpPorts | string | `""` |  |
| settings.database.host | string | `""` |  |
| settings.database.name | string | `""` | Name of the database used by KISS |
| settings.database.password | string | `""` | Password of the postgres user |
| settings.database.port | int | `5432` |  |
| settings.database.username | string | `""` | Username for KISS to access the database |
| settings.elastic.baseUrl | string | `""` | The URL for Elasticsearch |
| settings.elastic.excludedFieldsKennisbank | list | `[]` | Fields that a Kennisbank user is not allowed to search and view. |
| settings.elastic.password | string | `""` | Password to log in to Elasticsearch |
| settings.elastic.username | string | `""` | Username to log in to Elasticsearch |
| settings.email.enableSsl | bool | `true` | Use SSL (true/false) |
| settings.email.host | string | `""` | Address of the mail server |
| settings.email.password | string | `""` | Password for the mail server |
| settings.email.port | int | `25` | Port number of the mail connection |
| settings.email.username | string | `""` | Username for the mail server |
| settings.enterpriseSearch.baseUrl | string | `""` | URL of the API through which KISS can query enterprise search |
| settings.enterpriseSearch.engine | string | `""` | The name of the `meta-engine` engine used by KISS. |
| settings.enterpriseSearch.privateApiKey | string | `""` | Private API key for Elastic API |
| settings.enterpriseSearch.publicApiKey | string | `""` |  |
| settings.feedback.emailFrom | string | `""` | From address of the feedback email |
| settings.feedback.emailTo | string | `""` | Address where the feedback email should be sent |
| settings.groepen | object | `{"baseUrl":"","clientId":"","clientSecret":"","objectTypeUrl":"","token":""}` | groepen configuration |
| settings.groepen.baseUrl | string | `""` | URL of the Objects API for groups. |
| settings.groepen.clientId | string | `""` | Client ID for the Objects API for groups. Leave `token` empty if you use a Client ID and Client Secret. |
| settings.groepen.clientSecret | string | `""` | Client Secret for the Objects API for groups. Leave `token` empty if you use a Client ID and Client Secret. |
| settings.groepen.objectTypeUrl | string | `""` | URL of the Group Object Type |
| settings.groepen.token | string | `""` | Token of the Objects API for groups. Leave `clientId` and `clientSecret` empty if you use a token. |
| settings.haalCentraal.apiKey | string | `""` | Key for the Haal Centraal API |
| settings.haalCentraal.baseUrl | string | `""` | URL of the Haal Centraal API |
| settings.haalCentraal.customHeaders | object | `{}` |  |
| settings.haalCentraal.userHeaderName | string | `""` |  |
| settings.kvk.apiKey | string | `""` | Key for the KvK API |
| settings.kvk.baseUrl | string | `""` | URL of the KvK API |
| settings.kvk.customHeaders | object | `{}` |  |
| settings.kvk.userHeaderName | string | `""` |  |
| settings.logboek.baseUrl | string | `""` | URL of the Objects API where the logbook is stored |
| settings.logboek.objectTypeUrl | string | `""` | URL of the Logbook Object Type. |
| settings.logboek.objectTypeVersion | int | `1` | Version number of the Logbook Object Type. |
| settings.logboek.token | string | `""` | Token for the Objects API for Logbooks |
| settings.managementInformatie.apiKey | string | `""` | Secret that KISS uses to validate the JWT Token when requesting contact moment details |
| settings.oidc.authority | string | `""` | URL of the OpenID Connect Identity Provider |
| settings.oidc.clientId | string | `""` | For access to the OpenID Connect Identity Provider |
| settings.oidc.clientSecret | string | `""` | Secret for the OpenID Connect Identity Provider |
| settings.oidc.kennisbankRole | string | `""` | Name of the role for a Kennisbank employee. |
| settings.oidc.klantcontactmedewerkerRole | string | `""` | Name of the role for a Klant Contact Employee. |
| settings.oidc.medewerkerIdentificatie.claim | string | `""` | the claim to use for identifying the medewerker |
| settings.oidc.medewerkerIdentificatie.truncate | string | `nil` | max number of characters before truncation |
| settings.oidc.redacteurRole | string | `""` | Name of the role for a Redacteur. |
| settings.organisatieIds | list | `[]` | RSIN of the organization that registers the Contactmomenten |
| settings.registers | list | `[]` | Configuration for the different registers for e.g. zaken and klantcontacten. Check [the json schema](./values.schema.json) for the different possible configurations. |
| settings.syncJobs.image.pullPolicy | string | `"IfNotPresent"` |  |
| settings.syncJobs.image.repository | string | `"ghcr.io/klantinteractie-servicesysteem/kiss-elastic-sync"` |  |
| settings.syncJobs.image.tag | string | `"0.3.0"` |  |
| settings.syncJobs.kennisbank.baseUrl | string | `""` | URL of the API for Kennisartikelen |
| settings.syncJobs.kennisbank.historyLimit | int | `1` |  |
| settings.syncJobs.kennisbank.objectTypeUrl | string | `""` | URL of the Kennisartikel Object Type |
| settings.syncJobs.kennisbank.resources | object | `{}` |  |
| settings.syncJobs.kennisbank.schedule | string | `"*/59 * * * *"` |  |
| settings.syncJobs.kennisbank.token | string | `""` | Key for the API for Kennisartikelen |
| settings.syncJobs.medewerkers | object | `{"baseUrl":"","clientId":"","clientSecret":"","historyLimit":1,"objectTypeUrl":"","resources":{},"schedule":"*/59 * * * *","token":"","useEmail":false}` | medewerkers sync job configuration |
| settings.syncJobs.medewerkers.baseUrl | string | `""` | URL of the Objects API for employees |
| settings.syncJobs.medewerkers.clientId | string | `""` | Client ID for the Objects API for employees. Leave `token` empty if you use a Client ID and Client Secret. |
| settings.syncJobs.medewerkers.clientSecret | string | `""` | Client Secret for the Objects API for employees. Leave `token` empty if you use a Client ID and Client Secret. |
| settings.syncJobs.medewerkers.objectTypeUrl | string | `""` | URL of the Employee Object Type |
| settings.syncJobs.medewerkers.token | string | `""` | Token for the Objects API for employees. Leave `clientId` and `clientSecret` empty if you use a token. |
| settings.syncJobs.medewerkers.useEmail | bool | `false` | This variable determines whether a contact request for an employee can only be made by email address. |
| settings.syncJobs.sharepoint | list | `[]` | sharepoint sync jobs configuration |
| settings.syncJobs.vac | object | `{"baseUrl":"","historyLimit":1,"manageFromKiss":false,"objectTypeUrl":"","objectTypeVersion":1,"resources":{},"schedule":"*/59 * * * *","token":""}` | vac sync job configuration |
| settings.syncJobs.vac.baseUrl | string | `""` | URL of the Objects API for VACs |
| settings.syncJobs.vac.manageFromKiss | bool | `false` | This variable determines whether the navigation item for managing VACs is present in the management navigation. |
| settings.syncJobs.vac.objectTypeUrl | string | `""` | URL of the VAC Object Type |
| settings.syncJobs.vac.objectTypeVersion | int | `1` | Version number of the VAC Object Type |
| settings.syncJobs.vac.token | string | `""` | Token for the Objects API for VACs |
| settings.syncJobs.website | list | `[]` | website sync jobs configuration |
| tolerations | list | `[]` |  |

----------------------------------------------
Autogenerated from chart metadata using [helm-docs v1.14.2](https://github.com/norwoodj/helm-docs/releases/v1.14.2)
