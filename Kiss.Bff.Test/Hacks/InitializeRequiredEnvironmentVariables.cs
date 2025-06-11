using System.Runtime.CompilerServices;

namespace Kiss.Bff.Test.Hacks
{
    public class InitializeRequiredEnvironmentVariables
    {
        [ModuleInitializer]
        public static void Run()
        {
            Environment.SetEnvironmentVariable("MANAGEMENTINFORMATIE_API_KEY", "eenZeerGeheimeSleutelMetMinimaal32TekensLang");

            // Minimalistische dummy waarde zodat AddRegistryConfig niet crasht
            Environment.SetEnvironmentVariable("REGISTERS__0__IS_DEFAULT", "true");
            Environment.SetEnvironmentVariable("REGISTERS__0__REGISTRY_VERSION", "OpenKlant2");
            Environment.SetEnvironmentVariable("REGISTERS__0__KLANTINTERACTIE_BASE_URL", "http://unittest.local");
            Environment.SetEnvironmentVariable("REGISTERS__0__KLANTINTERACTIE_TOKEN", "unittest-token");
        }
    }
}
