namespace EPR.Calculator.Frontend.UnitTests.Controllers
{
    using System;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Text;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using EPR.Calculator.Frontend.Constants;
    using EPR.Calculator.Frontend.Controllers;
    using EPR.Calculator.Frontend.Enums;
    using EPR.Calculator.Frontend.UnitTests.HelpersTest;
    using EPR.Calculator.Frontend.UnitTests.Mocks;
    using EPR.Calculator.Frontend.ViewModels;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Identity.Web;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SendBillingFileControllerTests
    {
        private readonly IConfiguration _configuration = ConfigurationItems.GetConfigurationValues();
        private Mock<IHttpClientFactory> _mockClientFactory;
        private Mock<ILogger<SendBillingFileController>> _mockLogger;
        private TelemetryClient _mockTelemetryClient;

        public SendBillingFileControllerTests()
        {
            this.Fixture = new Fixture();
            this.MockHttpContext = new Mock<HttpContext>();
            this.MockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
        }

        private Fixture Fixture { get; }

        private Mock<HttpContext> MockHttpContext { get; }

        [TestInitialize]
        public void Setup()
        {
            _mockClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<SendBillingFileController>>();
            _mockTelemetryClient = new TelemetryClient();
        }

        [TestMethod]
        public void CanCallIndex()
        {
            var mockClient = new TelemetryClient();
            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);
            var mockHttpSession = new MockHttpSession();
            mockHttpSession.SetString("accessToken", "something");

            var context = new DefaultHttpContext()
            {
                User = principal,
                Session = mockHttpSession
            };

            var controller = new SendBillingFileController(_configuration, new Mock<ITokenAcquisition>().Object, mockClient);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);

            var model = result.Model as SendBillingFileViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual("Calculation run 99", model.CalcRunName);
            Assert.AreEqual(CommonConstants.SendBillingFile, model.SendBillFileHeading);
            Assert.AreEqual(CommonConstants.WarningContent, model.WarningContent);
            Assert.AreEqual(CommonConstants.ConfirmationContent, model.ConfirmationContent);
        }
    }
}