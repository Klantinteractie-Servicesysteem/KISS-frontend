using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityModel;
using Kiss.Bff.Feedback;
using Kiss.Bff.NieuwsEnWerkinstructies.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class SendFeedbackTest : TestHelper
    {

        private SendFeedback _controller;
        private Mock<SmtpClient> _smtpClientMock;
        private Mock<IConfiguration> _configurationMock;

        [TestInitialize]
        public void Initialize()
        {
            _smtpClientMock = new Mock<SmtpClient>();
            _configurationMock = new Mock<IConfiguration>();

            _controller = new SendFeedback(_smtpClientMock.Object, _configurationMock.Object);

            // Mock the User for the controller
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(JwtClaimTypes.Name, "John Doe"),
                new Claim(JwtClaimTypes.Email, "john.doe@example.com"),
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };
        }

        [TestMethod]
        public void BuildEmailBody_ContainsUserName()
        {
            // Arrange
            var model = new FeedbackModel("Topic", "Feedback Name", new string[][] { new string[] { "Item 1", "Item 2" } });
            var userName = "John Doe";

            // Act
            var result = _controller.BuildEmailBody(userName, model);

            // Assert
            Assert.IsTrue(result.Contains(userName));
        }
    }
}
