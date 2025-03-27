using System.Net;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
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
    public class CalculationRunDetailsNewControllerTests
    {
        private readonly IConfiguration _configuration = ConfigurationItems.GetConfigurationValues();
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private Mock<ILogger<CalculationRunDetailsNewController>> _mockLogger;
        private Mock<ITokenAcquisition> _mockTokenAcquisition;
        private TelemetryClient _telemetryClient;
        private Mock<HttpContext> _mockHttpContext;
        private CalculationRunDetailsNewController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<CalculationRunDetailsNewController>>();
            _mockTokenAcquisition = new Mock<ITokenAcquisition>();
            _telemetryClient = new TelemetryClient();
            _mockHttpContext = new Mock<HttpContext>();

            _controller = new CalculationRunDetailsNewController(
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
        public async Task IndexAsync_ReturnsRedirect_WhenApiCallFails()
        {
            // Arrange
            var runId = 1;
            var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.BadRequest, string.Empty);
            var client = new HttpClient(mockHttpMessageHandler.Object);

            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            // Mocking HttpContext.User.Identity.Name to simulate a logged-in user
            _mockHttpContext.Setup(ctx => ctx.User.Identity.Name).Returns("TestUser");

            // Act
            var result = await _controller.IndexAsync(runId);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(ActionNames.StandardErrorIndex, redirectResult.ActionName);
        }

        [TestMethod]
        public async Task IndexAsync_ReturnsView_WhenRunIsEligibleForDisplay()
        {
            // Arrange

            CalculatorRunDto calculatorRunDto = MockData.GetCalculatorRun();
            var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, calculatorRunDto);
            var client = new HttpClient(mockHttpMessageHandler.Object);

            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            // Mocking HttpContext.User.Identity.Name to simulate a logged-in user
            _mockHttpContext.Setup(ctx => ctx.User.Identity.Name).Returns("TestUser");

            // Act
            var result = await _controller.IndexAsync(calculatorRunDto.RunId);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(ViewNames.CalculationRunDetailsNewIndex, viewResult.ViewName);

            var model = viewResult.Model as CalculatorRunDetailsViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual("TestUser", model.CurrentUser); // Assuming CurrentUser is set from HttpContext.User.Identity.Name
        }

        [TestMethod]
        public async Task IndexAsync_ReturnsErrorPage_WhenRunNotEligibleForDisplay()
        {
            // Arrange
            CalculatorRunDto calculatorRunDto = MockData.GetRunningCalculatorRun();
            var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, calculatorRunDto);
            var client = new HttpClient(mockHttpMessageHandler.Object);

            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            // Mocking HttpContext.User.Identity.Name to simulate a logged-in user
            _mockHttpContext.Setup(ctx => ctx.User.Identity.Name).Returns("TestUser");

            // Act
            var result = await _controller.IndexAsync(calculatorRunDto.RunId);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(ViewNames.CalculationRunDetailsErrorPage, viewResult.ViewName);

            var model = viewResult.Model as ViewModelCommonData;
            Assert.IsNotNull(model);
            Assert.AreEqual("TestUser", model.CurrentUser); // Assuming CurrentUser is set from HttpContext.User.Identity.Name
        }

        [TestMethod]
        public async Task IndexAsync_ThrowsArgumentNullException_WhenCalculatorRunIsNull()
        {
            // Arrange
            var runId = 1;
            var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, null);
            var client = new HttpClient(mockHttpMessageHandler.Object);

            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            // Mocking HttpContext.User.Identity.Name to simulate a logged-in user
            _mockHttpContext.Setup(ctx => ctx.User.Identity.Name).Returns("TestUser");

            // Act
            var result = await _controller.IndexAsync(runId);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(ActionNames.StandardErrorIndex, redirectResult.ActionName);
        }

        private static Mock<HttpMessageHandler> CreateMockHttpMessageHandler(HttpStatusCode statusCode, object content)
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(JsonConvert.SerializeObject(content))
                });

            return mockHttpMessageHandler;
        }
    }
}
