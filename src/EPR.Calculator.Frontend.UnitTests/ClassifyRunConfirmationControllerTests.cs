using System.Net;
using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
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
    public class ClassifyRunConfirmationControllerTests
    {
        private readonly IConfiguration _configuration = ConfigurationItems.GetConfigurationValues();
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private Mock<ILogger<ClassifyRunConfirmationController>> _mockLogger;
        private Mock<ITokenAcquisition> _mockTokenAcquisition;
        private TelemetryClient _telemetryClient;
        private Mock<HttpContext> _mockHttpContext;
        private ClassifyRunConfirmationController _controller;

        public ClassifyRunConfirmationControllerTests()
        {
            this.Fixture = new Fixture();
            this.MockHttpContext = new Mock<HttpContext>();
            this.MockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);

            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<ClassifyRunConfirmationController>>();
            _mockTokenAcquisition = new Mock<ITokenAcquisition>();
            _telemetryClient = new TelemetryClient();
            _mockHttpContext = new Mock<HttpContext>();
        }

        private Fixture Fixture { get; }

        private Mock<HttpContext> MockHttpContext { get; }

        [TestInitialize]
        public void Setup()
        {
            _controller = new ClassifyRunConfirmationController(
               _configuration,
               _mockHttpClientFactory.Object,
               _mockLogger.Object,
               _mockTokenAcquisition.Object,
               _telemetryClient)
            {
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

            // Act
            var result = _controller.Index(runId);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(ActionNames.StandardErrorIndex, redirectResult.ActionName);
        }

        [TestMethod]
        public async Task IndexAsync_ReturnsView_WhenRunExists()
        {
            // Arrange
            CalculatorRunDto calculatorRunDto = MockData.GetCalculatorRun();
            var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, calculatorRunDto);
            var client = new HttpClient(mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);
            _mockHttpContext.Setup(ctx => ctx.User.Identity.Name).Returns("TestUser");

            // Act
            var result = _controller.Index(calculatorRunDto.RunId);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(ViewNames.ClassifyRunConfirmationIndex, viewResult.ViewName);

            var model = viewResult.Model as ClassifyRunConfirmationViewModel;
            Assert.IsNotNull(model);
        }

        [TestMethod]
        public void Submit_RedirectsToIndex_WhenModelStateIsInvalid()
        {
            // Arrange
            int runId = 1;
            _controller.ModelState.AddModelError("Error", "Invalid model state");

            // Act
            var result = _controller.Submit(runId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(runId, result.RouteValues["runId"]);
        }

        [TestMethod]
        public void Submit_RedirectsToPaymentCalculatorIndex_WhenModelStateIsValid()
        {
            // Arrange
            int runId = 1;

            // Act
            var result = _controller.Submit(runId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(ControllerNames.PaymentCalculator , result.ControllerName);
            Assert.AreEqual(runId, result.RouteValues["runId"]);
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