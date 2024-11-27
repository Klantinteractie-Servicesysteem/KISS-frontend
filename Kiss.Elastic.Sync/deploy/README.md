# Deployen cronjobs

The syncing of sources to Elasticsearch is done by cronjobs. For each cronjob (for each supported source) a yaml file can be found in this folder. 

To install a cronjob you can run the following command on the cluster that KISS and Elasticsearch are running on:
`kubectl apply -f .\cronjob-kennisartikelen.yaml`

To adjust the sync schedule, either:
- make custom versions of these yaml files and use those,
- adjust the schedule in the Kubernetes cluster after installation
- if you are using KISS as part of PodiumD, you can override them at deployment (see https://github.com/Dimpact-Samenwerking/helm-charts/tree/main/charts/kiss)
