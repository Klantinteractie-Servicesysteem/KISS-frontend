# End to end tests
These tests are scheduled to run on Github Actions. [An html report](https://klantinteractie-servicesysteem.github.io/KISS-frontend/) is generated on Github Pages.

## Run/debug the tests locally
1. In Visual Studio, right click the `Kiss.Bff.EndToEndTest` project and select `Manage user secrets`
1. Fill in the following values:
```jsonc
{
  "TestSettings": {
    "TEST_BASE_URL": "", // a valid base url for an environment where an instance of kiss is running
    "TEST_USERNAME": "", // a valid username to login to Azure Entra Id
    "TEST_PASSWORD": "", // a valid password to login to Azure Entra Id
    "TEST_TOTP_SECRET": "", // a secret to generate 2 Factor Authentication codes for Azure Entra Id
    "TEST_OPEN_KLANT_BASE_URL": "https://openklant.dev.kiss-demo.nl/klantinteracties/api/v1/", // the open klant api base url including domain + /klantinteracies/api/v1/
    "TEST_OPEN_KLANT_SECRET": "" the API secret for the open klant api
  
  }
}
```