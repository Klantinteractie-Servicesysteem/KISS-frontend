using System.Reflection.Metadata.Ecma335;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

namespace Kiss.Bff.EndToEndTest.AnonymousContactmomentBronnen.Helpers
{
    public static class contactverzoekLocators
    {
        public static ILocator GetAfdelingRadioButton(this IPage page)
        {
            return page.GetByRole(AriaRole.Radio, new() { Name = "Afdeling" });
        }

        public static ILocator GetGroupRadioButton(this IPage page)
        {
            return page.GetByRole(AriaRole.Radio, new() { Name = "Groep" });
        }

        public static ILocator GetMedewerkerRadioButton(this IPage page)
        {
            return page.GetByRole(AriaRole.Radio, new() { Name = "Medewerker" });
        }

        public static ILocator GetAfdelingCombobox(this IPage page)
        {
            return page.GetByRole(AriaRole.Combobox, new() { Name = "Afdeling*" });
        }

        public static ILocator GetGroupCombobox(this IPage page)
        {
            return page.GetByRole(AriaRole.Combobox, new() { Name = "Groep*" });
        }


        public static ILocator GetInterneToelichtingTextbox(this IPage page)
        {
            return page.GetByRole(AriaRole.Textbox, new() { Name = "Interne toelichting voor" });
        }

        public static ILocator GetSpecifiekeVraagTextbox(this IPage page)
        {
            return page.GetByRole(AriaRole.Textbox, new() { Name = "Specifieke vraag *" });
        }

        public static ILocator GetTelefoonnummerTextbox(this IPage page)
        {
            return page.GetByRole(AriaRole.Textbox, new() { Name = "Telefoonnummer 1" });
        }

        public static ILocator GetEmailTextbox(this IPage page)
        {
            return page.GetByRole(AriaRole.Textbox, new() { Name = "E-mailadres" });
        }

        public static ILocator GetAfdelingTextbox(this IPage page)
        {
            return page.GetByLabel("Afdeling / groep Afdeling:");
        }


        public static ILocator GetAfrondenButton(this IPage page)
        {
            return page.GetByRole(AriaRole.Button, new() { Name = "Afronden" });
        }

        public static ILocator GetOpslaanButton(this IPage page)
        {
            return page.GetByRole(AriaRole.Button, new() { Name = "Opslaan" });
        }

        public static ILocator GetTelefoonnummer1field(this IPage page)
        {
            return page.GetByLabel("Telefoonnummer 1");
        }

        public static ILocator GetTelefoonnummer2field(this IPage page)
        {
            return page.GetByLabel("Telefoonnummer 2", new() { Exact = true });
        }

        public static ILocator GetEmailfield(this IPage page)
        {
            return page.GetByLabel("E-mailadres");
        }
        public static ILocator GetContactverzoekenLink(this IPage page)
        {
            return page.GetByRole(AriaRole.Link, new() { Name = "Contactverzoeken" });
        }

        public static ILocator GetContactverzoekSearchBar(this IPage page)
        {
            return page.GetByRole(AriaRole.Textbox, new() { Name = "Telefoonnummer of e-mailadres" });

        }

        public static ILocator GetZoekenButton(this IPage page)
        {
            return page.GetByRole(AriaRole.Main).GetByRole(AriaRole.Button, new() { Name = "Zoeken" });
        }

        public static ILocator GetContactVerzoekSuccessToast(this IPage page)
        {
            return page.Locator("output[role='status'].confirm");
        }
    }
}