using System.Net;
using System.Security.Claims;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
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
        private Mock<ILogger<ClassifyingCalculationRunScenario1Controller>> _mockLogger;
        private Mock<ITokenAcquisition> _mockTokenAcquisition;
        private TelemetryClient _mockTelemetryClient;
        private ClassifyingCalculationRunScenario1Controller _controller;
        private Mock<HttpMessageHandler> _mockMessageHandler;
        private Mock<HttpContext> _mockHttpContext;

        [TestInitialize]
        public void Setup()
        {
            _mockHttpContext = new Mock<HttpContext>();
            _mockClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<ClassifyingCalculationRunScenario1Controller>>();
            _mockTokenAcquisition = new Mock<ITokenAcquisition>();
            _mockTelemetryClient = new TelemetryClient();
            _mockMessageHandler = new Mock<HttpMessageHandler>();

            _controller = new ClassifyingCalculationRunScenario1Controller(
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

            _controller = new ClassifyingCalculationRunScenario1Controller(
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
            Assert.AreEqual(ViewNames.ClassifyingCalculationRunScenario1Index, result.ViewName);
            var viewModel = result.Model as ClassifyCalculationRunScenerio1ViewModel;
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(runId, viewModel.CalculatorRunDetails.RunId);
        }

        [TestMethod]
        public async Task Submit_RedirectsToIndex_WhenModelStateIsInvalid()
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

            _controller = new ClassifyingCalculationRunScenario1Controller(
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
            ClassifyCalculationRunScenerio1ViewModel model = new ClassifyCalculationRunScenerio1ViewModel
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
            Assert.AreEqual(ViewNames.ClassifyingCalculationRunScenario1Index, result.ViewName);
            var viewModel = result.Model as ClassifyCalculationRunScenerio1ViewModel;
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(runId, viewModel.CalculatorRunDetails.RunId);
        }

        [TestMethod]
        public async Task Submit_RedirectsToClassifyRunConfirmation_WhenModelStateIsValid()
        {
            // Arrange
            int runId = 1;
            ClassifyCalculationRunScenerio1ViewModel model = new ClassifyCalculationRunScenerio1ViewModel
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
        }

        [TestMethod]
        public async Task Submit_InvalidModel_ReturnsViewResult_WithErrors()
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

            _controller = new ClassifyingCalculationRunScenario1Controller(
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
            var model = new ClassifyCalculationRunScenerio1ViewModel { CalculatorRunDetails = new CalculatorRunDetailsViewModel { RunId = 1, RunName = "Test Run" } };
            _controller.ModelState.AddModelError("ClassifyRunType", "Required");

            // Act
            var result = await _controller.Submit(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(ClassifyCalculationRunScenerio1ViewModel));
            Assert.IsTrue(_controller.ModelState.ErrorCount > 0);
        }

        [TestMethod]
        public async Task Submit_ValidModel_RedirectsToConfirmation()
        {
            // Arrange
            var model = new ClassifyCalculationRunScenerio1ViewModel
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
            Assert.AreEqual(ControllerNames.StandardErrorController, result.ControllerName);
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