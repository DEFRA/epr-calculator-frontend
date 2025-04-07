namespace EPR.Calculator.Frontend.UnitTests.Controllers
{
    using System.Security.Claims;
    using System.Security.Principal;
    using AutoFixture;
    using EPR.Calculator.Frontend.Constants;
    using EPR.Calculator.Frontend.Controllers;
    using EPR.Calculator.Frontend.UnitTests.Mocks;
    using EPR.Calculator.Frontend.ViewModels;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SendBillingFileControllerTests
    {
        public SendBillingFileControllerTests()
        {
            this.Fixture = new Fixture();
            this.MockHttpContext = new Mock<HttpContext>();
            this.MockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
        }

        private Fixture Fixture { get; }

        private Mock<HttpContext> MockHttpContext { get; }

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
            var controller = new SendBillingFileController();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            // Act
            var result = controller.Index(99) as ViewResult;

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