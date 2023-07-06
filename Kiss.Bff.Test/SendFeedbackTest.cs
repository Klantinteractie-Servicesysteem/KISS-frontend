using Kiss.Bff.Feedback;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class SendFeedbackTest : TestHelper
    {
        [TestMethod]
        public void BuildEmailBody_ContainsUserName()
        {
            // Arrange
            var model = new FeedbackModel("Topic", "Feedback Name", new string[][] { new string[] { "Item 1", "Item 2" } });
            var userName = "John Doe";

            // Act
            var result = SendFeedback.BuildEmailBody(userName, model);

            // Assert
            Assert.IsTrue(result.Contains(userName));
        }
    }
}
