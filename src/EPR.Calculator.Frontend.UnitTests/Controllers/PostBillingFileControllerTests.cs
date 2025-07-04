namespace EPR.Calculator.Frontend.UnitTests.Controllers
{
    using System.Net;
    using System.Security.Claims;
    using AutoFixture;
    using EPR.Calculator.Frontend.Constants;
    using EPR.Calculator.Frontend.Controllers;
    using EPR.Calculator.Frontend.Helpers;
    using EPR.Calculator.Frontend.Services;
    using EPR.Calculator.Frontend.UnitTests.HelpersTest;
    using EPR.Calculator.Frontend.UnitTests.Mocks;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Identity.Web;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Moq.Protected;
    using Newtonsoft.Json;

    [TestClass]
    public class PostBillingFileControllerTests
    {
        private readonly IConfiguration _configuration = ConfigurationItems.GetConfigurationValues();
        private Mock<IHttpClientFactory> _mockClientFactory;
        private Mock<ILogger<PostBillingFileController>> _mockLogger;
        private Mock<ITokenAcquisition> _mockTokenAcquisition;
        private TelemetryClient _mockTelemetryClient;
        private PostBillingFileController _controller;
        private Mock<HttpMessageHandler> _mockMessageHandler;
        private Mock<HttpContext> _mockHttpContext;

        private Fixture Fixture { get; } = new Fixture();

        [TestInitialize]
        public void Setup()
        {
            _mockHttpContext = new Mock<HttpContext>();
            _mockClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<PostBillingFileController>>();
            _mockTokenAcquisition = new Mock<ITokenAcquisition>();
            _mockTelemetryClient = new TelemetryClient();
            _mockMessageHandler = new Mock<HttpMessageHandler>();

            var mockSession = new MockHttpSession();
            _mockHttpContext.Setup(s => s.Session).Returns(mockSession);
            _mockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
            _mockTokenAcquisition
                .Setup(x => x.GetAccessTokenForUserAsync(It.IsAny<IEnumerable<string>>(), null, null, null, null))
                .ReturnsAsync("somevalue");
            mockSession.SetString("accessToken", "something");

            _controller = new PostBillingFileController(
                       _configuration,
                       _mockTokenAcquisition.Object,
                       _mockTelemetryClient,
                       new Mock<IApiService>().Object,
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

        [TestMethod]
        public async Task Index_ValidRunId_ReturnsViewResult()
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

            _controller = new PostBillingFileController(
                       _configuration,
                       _mockTokenAcquisition.Object,
                       _mockTelemetryClient,
                       new Mock<IApiService>().Object,
                       new Mock<ICalculatorRunDetailsService>().Object);

            // Setting the mocked HttpContext for the controller
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            };

            // Act
            var result = await _controller.Index(10) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.PostBillingFileIndex, result.ViewName);
        }

        [TestMethod]
        public async Task Index_InvalidModelState_ReturnsRedirectToAction()
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
                       Content = new StringContent(string.Empty),
                   });
            _mockClientFactory = TestMockUtils.BuildMockHttpClientFactory(_mockMessageHandler.Object);

            _controller = new PostBillingFileController(
                       _configuration,
                       _mockTokenAcquisition.Object,
                       _mockTelemetryClient,
                       new Mock<IApiService>().Object,
                       new Mock<ICalculatorRunDetailsService>().Object);

            // Setting the mocked HttpContext for the controller
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            };

            // Arrange
            var runId = 10;

            // Act
            var result = await _controller.Index(runId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual(CommonUtil.GetControllerName(typeof(StandardErrorController)), result.ControllerName);
        }
    }
}