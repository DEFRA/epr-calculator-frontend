using System.Net;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class CalculationRunOverviewControllerTests
    {
        private readonly IConfiguration _configuration = ConfigurationItems.GetConfigurationValues();
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private Mock<ILogger<CalculationRunOverviewController>> _mockLogger;
        private Mock<ITokenAcquisition> _mockTokenAcquisition;
        private TelemetryClient _telemetryClient;
        private CalculationRunOverviewController _controller;
        private Mock<HttpContext> _mockHttpContext;
        private Mock<HttpMessageHandler> _mockMessageHandler;

        public CalculationRunOverviewControllerTests()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<CalculationRunOverviewController>>();
            _mockTokenAcquisition = new Mock<ITokenAcquisition>();
            _telemetryClient = new TelemetryClient();
            _mockHttpContext = new Mock<HttpContext>();
            _mockMessageHandler = new Mock<HttpMessageHandler>();

            _controller = new CalculationRunOverviewController(
                   _configuration,
                   _mockHttpClientFactory.Object,
                   _mockLogger.Object,
                   _mockTokenAcquisition.Object,
                   _telemetryClient)
            {
                // Setting the mocked HttpContext for the controller
                ControllerContext = new ControllerContext
                {
                    HttpContext = _mockHttpContext.Object
                }
            };
        }

        [TestMethod]
        public async Task IndexAsync_ReturnsViewResult_WithValidViewModel()
        {
            // Setup
            _mockMessageHandler
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                   {
                       StatusCode = HttpStatusCode.OK,
                       Content = new StringContent(JsonConvert.SerializeObject(MockData.GetCalculatorRun())),
                   });
            _mockHttpClientFactory = TestMockUtils.BuildMockHttpClientFactory(_mockMessageHandler.Object);

            _controller = new CalculationRunOverviewController(
                _configuration,
                _mockHttpClientFactory.Object, _mockLogger.Object,
                _mockTokenAcquisition.Object,
                _telemetryClient);

            // Setting the mocked HttpContext for the controller
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            };

            // Mocking HttpContext.User.Identity.Name to simulate a logged-in user
            _mockHttpContext.Setup(ctx => ctx.User.Identity.Name).Returns("TestUser");

            var result = await _controller.Index(10);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.IsInstanceOfType(viewResult.Model, typeof(CalculatorRunOverviewViewModel));
        }

        [TestMethod]
        public void Submit_ModelStateInvalid_RedirectsToIndex()
        {
            // Mocking HttpContext.User.Identity.Name to simulate a logged-in user
            _mockHttpContext.Setup(ctx => ctx.User.Identity.Name).Returns("TestUser");

            // Arrange
            _controller.ModelState.AddModelError("Error", "Model state is invalid");

            // Act
            var result = _controller.Submit(1) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(1, result.RouteValues["runId"]);
        }

        [TestMethod]
        public void Submit_ModelStateValid_RedirectsToSendBillingFile()
        {
            // Mocking HttpContext.User.Identity.Name to simulate a logged-in user
            _mockHttpContext.Setup(ctx => ctx.User.Identity.Name).Returns("TestUser");

            // Act
            var result = _controller.Submit(1) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(ControllerNames.SendBillingFile, result.ControllerName);
            Assert.AreEqual(1, result.RouteValues["runId"]);
        }
    }
}
