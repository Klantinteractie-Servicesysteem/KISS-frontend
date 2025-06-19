
using Kiss.Bff.EndToEndTest.NieuwsEnWerkInstructies.Helpers;
using Kiss.Bff.EndToEndTest.AfhandelingForm.Helpers;
using Kiss.Bff.EndToEndTest.AnonymousContactmomentBronnen.Helpers;


namespace Kiss.Bff.EndToEndTest.Beheer
{
    [TestClass]
    public class SkillScenarios : KissPlaywrightTest
    {
        [TestMethod("1. Navigation to Skill page")]
        public async Task NavigationKanalen()
        {
            await Step("Given the user navigates to the Beheer tab");
            await Page.GotoAsync("/");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Beheer" }).ClickAsync();

            await Step("When the user clicks on Skills tab");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Skills" }).ClickAsync();

            await Step("Then list of skills are displayed");
            await Expect(Page.GetByRole(AriaRole.Listitem)).ToBeVisibleAsync();
        }

        [TestMethod("2. Add a New Skill")]
        [DataRow("Automation Skill")]
        public async Task AddNewSkill(string skillName)
        {
            Skill skill = null;

            try
            {
                await Step($"When the user adds a new skill with the name '{skillName}'");
                skill = await Page.CreateSkill(skillName);
            }
            finally
            {
                if (skill != null)
                {
                    await skill.DisposeAsync();
                }
            }
        }


        [TestMethod("3. Editing an existing Skill")]
        [DataRow("Automation skill edit")]
        public async Task EditSkill(string skillName)
        {
            string updatedSkillName = "Automation skill Updated";


            Skill originalSkill = null;
            Skill updatedSkill = null;


            {
                await Step("Given the user navigates to the Skills management page");
                await Page.NavigateToSkillsBeheer();

                await Step($"When the user adds a new skill with the name '{skillName}'");
                originalSkill = await Page.CreateSkill(skillName);

                await Step($"When user clicks on skill list with name as '{skillName}'");
                await Page.GetByRole(AriaRole.Link, new() { Name = skillName }).ClickAsync();

                await Step($"And user updates title to '{updatedSkillName}'");
                await Page.GetByRole(AriaRole.Textbox, new() { Name = "Naam" }).FillAsync(updatedSkillName);

                await Step("And user clicks on Opslaan");
                await Page.GetOpslaanButton().ClickAsync();

                await Step($"And updated skill '{updatedSkillName}' is added to the list of Skills");
                updatedSkill = new Skill(Page) { Naam = updatedSkillName };

                await Expect(Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = updatedSkillName })).ToBeVisibleAsync();
                await updatedSkill.DisposeAsync();
            }

        }

        [TestMethod("4. Deleting a Skill")]
        [DataRow("Automation Skill To Delete")]
        public async Task DeleteSkill(string skillName)
        {
            Skill skill = null;

            try
            {
                await Step("When the user creates a new skill");
                skill = await Page.CreateSkill(skillName);
            }
            finally
            {
                if (skill != null)
                {
                    await Step("Then the user deletes the skill");
                    await skill.DisposeAsync();

                    await Step("And verifies that the skill is no longer visible");
                    var isVisible = await Page.GetByRole(AriaRole.Listitem)
                        .Filter(new() { HasText = skillName })
                        .IsVisibleAsync();

                    Assert.IsFalse(isVisible, $"Skill '{skillName}' should be deleted but is still visible.");
                }
            }
        }

    }
}
