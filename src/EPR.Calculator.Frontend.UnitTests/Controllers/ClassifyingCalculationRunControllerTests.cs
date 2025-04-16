using System.Security.Claims;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Moq;

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
        private Mock<HttpContext> _mockHttpContext;

        [TestInitialize]
        public void Setup()
        {
            _mockClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<ClassifyingCalculationRunScenario1Controller>>();
            _mockTokenAcquisition = new Mock<ITokenAcquisition>();
            _mockTelemetryClient = new TelemetryClient();
            _mockHttpContext = new Mock<HttpContext>();

            _controller = new ClassifyingCalculationRunScenario1Controller(
                   _configuration,
                   _mockClientFactory.Object,
                   _mockLogger.Object,
                   _mockTokenAcquisition.Object,
                   _mockTelemetryClient);

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "Test User")
            }));

            // Setting the mocked HttpContext for the controller
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [TestMethod]
        public void Index_ReturnsViewResult_WithValidViewModel()
        {
            // Arrange
            int runId = 1;

            // Act
            var result = _controller.Index(runId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.ClassifyingCalculationRunScenario1Index, result.ViewName);
            var viewModel = result.Model as ClassifyCalculationRunScenerio1ViewModel;
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(runId, viewModel.RunId);
        }

        [TestMethod]
        public void Submit_RedirectsToIndex_WhenModelStateIsInvalid()
        {
            // Arrange
            int runId = 1;
            ClassifyCalculationRunScenerio1ViewModel model = new ClassifyCalculationRunScenerio1ViewModel
            {
                RunId = runId,
                ClassifyRunType = ClassifyRunType.InitialRun
            };

            _controller.ModelState.AddModelError("TestError", "Test error message");

            // Act
            var result = _controller.Submit(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.ClassifyingCalculationRunScenario1Index, result.ViewName);
            var viewModel = result.Model as ClassifyCalculationRunScenerio1ViewModel;
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(runId, viewModel.RunId);
        }

        [TestMethod]
        public void Submit_RedirectsToClassifyRunConfirmation_WhenModelStateIsValid()
        {
            // Arrange
            int runId = 1;
            ClassifyCalculationRunScenerio1ViewModel model = new ClassifyCalculationRunScenerio1ViewModel
            {
                RunId = runId,
                ClassifyRunType = ClassifyRunType.InitialRun
            };

            // Act
            var result = _controller.Submit(model) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(ControllerNames.ClassifyRunConfirmation, result.ControllerName);
            Assert.AreEqual(runId, result.RouteValues["runId"]);
        }

        [TestMethod]
        public void Submit_InvalidModel_ReturnsViewResult_WithErrors()
        {
            // Arrange
            var model = new ClassifyCalculationRunScenerio1ViewModel { RunId = 1 };
            _controller.ModelState.AddModelError("ClassifyRunType", "Required");

            // Act
            var result = _controller.Submit(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(ClassifyCalculationRunScenerio1ViewModel));
            Assert.IsTrue(_controller.ModelState.ErrorCount > 0);
        }

        [TestMethod]
        public void Submit_ValidModel_RedirectsToConfirmation()
        {
            // Arrange
            var model = new ClassifyCalculationRunScenerio1ViewModel
            {
                RunId = 1,
                ClassifyRunType = ClassifyRunType.InitialRun
            };

            // Act
            var result = _controller.Submit(model) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(ControllerNames.ClassifyRunConfirmation, result.ControllerName);
        }

        [TestMethod]
        public void Submit_ExceptionThrown_RedirectsToError()
        {
            var result = _controller.Submit(null) as RedirectToActionResult;

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