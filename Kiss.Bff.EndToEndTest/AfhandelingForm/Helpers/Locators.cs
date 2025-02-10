﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kiss.Bff.EndToEndTest.AfhandelingForm.Helpers
{
    internal static class Locators
    {

        public static ILocator GetNieuwContactmomentButton(this IPage page)
        {
            return page.GetByRole(AriaRole.Button, new() { Name = "Nieuw contactmoment" });
        }
        public static ILocator GetPersonenNotitieblokTextbox(this IPage page)
        {
            return page.GetByRole(AriaRole.Tabpanel, new() { Name = "Notitieblok" }).GetByRole(AriaRole.Textbox);
        }
         
        public static ILocator GetPersonenAfrondenButton(this IPage page)
        {
            return page.GetByRole(AriaRole.Button, new() { Name = "Afronden" });
        }
         
        public static ILocator GetAfhandelingNotitieTextBox(this IPage page)
        {
            return page.GetByRole(AriaRole.Textbox, new() { Name = "Notitie" });
        }
 

        public static ILocator GetOpslaanButton(this IPage page)
        {
            return page.GetByRole(AriaRole.Button, new() { Name = "Opslaan" });
        }

        public static ILocator GetAfhandelingForm(this IPage page)
        {
            return  page.Locator("form.afhandeling"); 
        }

        public static ILocator GetSpecificVraagField(this IPage page)
        {
            return page.GetByRole(AriaRole.Textbox, new() { Name = "Specifieke vraag *" });
        }

        public static ILocator GetKanaalField(this IPage page)
        {
            return page.GetByLabel("Kanaal" );
        }

        public static ILocator GetAfhandelingField(this IPage page)
        {
            return page.GetByRole(AriaRole.Combobox, new() { Name = "Afhandeling" });
        }

        public static ILocator GetAfdelingField(this IPage page)
        {
            return page.GetByRole(AriaRole.Combobox, new() { Name = "Afdeling" });
        }
        public static ILocator GetAfdelingVoorField(this IPage page)
        {
            return page.Locator("input[type='search']");
        }

        public static ILocator GetAfhandelingSuccessToast(this IPage page)
        {
            return page.Locator("output[role='status'].confirm"); 
        }
    }
}
