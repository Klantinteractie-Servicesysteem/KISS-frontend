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
| azureVaultSecret.contentType | string | `""` |  |
| azureVaultSecret.objectName | string | `""` |  |
| azureVaultSecret.secretName | string | `"{{ .Values.existingSecret }}"` |  |
| azureVaultSecret.vaultName | string | `nil` |  |
| existingSecret | string | `nil` |  |
| extraIngress | list | `[]` | Specify extra ingresses, for example if you have multiple ingress classes |
| extraVolumeMounts | list | `[]` | Optionally specify extra list of additional volumeMounts, for example to trust extra ca certificates. |
| extraVolumes | list | `[]` | Optionally specify extra list of additional volumes, for example to trust extra ca certificates. |
| fullnameOverride | string | `""` |  |
| image.pullPolicy | string | `"IfNotPresent"` |  |
| image.repository | string | `"ghcr.io/klantinteractie-servicesysteem/kiss-frontend-api"` |  |
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
| securityContext.capabilities.drop[0] | string | `"ALL"` |  |
| securityContext.readOnlyRootFilesystem | bool | `false` |  |
| securityContext.runAsNonRoot | bool | `true` |  |
| securityContext.runAsUser | int | `1000` |  |
| service.port | int | `80` |  |
| service.type | string | `"ClusterIP"` |  |
| serviceAccount.annotations | object | `{}` |  |
| serviceAccount.automountServiceAccountToken | bool | `true` |  |
| serviceAccount.create | bool | `true` |  |
| serviceAccount.name | string | `""` |  |
| settings.afdelingen.baseUrl | string | `""` |  |
| settings.afdelingen.clientSecret | string | `""` |  |
| settings.afdelingen.objectTypeUrl | string | `""` |  |
| settings.afdelingen.token | string | `""` |  |
| settings.aspnetcore.environment | string | `""` |  |
| settings.aspnetcore.forwardedHeadersEnabled | bool | `true` |  |
| settings.aspnetcore.httpPorts | string | `""` |  |
| settings.elastic.baseUrl | string | `""` |  |
| settings.elastic.excludedFields.kennisbank | list | `[]` |  |
| settings.elastic.password | string | `""` |  |
| settings.elastic.username | string | `""` |  |
| settings.email.enableSsl | bool | `false` |  |
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
| settings.groepen.baseUrl | string | `""` |  |
| settings.groepen.clientSecret | string | `""` |  |
| settings.groepen.objectTypeUrl | string | `""` |  |
| settings.groepen.token | string | `""` |  |
| settings.haalCentraal.apiKey | string | `""` |  |
| settings.haalCentraal.baseUrl | string | `""` |  |
| settings.kvk.apiKey | string | `""` |  |
| settings.kvk.baseUrl | string | `""` |  |
| settings.logboek.baseUrl | string | `""` |  |
| settings.logboek.objectTypeUrl | string | `""` |  |
| settings.logboek.objectTypeVersion | int | `1` |  |
| settings.logboek.token | string | `""` |  |
| settings.managementInformatie.apiKey | string | `""` |  |
| settings.medewerker.objectTypeUrl | string | `""` |  |
| settings.medewerker.objectenClientId | string | `""` |  |
| settings.medewerker.objectenClientSecret | string | `""` |  |
| settings.medewerker.objectenToken | string | `""` |  |
| settings.oidc.authority | string | `""` |  |
| settings.oidc.clientId | string | `""` |  |
| settings.oidc.clientSecret | string | `""` |  |
| settings.oidc.kennisbankRole | string | `""` |  |
| settings.oidc.medewerkerIdentificatieClaim | string | `""` |  |
| settings.oidc.medewerkerIdentificatieTruncate | bool | `false` |  |
| settings.organisatieIds | list | `[]` |  |
| settings.postgres.db | string | `"kiss-frontend"` |  |
| settings.postgres.host | string | `""` |  |
| settings.postgres.password | string | `""` |  |
| settings.postgres.port | int | `5432` |  |
| settings.postgres.user | string | `""` |  |
| settings.registers | list | `[]` |  |
| settings.useMedewerkerEmail | bool | `false` |  |
| settings.useVacs | bool | `false` |  |
| settings.vac.objectTypeUrl | string | `""` |  |
| settings.vac.objectTypeVersion | int | `1` |  |
| settings.vac.objectenBaseUrl | string | `""` |  |
| settings.vac.objectenToken | string | `""` |  |
| tolerations | list | `[]` |  |

----------------------------------------------
Autogenerated from chart metadata using [helm-docs v1.14.2](https://github.com/norwoodj/helm-docs/releases/v1.14.2)
