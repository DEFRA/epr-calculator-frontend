using System.Net;
using System.Security.Claims;
using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using EPR.Calculator.Frontend.ViewModels;
using EPR.Calculator.Frontend.ViewModels.Enums;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.UnitTests.Controllers
{
    [TestClass]
    public class ClassifyingCalculationRunControllerTests
    {
        private readonly IConfiguration _configuration = ConfigurationItems.GetConfigurationValues();
        private Mock<IHttpClientFactory> _mockClientFactory;
        private Mock<ILogger<SetRunClassificationController>> _mockLogger;
        private Mock<ITokenAcquisition> _mockTokenAcquisition;
        private TelemetryClient _mockTelemetryClient;
        private SetRunClassificationController _controller;
        private Mock<HttpContext> _mockHttpContext;

        public ClassifyingCalculationRunControllerTests()
        {
            this.Fixture = new Fixture();
            this.MockMessageHandler = TestMockUtils.BuildMockMessageHandler(HttpStatusCode.Created);
            _mockClientFactory = TestMockUtils.BuildMockHttpClientFactory(this.MockMessageHandler.Object);

            _mockLogger = new Mock<ILogger<SetRunClassificationController>>();
            _mockTokenAcquisition = new Mock<ITokenAcquisition>();
            _mockTelemetryClient = new TelemetryClient();
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContext.Setup(context => context.Session)
                .Returns(TestMockUtils.BuildMockSession(this.Fixture).Object);
            _mockHttpContext.Setup(context => context.User)
                .Returns(new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, "Test User")
            ])));

            _controller = new SetRunClassificationController(
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
                HttpContext = this._mockHttpContext.Object
            };
        }

        private Fixture Fixture { get; init; }

        private Mock<HttpMessageHandler> MockMessageHandler { get; set; }

        [TestMethod]
        public async Task Index_ReturnsViewResult_WithValidViewModel()
        {
            // Setup
            this.MockMessageHandler
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                   {
                       StatusCode = HttpStatusCode.OK,
                       Content = new StringContent(JsonConvert.SerializeObject(MockData.GetCalculatorRun())),
                   });
            _mockClientFactory = TestMockUtils.BuildMockHttpClientFactory(this.MockMessageHandler.Object);

            _controller = new SetRunClassificationController(
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
            Assert.AreEqual(ViewNames.SetRunClassificationIndex, result.ViewName);
            var viewModel = result.Model as SetRunClassificationViewModel;
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(runId, viewModel.CalculatorRunDetails.RunId);
        }

        [TestMethod]
        public async Task IsRunEligibleForDisplay_ShouldReturnFalse_WhenRunClassificationIsUnclassified()
        {
            // Setup
            this.MockMessageHandler
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                   {
                       StatusCode = HttpStatusCode.OK,
                       Content = new StringContent(JsonConvert.SerializeObject(MockData.GetRunningCalculatorRun())),
                   });
            _mockClientFactory = TestMockUtils.BuildMockHttpClientFactory(this.MockMessageHandler.Object);

            _controller = new SetRunClassificationController(
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
            var viewModel = result.Model as SetRunClassificationViewModel;
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(runId, viewModel.CalculatorRunDetails.RunId);
        }

        [TestMethod]
        public async Task Submit_RedirectsToIndex_WhenModelStateIsInvalid()
        {
            // Setup
            this.MockMessageHandler
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                   {
                       StatusCode = HttpStatusCode.OK,
                       Content = new StringContent(JsonConvert.SerializeObject(MockData.GetCalculatorRun())),
                   });
            _mockClientFactory = TestMockUtils.BuildMockHttpClientFactory(this.MockMessageHandler.Object);

            _controller = new SetRunClassificationController(
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
            SetRunClassificationViewModel model = new SetRunClassificationViewModel
            {
                CalculatorRunDetails = new CalculatorRunDetailsViewModel
                {
                    RunId = runId,
                    RunName = "Test Run"
                },
                ClassifyRunType = ClassifyRunType.InitialRun
            };

            _controller.ModelState.AddModelError("TestError", "Test error message");

            // Act
            var result = await _controller.Submit(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.SetRunClassificationIndex, result.ViewName);
            var viewModel = result.Model as SetRunClassificationViewModel;
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(runId, viewModel.CalculatorRunDetails.RunId);
        }

        [TestMethod]
        public async Task Submit_RedirectsToClassifyRunConfirmation_WhenSubmitSuccessful()
        {
            // Arrange
            int runId = Fixture.Create<int>();
            SetRunClassificationViewModel model = new SetRunClassificationViewModel
            {
                CalculatorRunDetails = new CalculatorRunDetailsViewModel
                {
                    RunId = runId,
                    RunName = "Test Run"
                },
                ClassifyRunType = ClassifyRunType.InitialRun
            };

            // Act
            var result = await _controller.Submit(model) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(ControllerNames.ClassifyRunConfirmation, result.ControllerName);
            Assert.AreEqual(runId, result.RouteValues["runId"]);
            this.MockMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(m =>
                    m.Content.ReadAsStringAsync().Result.Contains(
                        $"\"RunId\":{runId},\"ClassificationId\":{(int)ClassifyRunType.InitialRun}")),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task Submit_InvalidModel_ReturnsViewResult_WithErrors()
        {
            // Setup
            this.MockMessageHandler
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                   {
                       StatusCode = HttpStatusCode.OK,
                       Content = new StringContent(JsonConvert.SerializeObject(MockData.GetCalculatorRun())),
                   });
            _mockClientFactory = TestMockUtils.BuildMockHttpClientFactory(this.MockMessageHandler.Object);

            _controller = new SetRunClassificationController(
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
            var model = new SetRunClassificationViewModel { CalculatorRunDetails = new CalculatorRunDetailsViewModel { RunId = 1, RunName = "Test Run" } };
            _controller.ModelState.AddModelError("ClassifyRunType", "Required");

            // Act
            var result = await _controller.Submit(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(SetRunClassificationViewModel));
            Assert.IsTrue(_controller.ModelState.ErrorCount > 0);
        }

        [TestMethod]
        public async Task Submit_ValidModel_RedirectsToConfirmation()
        {
            // Arrange
            var model = new SetRunClassificationViewModel
            {
                CalculatorRunDetails = new CalculatorRunDetailsViewModel
                {
                    RunId = 1,
                    RunName = "Test Run"
                },
                ClassifyRunType = ClassifyRunType.InitialRun
            };

            // Act
            var result = await _controller.Submit(model) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(ControllerNames.ClassifyRunConfirmation, result.ControllerName);
        }

        [TestMethod]
        public async Task Submit_ExceptionThrown_RedirectsToError()
        {
            var result = await _controller.Submit(null) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(CommonUtil.GetControllerName(typeof(StandardErrorController)), result.ControllerName);
            _mockLogger.Verify(
                x =>
           x.Log(
               LogLevel.Error,
               It.IsAny<EventId>(),
               It.Is<It.IsAnyType>((v, t) => true), // We can't match internal LogState
               It.IsAny<Exception>(),
               (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.AtLeastOnce);
        }

        [TestMethod]
        public async Task Submit_Http500_RedirectsToError()
        {
            // Arrange
            this.MockMessageHandler = TestMockUtils.BuildMockMessageHandler(HttpStatusCode.InternalServerError);
            this.MockMessageHandler
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                   {
                       StatusCode = HttpStatusCode.InternalServerError,
                       Content = new StringContent(JsonConvert.SerializeObject(MockData.GetCalculatorRun())),
                   });
            _mockClientFactory = TestMockUtils.BuildMockHttpClientFactory(this.MockMessageHandler.Object);

            _controller = new SetRunClassificationController(
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

            var result = await _controller.Submit(Fixture.Create<SetRunClassificationViewModel>()) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(CommonUtil.GetControllerName(typeof(StandardErrorController)), result.ControllerName);
            _mockLogger.Verify(
                x =>
           x.Log(
               LogLevel.Error,
               It.IsAny<EventId>(),
               It.Is<It.IsAnyType>((v, t) => true), // We can't match internal LogState
               It.IsAny<Exception>(),
               (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.AtLeastOnce);
        }
    }
}