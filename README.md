[![Build and Tests](https://github.com/Klantinteractie-Servicesysteem/KISS-frontend/actions/workflows/docker-image.yaml/badge.svg)](https://github.com/Klantinteractie-Servicesysteem/KISS-frontend/actions?query=workflow%3ADocker+CI+)
[![Code quality checks](https://github.com/Klantinteractie-Servicesysteem/KISS-frontend/actions/workflows/linter.yml/badge.svg)](https://github.com/Klantinteractie-Servicesysteem/KISS-frontend/actions?query=workflow%3A+code+quality+checks)
[![Known Vulnerabilities](https://snyk.io/test/github/Klantinteractie-Servicesysteem/KISS-frontend/badge.svg)](https://snyk.io/test/github/Klantinteractie-Servicesysteem/KISS-frontend)
[![Dependabot](https://img.shields.io/badge/dependabot-025E8C?style=for-the-badge&logo=dependabot&logoColor=white)](https://github.com/Klantinteractie-Servicesysteem/KISS-frontend/pulls?q=is%3Apr+author%3Aapp%2Fdependabot)
# kiss-frontend

## Set-up environment variables
1. Make a copy of .env.local.example, rename it .env.local and fill in the required secrets. This is a subset, a complete overview can be found at https://kiss-klantinteractie-servicesysteem.readthedocs.io/en/latest/INSTALLATION/:  
   - `OIDC_CLIENT_SECRET`, `OIDC_CLIENT_ID`, `OIDC_AUTHORITY`: use any OIDC provider. Users have access if they have a claim of either type `role` or type `roles` and a value that corresponds to the environment variable `OIDC_KLANTCONTACTMEDEWERKER_ROLE` (`Klantcontactmedewerker` by default). If you're using Azure AD, this can be done by [creating an application role and assigning it to either groups or individual users](https://learn.microsoft.com/en-us/azure/active-directory/develop/howto-add-app-roles-in-azure-ad-apps). Do the same with `OIDC_KLANTCONTACTMEDEWERKER_ROLE` (`Redacteur` by default) to enable a user to manage content in KISS.
   - `OIDC_MEDEWERKER_IDENTIFICATIE_CLAIM` and `OIDC_MEDEWERKER_IDENTIFICATIE_TRUNCATE`: the current ZGW standards limit the medewerker identificatie to a maximum of 24 characters. If you are using KISS with OpenKlant / OpenZaak, this means you either need to configure a claim in `OIDC_MEDEWERKER_IDENTIFICATIE_CLAIM` that will never exceed this limit, or configure for it to be truncated by setting `OIDC_MEDEWERKER_IDENTIFICATIE_TRUNCATE` to 24. If you are using KISS with the eSuite, make sure you configure a claim in `OIDC_MEDEWERKER_IDENTIFICATIE_CLAIM` that contains te username known in the eSuite.
   - `KVK_BASE_URL`: for the KvK test environment, use `https://api.kvk.nl/test/api/v1` 
   - `KVK_API_KEY`: for the KvK test environment, look for the API key on [the KvK website](https://developers.kvk.nl/documentation/testing)
2. Download the Root and intermediate certificates from [the KvK website](https://developers.kvk.nl/documentation/install-tls-certificate#download-certificates) and place them in a `certificates` folder in the root of the repo
Now, you can either run the application from Visual Studio or with docker-compose

## Run from Visual Studio 2022 
Be sure to set-up environment variables first
1. Make sure you've installed Docker Desktop version 4.19.0 or newer (preferably with WSL2 if using windows) and visual studio version 17.5.5 or newer
2. Open KISS-frontend.sln in Visual Studio 2022
3. Right-click the solution in the Solution Explorer and pick Configure Startup Projects
4. Select Multiple startup projects, and set the Action to Start for docker-compose and KISS-frontend
5. Startup the solution and wait for both the BFF and the frontend to be ready (note, initially you might get an error, due to different startup times of individual components. refresh the page after a few moments)

## Run with docker-compose
Be sure to set-up environment variables first
To run the front-end and BFF with docker you need a cmd opened in the root of this project.
Build the KISS-frontend image
```sh
docker-compose build
```
After that you can run the image with its dependencies: 
```sh
docker-compose up
```
Then launch a browser on [this address](http://localhost:7231)

## Type Support for `.vue` Imports in TS (Visual Studio Code)
1. Install the recommended Extensions (a pop-up should launch if you open the root of the repository in VS Code)
1. Disable the built-in TypeScript Extension
    1) Run `Extensions: Show Built-in Extensions` from VSCode's command palette
    2) Find `TypeScript and JavaScript Language Features`, right click and select `Disable (Workspace)`
1. Reload the VSCode window by running `Developer: Reload Window` from the command palette.

## Elastic search
KISS uses Elastic search. Building a search query is composed of two steps:
1. The [App Search search-explain endpoint](https://www.elastic.co/guide/en/app-search/current/search-explain.html) is called to build an elasticsearch query
1. The [Elasticsearch search endpoint](https://www.elastic.co/guide/en/elasticsearch/reference/current/search-search.html) is called with the query from the previous step
This is why both Enterprise Search and Elasticsearch need to be accessed by KISS.

Follow these steps to connect your local development environment to an Elastic search instance hosted in Azure/kubernetes. 
(Note: before you follow these steps make sure you have the role "Co-Administrator" with the corresponding subscription. if you dont, search in the azure subsription and add in the tab 'Access Control', your user with the role 'Co-Administator'. this can be found under the sub tab 'classic administrator'. otherwise redo the steps below)
1. Install the azure cli
1. `az login` (once, and make sure that the account you're logging in with, has a the correct role with the corresponding subscription)
1. `az account set --subscription [...your subscriptionid...]` (once)
1. `az aks get-credentials --resource-group [for example: KISS_Kubernetes_Dev] --name [for example: KISS_Kubernetes_Dev]`
1. `kubectl config set-context --current --namespace=[for example: kiss-namespace]`
1. `kubectl port-forward service/kiss-ent-http 3002`
1. in a new window: `kubectl port-forward service/kiss-es-http 9200`

## Adding Migrations
When adding new migrations to the project, ensure you have the correct startup project selected.
You might encounter the following error message if the incorrect project has been selected:
```sh
Startup project 'docker-compose' is a Docker project. Select an ASP.NET Core Web Application as your startup project and try again.
```
To resolve this, follow these steps:
1. In the Solution Explorer, look for the kiss.bff project.
1. Right-click on kiss.bff and select 'Set as StartUp Project'.

Now, you should be able to add new migrations. The kiss.bff project contains the migrations, and setting this project as your startup project should resolve the above error.
Make sure to switch back to the original startup project configuration after adding the migrations, if required for your development process.

## OIDC authentication (Azure AD)
An OIDC identity provider is required to login to the application.
The secret that is used in the connection must be renewed periodically if Azure AD is used as the identity provider.
A redirectloop after log in is an idication the secret has expired
1. go to the appregistration.
1. go to certificates en secrets.
1. Add a secret (the value is generated automatically) and copy the value.
1. update de value of the OIDC_CLIENT_SECRET in your env.local (OIDC_CLIENT_ID is not affected)
