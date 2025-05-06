using System.Security.Claims;
using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
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
            this.MockMessageHandler = TestMockUtils.BuildMockMessageHandler();
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

            // Setting the mocked HttpContext for the controller
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = this._mockHttpContext.Object
            };
        }

        private Fixture Fixture { get; init; }

        private Mock<HttpMessageHandler> MockMessageHandler { get; init; }

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
            var viewModel = result.Model as SetRunClassificationViewModel;
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(runId, viewModel.RunId);
        }

        [TestMethod]
        public async Task Submit_RedirectsToIndex_WhenModelStateIsInvalid()
        {
            // Arrange
            int runId = 1;
            SetRunClassificationViewModel model = new SetRunClassificationViewModel
            {
                RunId = runId,
                ClassifyRunType = ClassifyRunType.InitialRun
            };

            _controller.ModelState.AddModelError("TestError", "Test error message");

            // Act
            var result = await _controller.Submit(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.ClassifyingCalculationRunScenario1Index, result.ViewName);
            var viewModel = result.Model as SetRunClassificationViewModel;
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(runId, viewModel.RunId);
        }

        [TestMethod]
        public async Task Submit_RedirectsToClassifyRunConfirmation_WhenSubmitSuccessful()
        {
            // Arrange
            int runId = Fixture.Create<int>();
            SetRunClassificationViewModel model = new SetRunClassificationViewModel
            {
                RunId = runId,
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
            // Arrange
            var model = new SetRunClassificationViewModel { RunId = 1 };
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
                RunId = 1,
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