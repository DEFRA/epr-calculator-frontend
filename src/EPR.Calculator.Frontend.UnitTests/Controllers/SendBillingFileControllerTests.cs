using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests.Controllers
{
    [TestClass]
    public class SendBillingFileControllerTests
    {
        private readonly IConfiguration _configuration = ConfigurationItems.GetConfigurationValues();
        private Mock<ITokenAcquisition> _mockTokenAcquisition;
        private TelemetryClient _telemetryClient;
        private SendBillingFileController _controller;
        private Mock<HttpContext> _mockHttpContext;

        public SendBillingFileControllerTests()
        {
            _mockTokenAcquisition = new Mock<ITokenAcquisition>();
            _telemetryClient = new TelemetryClient();
            _mockHttpContext = new Mock<HttpContext>();

            _controller = new SendBillingFileController(
                   _configuration,
                   _mockTokenAcquisition.Object,
                   _telemetryClient,
                   new Mock<IHttpClientFactory>().Object)
            {
                // Setting the mocked HttpContext for the controller
                ControllerContext = new ControllerContext
                {
                    HttpContext = _mockHttpContext.Object
                }
            };
        }

        [TestMethod]
        public void CanCallIndex()
        {
            int runId = 99;

            // Mocking HttpContext.User.Identity.Name to simulate a logged-in user
            _mockHttpContext.Setup(ctx => ctx.User.Identity.Name).Returns("TestUser");

            var result = _controller.Index(runId);

            // Assert
            Assert.IsNotNull(result);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.IsInstanceOfType(viewResult.Model, typeof(SendBillingFileViewModel));
        }

        [TestMethod]
        public void Submit_ModelStateInvalid_RedirectsToIndex()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Model state is invalid");
            int runId = 1;

            // Act
            var result = _controller.Submit(runId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(runId, result.RouteValues["runId"]);
        }

        [TestMethod]
        public void Submit_ModelStateValid_RedirectsToBillingFileSuccess()
        {
            // Arrange
            int runId = 1;

            // Act
            var result = _controller.Submit(runId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.BillingFileSuccess, result.ActionName);
            Assert.AreEqual(ControllerNames.PaymentCalculator, result.ControllerName);
        }
    }
}