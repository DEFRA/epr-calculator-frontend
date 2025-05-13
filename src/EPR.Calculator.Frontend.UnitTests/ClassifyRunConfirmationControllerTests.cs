using System.Net;
using System.Security.Claims;
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
using Microsoft.Azure.Documents.SystemFunctions;
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
        private Mock<IHttpClientFactory> _mockClientFactory;
        private Mock<ILogger<ClassifyRunConfirmationController>> _mockLogger;
        private Mock<ITokenAcquisition> _mockTokenAcquisition;
        private TelemetryClient _mockTelemetryClient;
        private ClassifyRunConfirmationController _controller;
        private Mock<HttpMessageHandler> _mockMessageHandler;
        private Mock<HttpContext> _mockHttpContext;

        public ClassifyRunConfirmationControllerTests()
        {
            _mockHttpContext = new Mock<HttpContext>();
            _mockClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<ClassifyRunConfirmationController>>();
            _mockTokenAcquisition = new Mock<ITokenAcquisition>();
            _mockTelemetryClient = new TelemetryClient();
            _mockMessageHandler = new Mock<HttpMessageHandler>();

            _controller = new ClassifyRunConfirmationController(
                       _configuration,
                       _mockClientFactory.Object,
                       _mockLogger.Object,
                       _mockTokenAcquisition.Object,
                       _mockTelemetryClient);

            _mockHttpContext.Setup(context => context.User)
               .Returns(new ClaimsPrincipal(new ClaimsIdentity(
           [
               new Claim(ClaimTypes.Name, "Test User")
           ])));

            // Setting the mocked HttpContext for the controller
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            };
        }

        [TestMethod]
        public async Task Index_ReturnsViewResult_WithValidViewModel()
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
            _mockClientFactory = TestMockUtils.BuildMockHttpClientFactory(_mockMessageHandler.Object);

            _controller = new ClassifyRunConfirmationController(
                _configuration,
                _mockClientFactory.Object,
                _mockLogger.Object,
                _mockTokenAcquisition.Object,
                _mockTelemetryClient);

            // Setting the mocked HttpContext for the controller
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            };

            // Arrange
            int runId = 1;

            // Act
            var result = await _controller.Index(runId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.ClassifyRunConfirmationIndex, result.ViewName);
            var viewModel = result.Model as ClassifyRunConfirmationViewModel;
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(runId, viewModel.CalculatorRunDetails.RunId);
        }

        [TestMethod]
        public async Task IsRunEligibleForDisplay_ShouldReturnFalse_WhenRunClassificationIsUnclassified()
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
                       Content = new StringContent(JsonConvert.SerializeObject(MockData.GetRunningCalculatorRun())),
                   });
            _mockClientFactory = TestMockUtils.BuildMockHttpClientFactory(_mockMessageHandler.Object);

            _controller = new ClassifyRunConfirmationController(
                _configuration,
                _mockClientFactory.Object,
                _mockLogger.Object,
                _mockTokenAcquisition.Object,
                _mockTelemetryClient);

            // Setting the mocked HttpContext for the controller
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            };

            // Arrange
            int runId = 1;

            // Act
            var result = await _controller.Index(runId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunDetailsNewErrorPage, result.ViewName);
            var viewModel = result.Model as ClassifyRunConfirmationViewModel;
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(runId, viewModel.CalculatorRunDetails.RunId);
        }

        [TestMethod]
        public async Task Index_WhenCalculatorRunDetailsIsNullOrRunIdIsZero_RedirectsToStandardError()
        {
            // Setup
            _mockMessageHandler
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                   {
                       StatusCode = HttpStatusCode.NotFound,
                       Content = null,
                   });
            _mockClientFactory = TestMockUtils.BuildMockHttpClientFactory(_mockMessageHandler.Object);

            _controller = new ClassifyRunConfirmationController(
                _configuration,
                _mockClientFactory.Object,
                _mockLogger.Object,
                _mockTokenAcquisition.Object,
                _mockTelemetryClient);

            // Setting the mocked HttpContext for the controller
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            };
            // Act
            var result = await _controller.Index(123);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);            
            Assert.AreEqual(ActionNames.StandardErrorIndex, redirectResult.ActionName);
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
            Assert.AreEqual(ControllerNames.PaymentCalculator, result.ControllerName);
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