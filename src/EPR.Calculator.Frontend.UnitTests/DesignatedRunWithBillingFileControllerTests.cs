using System.Net;
using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
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
    public class DesignatedRunWithBillingFileControllerTests
    {
        private readonly IConfiguration _configuration = ConfigurationItems.GetConfigurationValues();
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private Mock<ILogger<DesignatedRunWithBillingFileController>> _mockLogger;
        private Mock<ITokenAcquisition> _mockTokenAcquisition;
        private TelemetryClient _telemetryClient;
        private DesignatedRunWithBillingFileController _controller;
        private Mock<HttpContext> _mockHttpContext;
        private Mock<HttpMessageHandler> _mockMessageHandler;
        private Mock<ICalculatorRunDetailsService> _mockCalculatorRunDetailsService;

        public DesignatedRunWithBillingFileControllerTests()
        {
            this.Fixture = new Fixture();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<DesignatedRunWithBillingFileController>>();
            _mockTokenAcquisition = new Mock<ITokenAcquisition>();
            _telemetryClient = new TelemetryClient();
            _mockHttpContext = new Mock<HttpContext>();
            _mockMessageHandler = new Mock<HttpMessageHandler>();
            _mockCalculatorRunDetailsService = new Mock<ICalculatorRunDetailsService>();

            _controller = new DesignatedRunWithBillingFileController(
                   _configuration,
                   new Mock<IApiService>().Object,
                   _mockTokenAcquisition.Object,
                   _telemetryClient,
                   _mockCalculatorRunDetailsService.Object)
            {
                // Setting the mocked HttpContext for the controller
                ControllerContext = new ControllerContext
                {
                    HttpContext = _mockHttpContext.Object
                }
            };
        }

        private Fixture Fixture { get; init; }

        [TestMethod]
        public async Task IndexAsync_InvalidRunId_RedirectsToStandardError()
        {
            // Arrange
            int invalidRunId = -1;
            _mockHttpContext.Setup(ctx => ctx.User.Identity.Name).Returns("TestUser");

            // Act
            var result = await _controller.Index(invalidRunId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual(CommonUtil.GetControllerName(typeof(StandardErrorController)), result.ControllerName);
        }

        [TestMethod]
        public async Task IndexAsync_ReturnsViewResult_WithValidViewModel()
        {
            // Arrange
            var controller = BuildTestClass(HttpStatusCode.OK, MockData.GetCalculatorRun());

            // Setting the mocked HttpContext for the controller
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            };

            // Act
            var result = await controller.Index(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.IsInstanceOfType(viewResult.Model, typeof(CalculatorRunOverviewViewModel));
        }

        [TestMethod]
        public async Task IndexAsync_EmptyViewModel_RedirectsToStandardError()
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
                       Content = new StringContent(JsonConvert.SerializeObject(new CalculatorRunDto())),
                   });
            _mockHttpClientFactory = TestMockUtils.BuildMockHttpClientFactory(_mockMessageHandler.Object);

            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(r => r.Headers).Returns(new HeaderDictionary
            {
                { "Referer", "https://testhost/previous-page" }
            });

            _mockHttpContext.Setup(ctx => ctx.Request).Returns(mockRequest.Object);

            _controller = new DesignatedRunWithBillingFileController(
                _configuration,
                new Mock<IApiService>().Object,
                _mockTokenAcquisition.Object,
                _telemetryClient,
                new Mock<ICalculatorRunDetailsService>().Object);

            // Setting the mocked HttpContext for the controller
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            };

            // Mocking HttpContext.User.Identity.Name to simulate a logged-in user
            _mockHttpContext.Setup(ctx => ctx.User.Identity.Name).Returns("TestUser");

            var result = await _controller.Index(1) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual(CommonUtil.GetControllerName(typeof(StandardErrorController)), result.ControllerName);
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

        [TestMethod]
        public async Task Should_SetBackLink()
        {
            // Arrange
            var controller = BuildTestClass(HttpStatusCode.OK, MockData.GetCalculatorRun());

            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
            var headers = new HeaderDictionary
            {
                { "Referer", "https://calculator/" }
            };
            _mockHttpContext.Setup(c => c.Request.Headers).Returns(headers);

            _mockCalculatorRunDetailsService.Setup(s => s.GetCalculatorRundetailsAsync(It.IsAny<HttpContext>(), It.IsAny<int>()))
                .ReturnsAsync(new CalculatorRunDetailsViewModel() { RunId = 1 });

            // Setting the mocked HttpContext for the controller
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            };

            // Act
            var result = await _controller.Index(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.IsInstanceOfType(viewResult.Model, typeof(CalculatorRunOverviewViewModel));
        }

        [TestMethod]
        public async Task GenerateBillingFile_Returns_Success()
        {
            // Arrange
            int testRunId = 1;

            var controller = BuildTestClass(HttpStatusCode.OK, MockData.GetCalculatorRun());

            // Setting the mocked HttpContext for the controller
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            };

            // Act
            var result = await controller.GenerateDraftBillingFile(testRunId) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.RouteValues["action"]);
            Assert.AreEqual(ControllerNames.CalculationRunOverview, result.RouteValues["controller"]);
            Assert.AreEqual(testRunId, result.RouteValues["runId"]);
        }

        [TestMethod]
        public async Task GenerateBillingFile_Throws_WhenGenerationFails()
        {
            // Arrange
            int testRunId = 1;

            var controller = BuildTestClass(HttpStatusCode.InternalServerError, MockData.GetCalculatorRun());

            // Setting the mocked HttpContext for the controller
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await controller.GenerateDraftBillingFile(testRunId);
            });
        }

        private DesignatedRunWithBillingFileController BuildTestClass(
           HttpStatusCode httpStatusCode,
           CalculatorRunDto data = null,
           CalculatorRunDetailsViewModel details = null)
        {
            data = data ?? MockData.GetCalculatorRun();
            details = details ?? Fixture.Create<CalculatorRunDetailsViewModel>();
            var mockApiService = TestMockUtils.BuildMockApiService(
                httpStatusCode,
                System.Text.Json.JsonSerializer.Serialize(data ?? MockData.GetCalculatorRun())).Object;

            var testClass = new DesignatedRunWithBillingFileController(
                ConfigurationItems.GetConfigurationValues(),
                mockApiService,
                _mockTokenAcquisition.Object,
                new TelemetryClient(),
                TestMockUtils.BuildMockCalculatorRunDetailsService(details).Object);
            testClass.ControllerContext.HttpContext = new DefaultHttpContext();

            return testClass;
        }
    }
}
