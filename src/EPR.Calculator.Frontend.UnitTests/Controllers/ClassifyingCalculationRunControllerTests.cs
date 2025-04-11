using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Enums;
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

namespace EPR.Calculator.Frontend.UnitTests.Controllers
{
    [TestClass]
    public class ClassifyingCalculationRunControllerTests
    {
        private readonly IConfiguration _configuration = ConfigurationItems.GetConfigurationValues();
        private Mock<IHttpClientFactory> _mockClientFactory;
        private Mock<ILogger<ClassifyingCalculationRunScenario1Controller>> _mockLogger;
        private TelemetryClient _mockTelemetryClient;

        public ClassifyingCalculationRunControllerTests()
        {
            this.Fixture = new Fixture();
            this.MockHttpContext = new Mock<HttpContext>();
            this.MockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
        }

        private Fixture Fixture { get; }

        private Mock<HttpContext> MockHttpContext { get; }

        [TestInitialize]
        public void Setup()
        {
            this._mockClientFactory = new Mock<IHttpClientFactory>();
            this._mockLogger = new Mock<ILogger<ClassifyingCalculationRunScenario1Controller>>();
            this._mockTelemetryClient = new TelemetryClient();
        }

        [TestMethod]
        public async Task Index_ReturnsView_WhenApiCallIsSuccessful()
        {
            // Arrange
            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);
            var mockHttpSession = new MockHttpSession();
            mockHttpSession.SetString("accessToken", "something");

            var context = new DefaultHttpContext()
            {
                User = principal,
                Session = mockHttpSession
            };

            var controller = new ClassifyingCalculationRunScenario1Controller(_configuration, _mockClientFactory.Object,
                _mockLogger.Object, new Mock<ITokenAcquisition>().Object, this._mockTelemetryClient);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };
            controller.ControllerContext.HttpContext.Request.Scheme = "https";
            controller.ControllerContext.HttpContext.Request.Host = new HostString("localhost:7163");
            int runId = 240008;
            string calcName = "Calculation Run 99";

            // Act
            var result = controller.Index(runId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.ClassifyingCalculationRunScenario1Index, result.ViewName);
            var model = result.Model as ClassifyCalculationRunScenerio1ViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(runId, model.CalculatorRunStatus.RunId);
            Assert.AreEqual(calcName, model.CalculatorRunStatus.CalcName);
            Assert.AreEqual("12:09", model.CalculatorRunStatus.CreatedTime);
            Assert.AreEqual("01 May 2024", model.CalculatorRunStatus.CreatedDate);
        }

        [TestMethod]
        public async Task Submit_Post_ValidModelState_RedirectsToClassifyRunConfirmation()
        {
            // Arrange
            var runId = Fixture.Create<int>();

            var submitModel = Fixture.Create<ClassifyCalculationRunScenerio1SubmitViewModel>() with { RunId = runId, ClassifyRunType = ClassifyRunType.TestRun };

            var controller = new ClassifyingCalculationRunScenario1Controller(_configuration, _mockClientFactory.Object,
                _mockLogger.Object, new Mock<ITokenAcquisition>().Object, this._mockTelemetryClient);
            controller.ControllerContext.HttpContext = this.MockHttpContext.Object;

            // Act
            var result = controller.Submit(submitModel);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(ActionNames.Index, redirectResult.ActionName);
            Assert.AreEqual(ControllerNames.ClassifyRunConfirmation, redirectResult.ControllerName);
            Assert.AreEqual(runId, redirectResult.RouteValues["runId"]);
        }

        [TestMethod]
        public async Task Submit_Post_InValidModelState_RedirectsToIndex()
        {
            // Arrange
            var submitModel = Fixture.Create<ClassifyCalculationRunScenerio1SubmitViewModel>();
            var errorMessage = Fixture.Create<string>();

            var controller = new ClassifyingCalculationRunScenario1Controller(_configuration, _mockClientFactory.Object,
                _mockLogger.Object, new Mock<ITokenAcquisition>().Object, this._mockTelemetryClient);
            controller.ControllerContext.HttpContext = this.MockHttpContext.Object;

            controller.ModelState.AddModelError("Test", errorMessage);

            // Act
            var result = controller.Submit(submitModel) as ViewResult;
            var model = result.Model as ClassifyCalculationRunScenerio1ViewModel;

            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.ClassifyingCalculationRunScenario1Index, result.ViewName);
            Assert.IsNotNull(model);
            Assert.AreEqual(errorMessage, model.Errors.ErrorMessage);
            Assert.AreEqual(submitModel.RunId, model.CalculatorRunStatus.RunId);
        }

        [TestMethod]
        public async Task Submit_Post_InValidSubmitModel_RedirectsToStandardError()
        {
            // Arrange
            var controller = new ClassifyingCalculationRunScenario1Controller(_configuration, _mockClientFactory.Object,
                _mockLogger.Object, new Mock<ITokenAcquisition>().Object, this._mockTelemetryClient);
            controller.ControllerContext.HttpContext = this.MockHttpContext.Object;

            // Act
            var result = controller.Submit(null) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
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