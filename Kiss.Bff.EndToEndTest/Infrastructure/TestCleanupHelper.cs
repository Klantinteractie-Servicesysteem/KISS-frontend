using Kiss.Bff.EndToEndTest.Infrastructure.ApiClients;

namespace Kiss.Bff.EndToEndTest.Infrastructure
{
    public class TestCleanupHelper
    {
        public OpenKlantApiClient OpenKlantApiClient { get; }

        public TestCleanupHelper(OpenKlantApiClient openKlantApiClient) => OpenKlantApiClient = openKlantApiClient;

        /// <summary>
        /// Cleans up the actor and klant contact that were created during a test.
        /// Uses the provided klantContactUuid to find also find the actor, because the actor is often not known during testing
        /// </summary>
        public async Task CleanupPostKlantContacten(string klantContactUuid)
        {
            var actorKlantContact = await OpenKlantApiClient.GetActorKlantContact(klantContactUuid);
            await OpenKlantApiClient.DeleteActor(actorKlantContact.Actor.Uuid);
            await OpenKlantApiClient.DeleteKlantContact(actorKlantContact.KlantContact.Uuid);
        }
    }
}
