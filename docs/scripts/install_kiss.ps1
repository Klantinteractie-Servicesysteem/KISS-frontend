$env = "dev"
$cert = "__$env-kiss-demo_nl.crt"
$key = "kiss-$env-private.key"
$namespace = "kiss-namespace"
$kibanaHost = "kibana.dev.kiss-demo.nl"

kubectl create namespace $namespace
kubectl config current-context
kubectl config set-context --current --namespace=$namespace

# Add helm repos
helm repo add ingress-nginx-repo https://kubernetes.github.io/ingress-nginx
helm repo add kiss-repo-ci https://raw.githubusercontent.com/Klantinteractie-Servicesysteem/KISS-frontend/main/helm
helm repo add kiss-elastic https://raw.githubusercontent.com/Klantinteractie-Servicesysteem/.github/main/docs/scripts/elastic
helm repo update

# Install nginx ingress
helm upgrade --install --namespace kiss-namespace ingress-nginx-release ingress-nginx-repo/ingress-nginx `
 --set controller.service.externalTrafficPolicy=Local --set annotations.nginx.ingress.kubernetes.io/proxy-buffer-size=16k

# Create TLS secret
kubectl create secret tls --cert $cert --key=$key wildcard-kiss-tls -n $namespace

# Install elastic components
kubectl create -f https://download.elastic.co/downloads/eck/2.9.0/crds.yaml
kubectl apply -f https://download.elastic.co/downloads/eck/2.9.0/operator.yaml
helm upgrade --set kibanaDomain=$kibanaHost --install kiss-elastic kiss-elastic/kiss-elastic --version 0.1.0

$POSTGRES_PASSWORD = 'ChangeThis'
$ELASTIC_PASSWORD = kubectl get secret kiss-es-elastic-user -o go-template='{{.data.elastic | base64decode }}'

# Add config
kubectl create configmap kiss-config `
    --from-literal=AFDELINGEN_BASE_URL=$(AFDELINGEN_BASE_URL) `
    --from-literal=AFDELINGEN_CLIENT_ID=$(AFDELINGEN_CLIENT_ID) `
    --from-literal=AFDELINGEN_OBJECT_TYPE_URL=$(AFDELINGEN_OBJECT_TYPE_URL) `
    --from-literal=ASPNETCORE_ENVIRONMENT=$(ASPNETCORE_ENVIRONMENT) `
    --from-literal=ELASTIC_BASE_URL=$(ELASTIC_BASE_URL) `
    --from-literal=ELASTIC_USERNAME=$(ELASTIC_USERNAME) `
    --from-literal=EMAIL_ENABLE_SSL=$(EMAIL_ENABLE_SSL) `
    --from-literal=EMAIL_HOST=$(EMAIL_HOST) `
    --from-literal=EMAIL_PORT=$(EMAIL_PORT) `
    --from-literal=EMAIL_USERNAME=$(EMAIL_USERNAME) `
    --from-literal=ENTERPRISE_SEARCH_BASE_URL=$(ENTERPRISE_SEARCH_BASE_URL) `
    --from-literal=ENTERPRISE_SEARCH_ENGINE=$(ENTERPRISE_SEARCH_ENGINE) `
    --from-literal=FEEDBACK_EMAIL_FROM=$(FEEDBACK_EMAIL_FROM) `
    --from-literal=FEEDBACK_EMAIL_TO=$(FEEDBACK_EMAIL_TO) `
    --from-literal=GROEPEN_BASE_URL=$(GROEPEN_BASE_URL) `
    --from-literal=GROEPEN_CLIENT_ID=$(GROEPEN_CLIENT_ID) `
    --from-literal=GROEPEN_OBJECT_TYPE_URL=$(GROEPEN_OBJECT_TYPE_URL) `
    --from-literal=HAAL_CENTRAAL_BASE_URL=$(HAAL_CENTRAAL_BASE_URL) `
    --from-literal=KVK_BASE_URL=$(KVK_BASE_URL) `
    --from-literal=MEDEWERKER_OBJECTEN_BASE_URL=$(MEDEWERKER_OBJECTEN_BASE_URL) `
    --from-literal=MEDEWERKER_OBJECTTYPES_BASE_URL=$(MEDEWERKER_OBJECTTYPES_BASE_URL) `
    --from-literal=OIDC_AUTHORITY=$(OIDC_AUTHORITY) `
    --from-literal=OIDC_CLIENT_ID=$(OIDC_CLIENT_ID) `
    --from-literal=OIDC_KLANTCONTACTMEDEWERKER_ROLE=$(OIDC_KLANTCONTACTMEDEWERKER_ROLE) `
    --from-literal=OIDC_MEDEWERKER_IDENTIFICATIE_CLAIM=$(OIDC_MEDEWERKER_IDENTIFICATIE_CLAIM) `
    --from-literal=OIDC_MEDEWERKER_IDENTIFICATIE_TRUNCATE=$(OIDC_MEDEWERKER_IDENTIFICATIE_TRUNCATE) `
    --from-literal=OIDC_REDACTEUR_ROLE=$(OIDC_REDACTEUR_ROLE) `
    --from-literal=ORGANISATIE_IDS=$(ORGANISATIE_IDS) `
    --from-literal=POSTGRES_DB=$(POSTGRES_DB) `
    --from-literal=POSTGRES_HOST=$(POSTGRES_HOST) `
    --from-literal=POSTGRES_PORT=$(POSTGRES_PORT) `
    --from-literal=POSTGRES_USER=$(POSTGRES_USER) `
    --from-literal=SDG_BASE_URL=$(SDG_BASE_URL) `
    --from-literal=SDG_OBJECTEN_BASE_URL=$(SDG_OBJECTEN_BASE_URL) `
    --from-literal=SDG_OBJECT_TYPE_URL=$(SDG_OBJECT_TYPE_URL) `
    --from-literal=VAC_OBJECTEN_BASE_URL=$(VAC_OBJECTEN_BASE_URL) `
    --from-literal=VAC_OBJECT_TYPE_URL=$(VAC_OBJECT_TYPE_URL) `
    --from-literal=VAC_OBJECT_TYPE_VERSION=$(VAC_OBJECT_TYPE_VERSION) `
    --from-literal=REGISTERS__0__IS_DEFAULT=$(REGISTERS__0__IS_DEFAULT) `
    --from-literal=REGISTERS__0__KLANTINTERACTIE_BASE_URL=$(REGISTERS__0__KLANTINTERACTIE_BASE_URL) `
    --from-literal=REGISTERS__0__REGISTRY_VERSION=$(REGISTERS__0__REGISTRY_VERSION) `
    --from-literal=REGISTERS__0__ZAAKSYSTEEM_BASE_URL=$(REGISTERS__0__ZAAKSYSTEEM_BASE_URL) `
    --from-literal=REGISTERS__0__ZAAKSYSTEEM_API_CLIENT_ID=$(REGISTERS__0__ZAAKSYSTEEM_API_CLIENT_ID) `
    --from-literal=REGISTERS__0__ZAAKSYSTEEM_DEEPLINK_URL=$(REGISTERS__0__ZAAKSYSTEEM_DEEPLINK_URL) `
    --from-literal=REGISTERS__0__ZAAKSYSTEEM_DEEPLINK_PROPERTY=$(REGISTERS__0__ZAAKSYSTEEM_DEEPLINK_PROPERTY) `
    --from-literal=REGISTERS__1__IS_DEFAULT=$(REGISTERS__1__IS_DEFAULT) `
    --from-literal=REGISTERS__1__CONTACTMOMENTEN_BASE_URL=$(REGISTERS__1__CONTACTMOMENTEN_BASE_URL) `
    --from-literal=REGISTERS__1__REGISTRY_VERSION=$(REGISTERS__1__REGISTRY_VERSION) `
    --from-literal=REGISTERS__1__CONTACTMOMENTEN_API_CLIENT_ID=$(REGISTERS__1__CONTACTMOMENTEN_API_CLIENT_ID) `
    --from-literal=REGISTERS__1__INTERNE_TAAK_BASE_URL=$(REGISTERS__1__INTERNE_TAAK_BASE_URL) `
    --from-literal=REGISTERS__1__INTERNE_TAAK_CLIENT_ID=$(REGISTERS__1__INTERNE_TAAK_CLIENT_ID) `
    --from-literal=REGISTERS__1__INTERNE_TAAK_OBJECT_TYPE_URL=$(REGISTERS__1__INTERNE_TAAK_OBJECT_TYPE_URL) `
    --from-literal=REGISTERS__1__INTERNE_TAAK_TYPE_VERSION=$(REGISTERS__1__INTERNE_TAAK_TYPE_VERSION) `
    --from-literal=REGISTERS__1__KLANTEN_BASE_URL=$(REGISTERS__1__KLANTEN_BASE_URL) `
    --from-literal=REGISTERS__1__KLANTEN_CLIENT_ID=$(REGISTERS__1__KLANTEN_CLIENT_ID) `
    --from-literal=REGISTERS__1__ZAAKSYSTEEM_BASE_URL=$(REGISTERS__1__ZAAKSYSTEEM_BASE_URL) `
    --from-literal=REGISTERS__1__ZAAKSYSTEEM_API_CLIENT_ID=$(REGISTERS__1__ZAAKSYSTEEM_API_CLIENT_ID) `
    --from-literal=REGISTERS__1__ZAAKSYSTEEM_DEEPLINK_URL=$(REGISTERS__1__ZAAKSYSTEEM_DEEPLINK_URL) `
    --from-literal=REGISTERS__1__ZAAKSYSTEEM_DEEPLINK_PROPERTY=$(REGISTERS__1__ZAAKSYSTEEM_DEEPLINK_PROPERTY)
    --dry-run=client -o yaml | kubectl apply -f -

# Add secrets
kubectl create secret generic kiss-secrets `
    --from-literal=AFDELINGEN_CLIENT_SECRET=$(AFDELINGEN_CLIENT_SECRET) `
    --from-literal=AFDELINGEN_TOKEN=$(AFDELINGEN_TOKEN) `
    --from-literal=ELASTIC_PASSWORD=$ELASTIC_PASSWORD `
    --from-literal=EMAIL_PASSWORD=$(EMAIL_PASSWORD) `
    --from-literal=ENTERPRISE_SEARCH_PRIVATE_API_KEY=$(ENTERPRISE_SEARCH_PRIVATE_API_KEY) `
    --from-literal=ENTERPRISE_SEARCH_PUBLIC_API_KEY=$(ENTERPRISE_SEARCH_PUBLIC_API_KEY) `
    --from-literal=GROEPEN_CLIENT_SECRET=$(GROEPEN_CLIENT_SECRET) `
    --from-literal=GROEPEN_TOKEN=$(GROEPEN_TOKEN) `
    --from-literal=HAAL_CENTRAAL_API_KEY=$(HAAL_CENTRAAL_API_KEY) `
    --from-literal=KVK_API_KEY=$(KVK_API_KEY) `
    --from-literal=MEDEWERKER_OBJECTEN_TOKEN=$(MEDEWERKER_OBJECTEN_TOKEN) `
    --from-literal=OIDC_CLIENT_SECRET=$(OIDC_CLIENT_SECRET) `
    --from-literal=POSTGRES_PASSWORD=$POSTGRES_PASSWORD `
    --from-literal=SDG_OBJECTEN_TOKEN=$(SDG_OBJECTEN_TOKEN) `
    --from-literal=VAC_OBJECTEN_TOKEN=$(VAC_OBJECTEN_TOKEN) `
    --from-literal=VAC_OBJECTTYPES_TOKEN=$(VAC_OBJECTTYPES_TOKEN) `
    --from-literal=MANAGEMENTINFORMATIE_API_KEY=$(MANAGEMENTINFORMATIE_API_KEY) `
    --from-literal=REGISTERS__0__KLANTINTERACTIE_TOKEN=$(REGISTERS__0__KLANTINTERACTIE_TOKEN) `
    --from-literal=REGISTERS__0__ZAAKSYSTEEM_API_KEY=$(REGISTERS__0__ZAAKSYSTEEM_API_KEY) `
    --from-literal=REGISTERS__1__CONTACTMOMENTEN_API_KEY=$(REGISTERS__1__CONTACTMOMENTEN_API_KEY) `
    --from-literal=REGISTERS__1__INTERNE_TAAK_CLIENT_SECRET=$(REGISTERS__1__INTERNE_TAAK_CLIENT_SECRET) `
    --from-literal=REGISTERS__1__KLANTEN_CLIENT_SECRET=$(REGISTERS__1__KLANTEN_CLIENT_SECRET) `
    --from-literal=REGISTERS__1__ZAAKSYSTEEM_API_KEY=$(REGISTERS__1__ZAAKSYSTEEM_API_KEY) `
    --dry-run=client -o yaml | kubectl apply -f -

helm upgrade --install kiss-ci-release kiss-repo-ci/kiss-frontend --version 1.0.51 `
    --values [local dir]\kiss.template.yaml `
    --set image.tag='latest' `
    --set global.postgresql.auth.username='kiss.bff' `
    --set global.postgresql.auth.password=$POSTGRES_PASSWORD `
    --set global.postgresql.auth.database='kiss.bff' `
    --set global.postgresql.auth.postgresPassword=$POSTGRES_PASSWORD
