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
| settings.aspnetcore.environment | string | `"Production"` |  |
| settings.aspnetcore.forwardedHeadersEnabled | bool | `true` |  |
| settings.aspnetcore.httpPorts | string | `""` |  |
| settings.database.host | string | `""` |  |
| settings.database.name | string | `""` |  |
| settings.database.password | string | `""` |  |
| settings.database.port | int | `5432` |  |
| settings.database.username | string | `""` |  |
| settings.elastic.baseUrl | string | `""` |  |
| settings.elastic.excludedFieldsKennisbank | list | `[]` |  |
| settings.elastic.password | string | `""` |  |
| settings.elastic.username | string | `""` |  |
| settings.email.enableSsl | bool | `true` |  |
| settings.email.host | string | `""` |  |
| settings.email.password | string | `""` |  |
| settings.email.port | int | `25` |  |
| settings.email.username | string | `""` |  |
| settings.enterpriseSearch.baseUrl | string | `""` |  |
| settings.enterpriseSearch.engine | string | `""` |  |
| settings.enterpriseSearch.privateApiKey | string | `""` |  |
| settings.enterpriseSearch.publicApiKey | string | `""` |  |
| settings.feedback.emailFrom | string | `""` |  |
| settings.feedback.emailTo | string | `""` |  |
| settings.groepen | object | `{"baseUrl":"","clientId":"","clientSecret":"","objectTypeUrl":"","token":""}` | groepen configuration |
| settings.haalCentraal.apiKey | string | `""` |  |
| settings.haalCentraal.baseUrl | string | `""` |  |
| settings.haalCentraal.customHeaders | object | `{}` |  |
| settings.haalCentraal.userHeaderName | string | `""` |  |
| settings.kvk.apiKey | string | `""` |  |
| settings.kvk.baseUrl | string | `""` |  |
| settings.kvk.customHeaders | object | `{}` |  |
| settings.kvk.userHeaderName | string | `""` |  |
| settings.logboek.baseUrl | string | `""` |  |
| settings.logboek.objectTypeUrl | string | `""` |  |
| settings.logboek.objectTypeVersion | int | `1` |  |
| settings.logboek.token | string | `""` |  |
| settings.managementInformatie.apiKey | string | `""` |  |
| settings.oidc.authority | string | `""` |  |
| settings.oidc.clientId | string | `""` |  |
| settings.oidc.clientSecret | string | `""` |  |
| settings.oidc.kennisbankRole | string | `""` |  |
| settings.oidc.klantcontactmedewerkerRole | string | `""` |  |
| settings.oidc.medewerkerIdentificatie.claim | string | `""` | the claim to use for identifying the medewerker |
| settings.oidc.medewerkerIdentificatie.truncate | string | `nil` | max number of characters before truncation |
| settings.oidc.redacteurRole | string | `""` |  |
| settings.organisatieIds | list | `[]` |  |
| settings.registers | list | `[]` | Configuration for the different registers for e.g. zaken and klantcontacten. Check the json schema for the different possible configurations. |
| settings.syncJobs.image.pullPolicy | string | `"IfNotPresent"` |  |
| settings.syncJobs.image.repository | string | `"ghcr.io/klantinteractie-servicesysteem/kiss-elastic-sync"` |  |
| settings.syncJobs.image.tag | string | `"0.3.0"` |  |
| settings.syncJobs.kennisbank.baseUrl | string | `""` |  |
| settings.syncJobs.kennisbank.historyLimit | int | `1` |  |
| settings.syncJobs.kennisbank.objectTypeUrl | string | `""` |  |
| settings.syncJobs.kennisbank.resources | object | `{}` |  |
| settings.syncJobs.kennisbank.schedule | string | `"*/59 * * * *"` |  |
| settings.syncJobs.kennisbank.token | string | `""` |  |
| settings.syncJobs.medewerkers | object | `{"baseUrl":"","clientId":"","clientSecret":"","historyLimit":1,"objectTypeUrl":"","resources":{},"schedule":"*/59 * * * *","token":"","useEmail":false}` | medewerkers sync job configuration |
| settings.syncJobs.medewerkers.clientId | string | `""` | alternatively, client id and client secret can be provided, for instance if you are using the podiumd-adapter to get medewerkers. |
| settings.syncJobs.medewerkers.clientSecret | string | `""` | client secret to access the objects API. |
| settings.syncJobs.medewerkers.token | string | `""` | token to access the objects API. if provided, leave out clientId and clientSecret. |
| settings.syncJobs.sharepoint | list | `[]` | sharepoint sync jobs configuration |
| settings.syncJobs.vac | object | `{"baseUrl":"","historyLimit":1,"manageFromKiss":false,"objectTypeUrl":"","objectTypeVersion":1,"resources":{},"schedule":"*/59 * * * *","token":""}` | vac sync job configuration |
| settings.syncJobs.website | list | `[]` | website sync jobs configuration |
| tolerations | list | `[]` |  |

----------------------------------------------
Autogenerated from chart metadata using [helm-docs v1.14.2](https://github.com/norwoodj/helm-docs/releases/v1.14.2)
