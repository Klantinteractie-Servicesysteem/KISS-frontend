using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kiss.Bff.EndToEndTest.AnonymousContactmoment.Helpers;
using Kiss.Bff.EndToEndTest.AnonymousContactmomentBronnen.Helpers;
using Kiss.Bff.EndToEndTest.AnonymousContactverzoek.Helpers;


namespace Kiss.Bff.EndToEndTest.AnonymousContactmomentVerzoek
{

    [TestClass]
    public class AnonymousContactVerzoekScenarios : KissPlaywrightTest
    {
        [TestMethod("contactverzoek creation and search via telefoonnummer for an afdeling")]
        public async Task AnonymousContactVerzoekTelefoonAfdeling()
        {
            await Step("Given the user is on the Startpagina");
            await Page.GotoAsync("/");

            // await Step("When the user starts a new Contactmoment");
            // await Page.CreateNewContactmomentAsync();

            // await Step("And clicks on Contactverzoek-pane");
            // await Page.CreateNewcontactVerzoekAsync();

            // await Step("And Afdeling radiobutton is pre-selected");
            // var radio = Page.GetAfdelingRadioButton();
            // await radio.CheckAsync();
            // Assert.IsTrue(await radio.IsCheckedAsync());

            // await Step("And user selects 'parkeren' from dropdown list of field afdeling");
            // await Page.GetAfdelingCombobox().FillAsync("Parkeren");
            // await Page.GetByText("Parkeren").ClickAsync();

            // await Step("And enters 'test automation' in interne toelichting voor medewerker");
            // await Page.GetInterneToelichtingTextbox().FillAsync("test automation");

            // await Step("And enter '0617138555' in field telefoonnummer");
            // await Page.GetTelefoonnummerTextbox().FillAsync("0617138555");

            // await Step("And click on afronden");
            // await Page.GetAfrondenButton().ClickAsync();

            // await Step("And user fills in 'Hoe gaat het' in the specific vraag field");
            // await Page.GetSpecifiekeVraagTextbox().FillAsync("automation test specific vraag");

            // await Step("select channel from the list");
            // await Page.GetByLabel("Kanaal").SelectOptionAsync(new[] { new SelectOptionValue { Label = "balie" } });

            // await Step("And user clicks on Opslaan button");
            // await Page.GetOpslaanButton().ClickAsync();

            await Step("user starts a new Contactmoment and navigates to contactverzoek tab");
            await Page.CreateNewContactmomentAsync();
            await Page.GetContactverzoekenLink().ClickAsync();

            await Step("enter phone number in field Telefoonnummer of e-mailadres");
            await Page.GetContactverzoekSearchBar().FillAsync("0617138555");

            await Step("And clicks the Zoeken button");
            await Page.GetZoekenButton().ClickAsync();
            await Task.Delay(5000);

            await Step("And contactverzoek details are displayed");
            await Page.GetByText("automation test").ClickAsync();

            await Task.Delay(2000);

            var contactDetails = Page.GetByText("0617138555");
            await contactDetails.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

            Assert.IsTrue(await contactDetails.IsVisibleAsync(), "The contactverzoek details are not displayed.");

        }

        [TestMethod("contactverzoek creation and search via email for an afdeling")]
        public async Task AnonymousContactVerzoekEmailAfdeling()
        {
            await Step("Given the user is on the Startpagina");
            await Page.GotoAsync("/");

            await Step("When the user starts a new Contactmoment");
            await Page.CreateNewContactmomentAsync();

            await Step("And clicks on Contactverzoek-pane");
            await Page.CreateNewcontactVerzoekAsync();

            await Step("And Afdeling radiobutton is pre-selected");
            var radio = Page.GetAfdelingRadioButton();
            await radio.CheckAsync();
            Assert.IsTrue(await radio.IsCheckedAsync());

            await Step("And user selects 'parkeren' from dropdown list of field afdeling");
            await Page.GetAfdelingCombobox().FillAsync("Parkeren");
            await Page.GetByText("Parkeren").ClickAsync();

            await Step("And enters 'test automation' in interne toelichting voor medewerker");
            await Page.GetInterneToelichtingTextbox().FillAsync("test automation");

            // **Change here**: Filling in the email field instead of telefoonnummer.
            await Step("And enter 'testautomation@example.com' in field e-mailadres");
            await Page.GetEmailTextbox().FillAsync("testautomation@example.com");

            await Step("And click on afronden");
            await Page.GetAfrondenButton().ClickAsync();

            await Step("And user fills in 'Hoe gaat het' in the specific vraag field");
            await Page.GetSpecifiekeVraagTextbox().FillAsync("automation test specific vraag");

            await Step("select channel from the list");
            await Page.GetByLabel("Kanaal").SelectOptionAsync(new[] { new SelectOptionValue { Label = "balie" } });

            await Step("And user clicks on Opslaan button");
            await Page.GetOpslaanButton().ClickAsync();

            await Step("user starts a new Contactmoment and navigates to contactverzoek tab");
            await Page.CreateNewContactmomentAsync();
            await Page.GetContactverzoekenLink().ClickAsync();

            await Step("enter email address in field Telefoonnummer of e-mailadres");
            await Page.GetEmailTextbox().FillAsync("testautomation@example.com");

            await Step("And clicks the Zoeken button");
            await Page.GetZoekenButton().ClickAsync();

            await Step("And contactverzoek details are displayed");

            var contactDetails = Page.GetByText("testautomation@example.com");
            await contactDetails.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

            Assert.IsTrue(await contactDetails.IsVisibleAsync(), "The contactverzoek details are not displayed.");
        }

        [TestMethod("contactverzoek creation and search via telefoonnummer FOR GROUP")]

        public async Task AnonymousContactVerzoekTelefoonGroup()
        {
            await Step("Given the user is on the Startpagina");
            await Page.GotoAsync("/");

            await Step("When the user starts a new Contactmoment");
            await Page.CreateNewContactmomentAsync();

            await Step("And clicks on Contactverzoek-pane");
            await Page.CreateNewcontactVerzoekAsync();

            await Step("And selects group radiobutton");
            await Page.GetGroupRadioButton().ClickAsync();

            await Step("And user selects 'Brandweer' from dropdown list of field group");
            await Page.GetAfdelingCombobox().FillAsync("brandwee");
            await Page.GetByText("Brandweer").ClickAsync();

            await Step("And enters 'test automation' in interne toelichting voor medewerker");
            await Page.GetInterneToelichtingTextbox().FillAsync("test automation");

            await Step("And enter '0617138555' in field telefoonnummer");
            await Page.GetTelefoonnummerTextbox().FillAsync("0617138555");

            await Step("And click on afronden");
            await Page.GetAfrondenButton().ClickAsync();

            await Step("And user fills in 'Hoe gaat het' in the specific vraag field");
            await Page.GetSpecifiekeVraagTextbox().FillAsync("automation test specific vraag");

            await Step("select channel from the list");
            await Page.GetByLabel("Kanaal").SelectOptionAsync(new[] { new SelectOptionValue { Label = "balie" } });


            await Step("And user clicks on Opslaan button");
            await Page.GetOpslaanButton().ClickAsync();

            await Step("user starts a new Contactmoment and navigates to contactverzoek tab");
            await Page.CreateNewContactmomentAsync();
            await Page.GetContactverzoekenLink().ClickAsync();

            await Step("enter phone number in field Telefoonnummer of e-mailadres");
            await Page.GetTelefoonnummerTextbox().FillAsync("0617138555");

            await Step("And clicks the Zoeken button");
            await Page.GetZoekenButton().ClickAsync();
            await Step("And contactverzoek details are displayed");

            var contactDetails = Page.GetByText("0617138555");
            await contactDetails.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

            Assert.IsTrue(await contactDetails.IsVisibleAsync(), "The contactverzoek details are not displayed.");

        }

        [TestMethod("contactverzoek creation and search via email for group")]
        public async Task AnonymousContactVerzoekEmailGroup()
        {
            await Step("Given the user is on the Startpagina");
            await Page.GotoAsync("/");

            await Step("When the user starts a new Contactmoment");
            await Page.CreateNewContactmomentAsync();

            await Step("And clicks on Contactverzoek-pane");
            await Page.CreateNewcontactVerzoekAsync();

            await Step("And selects group radiobutton");
            await Page.GetGroupRadioButton().ClickAsync();

            await Step("And user selects 'Brandweer' from dropdown list of field group");
            await Page.GetAfdelingCombobox().FillAsync("brandwee");
            await Page.GetByText("Brandweer").ClickAsync();

            await Step("And enters 'test automation' in interne toelichting voor medewerker");
            await Page.GetInterneToelichtingTextbox().FillAsync("test automation");

            await Step("And enter 'testautomation@example.com' in field e-mailadres");
            await Page.GetEmailTextbox().FillAsync("testautomation@example.com");

            await Step("And click on afronden");
            await Page.GetAfrondenButton().ClickAsync();

            await Step("And user fills in 'Hoe gaat het' in the specific vraag field");
            await Page.GetSpecifiekeVraagTextbox().FillAsync("automation test specific vraag");

            await Step("select channel from the list");
            await Page.GetByLabel("Kanaal").SelectOptionAsync(new[] { new SelectOptionValue { Label = "balie" } });


            await Step("And user clicks on Opslaan button");
            await Page.GetOpslaanButton().ClickAsync();

            await Step("user starts a new Contactmoment and navigates to contactverzoek tab");
            await Page.CreateNewContactmomentAsync();
            await Page.GetContactverzoekenLink().ClickAsync();

            await Step("enter email address in field Telefoonnummer of e-mailadres");
            await Page.GetEmailTextbox().FillAsync("testautomation@example.com");

            await Step("And clicks the Zoeken button");
            await Page.GetZoekenButton().ClickAsync();

            await Step("And contactverzoek details are displayed");

            var contactDetails = Page.GetByText("testautomation@example.com");
            await contactDetails.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

            Assert.IsTrue(await contactDetails.IsVisibleAsync(), "The contactverzoek details are not displayed.");
        }

        [TestMethod("Validation of phone number field 1 in contactverzoek form")]
        public async Task AnonymousContactVerzoekTelefoonValidation()
        {
            await Step("Given the user is on the Startpagina");
            await Page.GotoAsync("/");

            await Step("When the user starts a new Contactmoment");
            await Page.CreateNewContactmomentAsync();

            await Step("And clicks on Contactverzoek-pane");
            await Page.CreateNewcontactVerzoekAsync();

            await Step("And Afdeling radiobutton is pre-selected");
            var radio = Page.GetAfdelingRadioButton();
            await radio.CheckAsync();
            Assert.IsTrue(await radio.IsCheckedAsync());

            await Step("And user selects 'parkeren' from dropdown list of field afdeling");
            await Page.GetAfdelingCombobox().FillAsync("Parkeren");
            await Page.GetByText("Parkeren").ClickAsync();

            await Step("And enters 'test automation' in interne toelichting voor medewerker");
            await Page.GetInterneToelichtingTextbox().FillAsync("test automation");

            await Step("And click on afronden");
            await Page.GetAfrondenButton().ClickAsync();

            await Step("And user fills in 'Hoe gaat het' in the specific vraag field");
            await Page.GetSpecifiekeVraagTextbox().FillAsync("automation test specific vraag");

            await Step("select channel from the list");
            await Page.GetByLabel("Kanaal").SelectOptionAsync(new[] { new SelectOptionValue { Label = "balie" } });

            await Step("And user clicks on Opslaan button");
            await Page.GetOpslaanButton().ClickAsync();

            await Step("message as vul minimal een telefoonnummer of een e-mailadres in is displayed");
            await Expect(Page.GetTelefoonnummer1field()).ToHaveJSPropertyAsync("validationMessage", "Vul minimaal een telefoonnummer of een e-mailadres in");

        }




    }


}


