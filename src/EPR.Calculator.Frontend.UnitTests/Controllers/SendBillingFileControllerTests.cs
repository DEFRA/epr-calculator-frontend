using System.Net;
using System.Security.Claims;
using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
using EPR.Calculator.Frontend.UnitTests.Mocks;
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
            this.Fixture = new Fixture();
            _mockTokenAcquisition = new Mock<ITokenAcquisition>();
            _telemetryClient = new TelemetryClient();

            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.Name, "TestUser"));
            var user = new ClaimsPrincipal(identity);
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContext.Setup(ctx => ctx.User).Returns(user);

            _controller = CreateController();
        }

        private Fixture Fixture { get; init; }

        [TestMethod]
        public async Task Index_WithValidRunDetails_ReturnsViewWithViewModel()
        {
            // Arrange
            int runId = 123;
            var runDetails = new CalculatorRunDetailsViewModel
            {
                RunId = runId,
                RunName = Fixture.Create<string>(),
            };

            var controller = BuildTestClass(
                Fixture,
                HttpStatusCode.OK,
                runDetails,
                runDetails,
                configurationItems: _configuration);

            // Act
            var result = await controller.Index(runId);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            var model = viewResult.Model as SendBillingFileViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(runId, model.RunId);
            Assert.AreEqual(runDetails.RunName, model.CalcRunName);
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
        public async Task Index_WithBillingFileNotGeneratedLatest_RedirectsToIndex()
        {
            // Arrange
            int runId = 123;
            var runDetails = new CalculatorRunDetailsViewModel
            {
                RunId = runId,
                RunName = Fixture.Create<string>(),
                IsBillingFileGeneratedLatest = false
            };

            var controller = BuildTestClass(
                Fixture,
                HttpStatusCode.OK,
                runDetails,
                runDetails,
                configurationItems: _configuration);

            // Act
            var result = await controller.Index(runId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(ControllerNames.CalculationRunOverview, result.ControllerName);
        }

        [TestMethod]
        public async Task Submit_ModelStateInvalid_RedirectsToIndex()
        {
            // Arrange
            SendBillingFileViewModel model = new SendBillingFileViewModel
            {
                RunId = 1,
                ConfirmSend = false, // Simulating invalid state
                CalcRunName = "Test Run",
            };

            _controller.ModelState.AddModelError("Error", "Invalid");

            // Act
            var result = await _controller.Submit(model);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(ViewNames.SendBillingFileIndex, viewResult.ViewName);
            Assert.AreEqual(model, viewResult.Model);
        }

        [TestMethod]
        public async Task Submit_ApiAccepted_RedirectsToBillingFileSuccess()
        {
            // Arrange
            Fixture.Customize<SendBillingFileViewModel>(c => c.With(c => c.ConfirmSend, true));
            SendBillingFileViewModel model = Fixture.Create<SendBillingFileViewModel>();

            var controller = BuildTestClass(
                Fixture,
                HttpStatusCode.Accepted);

            // Act
            var result = await controller.Submit(model);

            // Assert
            var redirect = result as RedirectToActionResult;
            Assert.IsNotNull(redirect);
            Assert.AreEqual(ActionNames.BillingFileSuccess, redirect.ActionName);
            Assert.AreEqual(CommonUtil.GetControllerName(typeof(BillingInstructionsController)), redirect.ControllerName);
        }

        [TestMethod]
        public async Task Submit_ApiUnprocessableEntity_BillingFileOutdated()
        {
            // Arrange
            Fixture.Customize<SendBillingFileViewModel>(c => c.With(c => c.ConfirmSend, true));
            SendBillingFileViewModel model = Fixture.Create<SendBillingFileViewModel>();

            var controller = BuildTestClass(
                Fixture,
                HttpStatusCode.UnprocessableEntity);

            // Act
            var result = await controller.Submit(model);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(ActionNames.Index, viewResult.ViewName);
            Assert.AreEqual(model, viewResult.Model);
        }

        [TestMethod]
        public async Task Submit_ApiResponse_InternalServerError()
        {
            // Arrange
            Fixture.Customize<SendBillingFileViewModel>(c => c.With(c => c.ConfirmSend, true));
            SendBillingFileViewModel model = Fixture.Create<SendBillingFileViewModel>();

            var controller = BuildTestClass(
                Fixture,
                HttpStatusCode.InternalServerError);

            // Act
            var result = await controller.Submit(model);

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
                new Mock<IApiService>().Object,
                new Mock<ICalculatorRunDetailsService>().Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext ?? _mockHttpContext.Object
                }
            };

            return controller;
        }

        private SendBillingFileController BuildTestClass(
                Fixture fixture,
                HttpStatusCode httpStatusCode,
                object data = null,
                CalculatorRunDetailsViewModel details = null,
                IConfiguration configurationItems = null)
        {
            data = data ?? MockData.GetCalculatorRun();
            configurationItems = configurationItems ?? ConfigurationItems.GetConfigurationValues();
            details = details ?? Fixture.Create<CalculatorRunDetailsViewModel>();
            var mockApiService = TestMockUtils.BuildMockApiService(
                httpStatusCode,
                System.Text.Json.JsonSerializer.Serialize(data ?? MockData.GetCalculatorRun())).Object;

            var testClass = new SendBillingFileController(
                configurationItems,
                new Mock<ITokenAcquisition>().Object,
                new TelemetryClient(),
                mockApiService,
                TestMockUtils.BuildMockCalculatorRunDetailsService(details).Object);
            testClass.ControllerContext.HttpContext = new DefaultHttpContext()
            {
                Session = TestMockUtils.BuildMockSession(fixture).Object,
            };

            return testClass;
        }
    }
}