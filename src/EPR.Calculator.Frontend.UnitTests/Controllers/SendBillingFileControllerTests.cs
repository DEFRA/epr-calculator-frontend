using System.Net;
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
            _mockHttpContext = new Mock<HttpContext>();

            _controller = new SendBillingFileController(
                   _configuration,
                   _mockTokenAcquisition.Object,
                   _telemetryClient,
                   new Mock<IHttpClientFactory>().Object)
            {
                // Setting the mocked HttpContext for the controller
                ControllerContext = new ControllerContext
                {
                    HttpContext = _mockHttpContext.Object
                }
            };
        }

        [TestMethod]
        public void CanCallIndex()
        {
            int runId = 99;

            // Mocking HttpContext.User.Identity.Name to simulate a logged-in user
            _mockHttpContext.Setup(ctx => ctx.User.Identity.Name).Returns("TestUser");

            var result = _controller.Index(runId);

            // Assert
            Assert.IsNotNull(result);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.IsInstanceOfType(viewResult.Model, typeof(SendBillingFileViewModel));
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
            Assert.AreEqual(CommonUtil.GetControllerName(typeof(PaymentCalculatorController)), redirect.ControllerName);
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
    }
}