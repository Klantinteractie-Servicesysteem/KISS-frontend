using Kiss.Bff.EndToEndTest.Infrastructure.ApiClients;
using Kiss.Bff.EndToEndTest.Infrastructure.ApiClients.Dtos;

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
        public async Task CleanupPostKlantContacten(IResponse klantContactPostResponse)
        {
            var klantContactUuid = await klantContactPostResponse.JsonAsync<UuidDto>();

            var actorKlantContact = await OpenKlantApiClient.GetActorKlantContact(klantContactUuid.Value);
            await OpenKlantApiClient.DeleteActor(actorKlantContact.Actor.Uuid);
            await OpenKlantApiClient.DeleteKlantContact(actorKlantContact.KlantContact.Uuid);
        }
    }
}
