$name = "elasticsearch_test"
$resourcegroup = "Test_Elasticsearch_Upgrade_RG"
$namespace = "kiss-namespace"
$elasticversion = "8.9.0"
$kibanaversion = "8.9.0"
$enterprisesearchversion = "8.9.0"

az aks get-credentials --resource-group $resourcegroup --name $name 
kubectl config set-context --current --namespace=$namespace

#update statefulset elasticsearch-master image container and initcontainer
kubectl set image statefulset/elasticsearch-master -n $namespace elasticsearch=docker.elastic.co/elasticsearch/elasticsearch:$elasticversion configure-sysctl=docker.elastic.co/elasticsearch/elasticsearch:$elasticversion

#update deployment kiss-ci-release-kibana image container
kubectl set image deployment/kiss-ci-release-kibana -n $namespace kibana=docker.elastic.co/kibana/kibana:$kibanaversion

#update deployment enterprise-search image container
kubectl set image deployment/enterprise-search-ent -n $namespace enterprise-search=docker.elastic.co/enterprise-search/enterprise-search:$enterprisesearchversion
