using System.Net;
using System.Security.Claims;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.UnitTests.Controllers
{
    [TestClass]
    public class SendBillingFileControllerTests
    {
        private readonly IConfiguration _configuration = ConfigurationItems.GetConfigurationValues();
        private Mock<ITokenAcquisition> _mockTokenAcquisition;
        private TelemetryClient _telemetryClient;
        private SendBillingFileController _controller;
        private Mock<HttpContext> _mockHttpContext;

        public SendBillingFileControllerTests()
        {
            _mockTokenAcquisition = new Mock<ITokenAcquisition>();
            _telemetryClient = new TelemetryClient();

            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.Name, "TestUser"));
            var user = new ClaimsPrincipal(identity);
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContext.Setup(ctx => ctx.User).Returns(user);

            _controller = CreateController();
        }

        [TestMethod]
        public async Task Index_WithValidRunDetails_ReturnsViewWithViewModel()
        {
            // Arrange
            int runId = 123;
            var runDetails = new CalculatorRunDetailsViewModel
            {
                RunId = runId,
                RunName = "Test Run"
            };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                   .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = new StringContent(JsonConvert.SerializeObject(runDetails)),
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var controller = CreateController(httpClientFactory: mockHttpClientFactory.Object);

            // Act
            var result = await controller.Index(runId);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            var model = viewResult.Model as SendBillingFileViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(runId, model.RunId);
            Assert.AreEqual("Test Run", model.CalcRunName);
        }

        [TestMethod]
        public async Task Index_WithNullRunDetails_RedirectsToStandardError()
        {
            // Arrange
            int runId = 123;
            var runDetails = new CalculatorRunDetailsViewModel
            {
                RunId = runId,
                RunName = "Test Run"
            };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                   .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = new StringContent(JsonConvert.SerializeObject(null)),
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var controller = CreateController(httpClientFactory: mockHttpClientFactory.Object);

            // Act
            var result = await controller.Index(runId);

            // Assert
            var redirect = result as RedirectToActionResult;
            Assert.IsNotNull(redirect);
            Assert.AreEqual(ActionNames.StandardErrorIndex, redirect.ActionName);
            Assert.AreEqual(CommonUtil.GetControllerName(typeof(StandardErrorController)), redirect.ControllerName);
        }

        [TestMethod]
        public async Task Submit_ModelStateInvalid_RedirectsToIndex()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Invalid");

            // Act
            var result = await _controller.Submit(1);

            // Assert
            var redirect = result as RedirectToActionResult;
            Assert.IsNotNull(redirect);
            Assert.AreEqual(ActionNames.Index, redirect.ActionName);
            Assert.AreEqual(1, redirect.RouteValues["runId"]);
        }

        [TestMethod]
        public async Task Submit_ApiAccepted_RedirectsToBillingFileSuccess()
        {
            // Arrange
            var acceptedCode = HttpStatusCode.Accepted;

            var mockHttpClientFactory = GetMockHttpClientFactoryWithResponse(acceptedCode);

            var controller = new SendBillingFileController(
                   _configuration,
                   _mockTokenAcquisition.Object,
                   _telemetryClient,
                   mockHttpClientFactory.Object)
            {
                // Setting the mocked HttpContext for the controller
                ControllerContext = new ControllerContext
                {
                    HttpContext = _mockHttpContext.Object
                }
            };

            // Act
            var result = await controller.Submit(1);

            // Assert
            var redirect = result as RedirectToActionResult;
            Assert.IsNotNull(redirect);
            Assert.AreEqual(ActionNames.BillingFileSuccess, redirect.ActionName);
            Assert.AreEqual(CommonUtil.GetControllerName(typeof(BillingInstructionsController)), redirect.ControllerName);
        }

        [TestMethod]
        public async Task Submit_ApiReturnsUnprocessableContent_RedirectsToStandardErrorIndex()
        {
            // Arrange
            var unprocessableCode = HttpStatusCode.UnprocessableContent;

            var mockHttpClientFactory = GetMockHttpClientFactoryWithResponse(unprocessableCode);

            var controller = new SendBillingFileController(
                   _configuration,
                   _mockTokenAcquisition.Object,
                   _telemetryClient,
                   mockHttpClientFactory.Object)
            {
                // Setting the mocked HttpContext for the controller
                ControllerContext = new ControllerContext
                {
                    HttpContext = _mockHttpContext.Object
                }
            };

            // Act
            var result = await controller.Submit(1);

            // Assert
            var redirect = result as RedirectToActionResult;
            Assert.IsNotNull(redirect);
            Assert.AreEqual(ActionNames.StandardErrorIndex, redirect.ActionName);
            Assert.AreEqual(CommonUtil.GetControllerName(typeof(StandardErrorController)), redirect.ControllerName);
        }

        [TestMethod]
        public async Task Submit_ApiException_RedirectsToStandardErrorIndex()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Simulated exception"));

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var controller = new SendBillingFileController(
                _configuration,
                _mockTokenAcquisition.Object,
                _telemetryClient,
                mockHttpClientFactory.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _mockHttpContext.Object
                }
            };

            // Act
            var result = await controller.Submit(1);

            // Assert
            var redirect = result as RedirectToActionResult;
            Assert.IsNotNull(redirect);
            Assert.AreEqual(ActionNames.StandardErrorIndex, redirect.ActionName);
            Assert.AreEqual(CommonUtil.GetControllerName(typeof(StandardErrorController)), redirect.ControllerName);
        }

        private static Mock<IHttpClientFactory> GetMockHttpClientFactoryWithResponse(HttpStatusCode code)
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
                    StatusCode = code
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            return mockHttpClientFactory;
        }

        private SendBillingFileController CreateController(
            IConfiguration configuration = null,
            ITokenAcquisition tokenAcquisition = null,
            TelemetryClient telemetryClient = null,
            IHttpClientFactory httpClientFactory = null,
            HttpContext httpContext = null)
        {
            var controller = new SendBillingFileController(
                configuration ?? _configuration,
                tokenAcquisition ?? _mockTokenAcquisition.Object,
                telemetryClient ?? _telemetryClient,
                httpClientFactory ?? new Mock<IHttpClientFactory>().Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext ?? _mockHttpContext.Object
                }
            };

            return controller;
        }
    }
}