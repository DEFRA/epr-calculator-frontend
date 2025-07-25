using System.Net;
using System.Security.Claims;
using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
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
            this.Fixture = new Fixture();
            _mockHttpContext = new Mock<HttpContext>();
            _mockClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<ClassifyRunConfirmationController>>();
            _mockTokenAcquisition = new Mock<ITokenAcquisition>();
            _mockTelemetryClient = new TelemetryClient();
            _mockMessageHandler = new Mock<HttpMessageHandler>();

            _controller = new ClassifyRunConfirmationController(
                       _configuration,
                       new Mock<IApiService>().Object,
                       _mockTokenAcquisition.Object,
                       _mockTelemetryClient,
                       new Mock<ICalculatorRunDetailsService>().Object);

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

        private Fixture Fixture { get; init; }

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
                       Content = new StringContent(JsonConvert.SerializeObject(MockData.GetCalculatorRunWithInitialRun())),
                   });
            _mockClientFactory = TestMockUtils.BuildMockHttpClientFactory(_mockMessageHandler.Object);

            _controller = new ClassifyRunConfirmationController(
                _configuration,
                new Mock<IApiService>().Object,
                _mockTokenAcquisition.Object,
                _mockTelemetryClient,
                new Mock<ICalculatorRunDetailsService>().Object);

            // Setting the mocked HttpContext for the controller
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            };

            // Arrange
            var data = MockData.GetCalculatorRunWithInitialRun();
            var controller = BuildTestClass(
                HttpStatusCode.OK,
                data,
                new CalculatorRunDetailsViewModel
                {
                    RunId = data.RunId,
                    RunClassificationId = (RunClassification)data.RunClassificationId,
                });

            // Act
            var result = await controller.Index(data.RunId) as ViewResult;
            var resultViewModel = result.Model as ClassifyRunConfirmationViewModel;

            // Assert
            Assert.AreEqual(ViewNames.ClassifyRunConfirmationIndex, result.ViewName);
            Assert.AreEqual(data.RunId, resultViewModel.CalculatorRunDetails.RunId);
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
                new Mock<IApiService>().Object,
                _mockTokenAcquisition.Object,
                _mockTelemetryClient,
                new Mock<ICalculatorRunDetailsService>().Object);

            // Setting the mocked HttpContext for the controller
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            };

            // Arrange
            int runId = 1;

            // Act
            var result = await _controller.Index(runId);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(ActionNames.StandardErrorIndex, redirectResult.ActionName);
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
                new Mock<IApiService>().Object,
                _mockTokenAcquisition.Object,
                _mockTelemetryClient,
                new Mock<ICalculatorRunDetailsService>().Object);

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
        public void Submit_RedirectsToBillingInstructionsIndex_WhenModelStateIsValid()
        {
            // Arrange
            int runId = 1;

            // Act
            var result = _controller.Submit(runId) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(RouteNames.BillingInstructionsIndex, result.RouteName);
            Assert.AreEqual(runId, result.RouteValues["calculationRunId"]);
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

        private ClassifyRunConfirmationController BuildTestClass(
            HttpStatusCode httpStatusCode,
            CalculatorRunDto data = null,
            CalculatorRunDetailsViewModel details = null)
        {
            data = data ?? MockData.GetCalculatorRun();
            details = details ?? Fixture.Create<CalculatorRunDetailsViewModel>();
            var mockApiService = TestMockUtils.BuildMockApiService(
                httpStatusCode,
                System.Text.Json.JsonSerializer.Serialize(data ?? MockData.GetCalculatorRun())).Object;

            var testClass = new ClassifyRunConfirmationController(
                ConfigurationItems.GetConfigurationValues(),
                mockApiService,
                _mockTokenAcquisition.Object,
                _mockTelemetryClient,
                TestMockUtils.BuildMockCalculatorRunDetailsService(details).Object);
            testClass.ControllerContext.HttpContext = new DefaultHttpContext();

            return testClass;
        }
    }
}