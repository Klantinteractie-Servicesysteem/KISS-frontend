[![Build and Tests](https://github.com/Klantinteractie-Servicesysteem/KISS-frontend/actions/workflows/docker-image.yaml/badge.svg)](https://github.com/Klantinteractie-Servicesysteem/KISS-frontend/actions?query=workflow%3ADocker+CI+)
[![Code quality checks](https://github.com/Klantinteractie-Servicesysteem/KISS-frontend/actions/workflows/linter.yml/badge.svg)](https://github.com/Klantinteractie-Servicesysteem/KISS-frontend/actions?query=workflow%3A+code+quality+checks)
[![Known Vulnerabilities](https://snyk.io/test/github/Klantinteractie-Servicesysteem/KISS-frontend/badge.svg)](https://snyk.io/test/github/Klantinteractie-Servicesysteem/KISS-frontend)
[![Dependabot](https://img.shields.io/badge/dependabot-025E8C?style=for-the-badge&logo=dependabot&logoColor=white)](https://github.com/Klantinteractie-Servicesysteem/KISS-frontend/pulls?q=is%3Apr+author%3Aapp%2Fdependabot)
# kiss-frontend

## Set-up environment variables
1. Make a copy of .env.local.example, rename it .env.local and fill in the required secrets:
   - `OIDC_CLIENT_SECRET`, `OIDC_CLIENT_ID`, `OIDC_AUTHORITY`: use any OIDC provider. Users have access if they have a claim of either type `role` or type `roles` and a value that corresponds to the environment variable `OIDC_KLANTCONTACTMEDEWERKER_ROLE` (`Klantcontactmedewerker` by default). If you're using Azure AD, this can be done by [creating an application role and assigning it to either groups or individual users](https://learn.microsoft.com/en-us/azure/active-directory/develop/howto-add-app-roles-in-azure-ad-apps). Do the same with `OIDC_KLANTCONTACTMEDEWERKER_ROLE` (`Redacteur` by default) to enable a user to manage content in KISS.
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
