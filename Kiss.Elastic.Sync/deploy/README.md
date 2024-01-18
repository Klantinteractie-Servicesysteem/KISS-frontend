# Deployen cronjobs

The syncing of sources to Elasticsearch is done by cronjobs. For each cronjob (for each supported source) a yaml file can be found in this folder. 

To install a cronjob you can run the following command on the cluster that KISS and Elasticsearch are running on:
`kubectl apply -f .\cronjob-kennisartikelen.yaml`