$kibanaServiceToken = "AAEAAWVsYXN0aWMva2liYW5hL3Rva2VuXzg3Umt3SWtCOEdmUkdEUGFaRnN1OkRzVlBrLXdyUTFhaEZoQ2NUV3d0RHc"
$resourceGroup = "Test_Elasticsearch_Upgrade_RG"
$name = "elasticsearch_test"
$namespace = "kiss-namespace"

if (!(Get-Module -ListAvailable -Name powershell-yaml)) {
    Install-Module -Name powershell-yaml -Force -Scope CurrentUser
}

# Used for editing yaml
Import-Module powershell-yaml

az aks get-credentials --resource-group $resourceGroup --name $name
kubectl config set-context --current --namespace=$namespace

# Fetch, backup and convert the existing kibana configuration
kubectl get configmap kiss-ci-release-kibana-config -n $namespace -o yaml > kibana.yaml
Copy-Item -Path .\kibana.yaml -Destination .\kibana-old.yaml
$kibanaYamlContent = ConvertFrom-Yaml (Get-Content -Raw -Path .\kibana.yaml)

if ($kibanaYamlContent.data.'kibana.yml' -match 'elasticsearch.serviceAccountToken') {
    $kibanaYamlContent.data.'kibana.yml' = $kibanaYamlContent.data.'kibana.yml' -replace 'elasticsearch.serviceAccountToken: \S+', "elasticsearch.serviceAccountToken: $kibanaServiceToken"
} elseif ($kibanaYamlContent.data.'kibana.yml') {
    $kibanaYamlContent.data.'kibana.yml' += "elasticsearch.serviceAccountToken: $kibanaServiceToken"
} else {
    Write-Output "The YAML file content does not meet the expected format. Exiting..."
    Start-Sleep -Seconds 5
    exit
}

$kibanaYamlContent | ConvertTo-Yaml | Out-File -FilePath .\kibana.yaml -Force
kubectl delete -f kibana.yaml -n $namespace
kubectl apply -f kibana.yaml -n $namespace

# Fetch, backup and convert the existing elasticsearch-master configuration
kubectl get statefulset elasticsearch-master -n $namespace -o yaml > elasticsearch-master.yaml
Copy-Item -Path .\elasticsearch-master.yaml -Destination .\elasticsearch-master-old.yaml
$elasticYamlContent = ConvertFrom-Yaml (Get-Content -Raw -Path .\elasticsearch-master.yaml)

$containers = $elasticYamlContent.spec.template.spec.containers
foreach($container in $containers) {
    if($container.env) {
        $container.env = $container.env | Where-Object { $_.name -ne "node.ingest" -and $_.name -ne "node.master" -and $_.name -ne "node.ml" -and $_.name -ne "node.data" -and $_.name -ne "node.remote_cluster_client" }
    }
}

$elasticYamlContent | ConvertTo-Yaml | Out-File -FilePath .\elasticsearch-master.yaml -Force
kubectl delete -f elasticsearch-master.yaml -n $namespace
kubectl apply -f elasticsearch-master.yaml -n $namespace

# Fetch, backup and convert the existing kibana deployment configuration
kubectl get deployment kiss-ci-release-kibana -n $namespace -o yaml > kiss-ci-release-kibana.yaml
Copy-Item -Path .\kiss-ci-release-kibana.yaml -Destination .\kiss-ci-release-kibana-old.yaml
$kibanaDeploymentYamlContent = ConvertFrom-Yaml (Get-Content -Raw -Path .\kiss-ci-release-kibana.yaml)

$kibanaContainers = $kibanaDeploymentYamlContent.spec.template.spec.containers
foreach($container in $kibanaContainers) {
    if($container.env) {
        $container.env = $container.env | Where-Object { $_.name -ne "ELASTICSEARCH_USERNAME" -and $_.name -ne "ELASTICSEARCH_PASSWORD" }
    }
}

$kibanaDeploymentYamlContent | ConvertTo-Yaml | Out-File -FilePath .\kiss-ci-release-kibana.yaml -Force
kubectl delete -f kiss-ci-release-kibana.yaml -n $namespace
kubectl apply -f kiss-ci-release-kibana.yaml -n $namespace
