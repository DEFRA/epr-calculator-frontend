using System.Net;
using System.Security.Claims;
using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
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

namespace EPR.Calculator.Frontend.UnitTests.Controllers
{
    [TestClass]
    public class ClassifyingCalculationRunControllerTests
    {
        private readonly IConfiguration _configuration = ConfigurationItems.GetConfigurationValues();
        private Mock<ILogger<SetRunClassificationController>> _mockLogger;
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private Mock<ITokenAcquisition> _mockTokenAcquisition;
        private TelemetryClient _mockTelemetryClient;
        private SetRunClassificationController _controller;
        private Mock<HttpContext> _mockHttpContext;

        public ClassifyingCalculationRunControllerTests()
        {
            this.Fixture = new Fixture();
            this.MockMessageHandler = TestMockUtils.BuildMockMessageHandler();
            SetMessageHandlerResponses(true, HttpStatusCode.OK);
            _mockHttpClientFactory = TestMockUtils.BuildMockHttpClientFactory(this.MockMessageHandler.Object);
            _mockLogger = new Mock<ILogger<SetRunClassificationController>>();
            _mockTokenAcquisition = new Mock<ITokenAcquisition>();
            _mockTelemetryClient = new TelemetryClient();

            _mockTokenAcquisition
                .Setup(x => x.GetAccessTokenForUserAsync(It.IsAny<IEnumerable<string>>(), null, null, null, null))
                .ReturnsAsync("somevalue");

            _controller = new SetRunClassificationController(
                       _configuration,
                       new Mock<IApiService>().Object,
                       _mockLogger.Object,
                       _mockTokenAcquisition.Object,
                       _mockTelemetryClient,
                       new Mock<ICalculatorRunDetailsService>().Object);

            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContext.Setup(context => context.User)
               .Returns(new ClaimsPrincipal(new ClaimsIdentity(
           [
               new Claim(ClaimTypes.Name, "Test User")
           ])));

            var mockSession = new MockHttpSession();
            mockSession.SetString("accessToken", "something");
            mockSession.SetString(SessionConstants.FinancialYear, "2024-25");
            var context = new DefaultHttpContext()
            {
                Session = mockSession
            };

            // Setting the mocked HttpContext for the controller
            _controller.ControllerContext = new ControllerContext { HttpContext = context };

            _controller = BuildTestClass(
                this.Fixture,
                HttpStatusCode.OK,
                MockData.GetCalculatorRun(),
                Fixture.Create<CalculatorRunDetailsViewModel>(),
                _configuration);
        }

        private Fixture Fixture { get; init; }

        private Mock<HttpMessageHandler> MockMessageHandler { get; set; }

        [TestMethod]
        public async Task Index_ReturnsViewResult_WithValidViewModel()
        {
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
            // Arrange
            SetMessageHandlerResponses(false, HttpStatusCode.OK);

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
            // Arrange
            int runId = 1;
            SetRunClassificationViewModel model = new SetRunClassificationViewModel
            {
                CalculatorRunDetails = new CalculatorRunDetailsViewModel
                {
                    RunId = runId,
                    RunName = "Test Run"
                },
                ClassifyRunType = (int)RunClassification.INITIAL_RUN
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
            SetMessageHandlerResponses(false, HttpStatusCode.Created);
            int runId = 1;
            SetRunClassificationViewModel model = new SetRunClassificationViewModel
            {
                CalculatorRunDetails = new CalculatorRunDetailsViewModel
                {
                    RunId = runId,
                    RunName = "Test Run"
                },
                ClassifyRunType = (int)RunClassification.INITIAL_RUN
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
                        $"\"RunId\":{runId}")),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task Submit_InvalidModel_ReturnsViewResult_WithErrors()
        {
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
            SetMessageHandlerResponses(false, HttpStatusCode.Created);
            var model = new SetRunClassificationViewModel
            {
                CalculatorRunDetails = new CalculatorRunDetailsViewModel
                {
                    RunId = 1,
                    RunName = "Test Run"
                },
                ClassifyRunType = (int)RunClassification.INITIAL_RUN
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
            _mockHttpClientFactory = TestMockUtils.BuildMockHttpClientFactory(this.MockMessageHandler.Object);

            _controller = new SetRunClassificationController(
                _configuration,
                new Mock<IApiService>().Object,
                _mockLogger.Object,
                _mockTokenAcquisition.Object,
                _mockTelemetryClient,
                new Mock<ICalculatorRunDetailsService>().Object);

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

        [TestMethod]
        public async Task Index_ReturnsStandardError_WhenNoClassifications()
        {
            var responseContent = "{\r\n  \"runId\": 1,\r\n  \"runClassificationId\": 7,\r\n  \"runName\": \"Test Calculator1702\",\r\n  \"runClassificationStatus\": \"UNCLASSIFIED\",\r\n  \"financialYear\": \"2025-26\"\r\n}";
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Content = new StringContent(responseContent)
                    });

            mockHttpMessageHandler
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.Is<HttpRequestMessage>(k => k.RequestUri != null && k.RequestUri.ToString().Contains("Financial")),
                   ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                   {
                       StatusCode = HttpStatusCode.BadRequest,
                       Content = new StringContent("{\r\n\"financialYear\": \"2025-26\",\r\n  \"classifications\": [\r\n    {\r\n      \"id\": 4,\r\n      \"status\": \"TEST RUN\"\r\n    },\r\n    {\r\n      \"id\": 8,\r\n      \"status\": \"INITIAL RUN\"\r\n    }\r\n  ]\r\n}"),
                   });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockTokenAcquisition
                .Setup(x => x.GetAccessTokenForUserAsync(It.IsAny<IEnumerable<string>>(), null, null, null, null))
                .ReturnsAsync("somevalue");

            _mockHttpClientFactory
               .Setup(_ => _.CreateClient(It.IsAny<string>()))
                   .Returns(httpClient);

            _controller = new SetRunClassificationController(
                       _configuration,
                       new Mock<IApiService>().Object,
                       _mockLogger.Object,
                       _mockTokenAcquisition.Object,
                       _mockTelemetryClient,
                       new Mock<ICalculatorRunDetailsService>().Object);

            _mockHttpContext.Setup(context => context.User)
               .Returns(new ClaimsPrincipal(new ClaimsIdentity(
           [
               new Claim(ClaimTypes.Name, "Test User")
           ])));

            var mockSession = new MockHttpSession();
            mockSession.SetString("accessToken", "something");
            mockSession.SetString(SessionConstants.FinancialYear, "2024-25");
            var context = new DefaultHttpContext()
            {
                Session = mockSession
            };

            // Setting the mocked HttpContext for the controller
            _controller.ControllerContext = new ControllerContext { HttpContext = context };

            // Arrange
            int runId = 1;

            // Act
            var result = await _controller.Index(runId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(CommonUtil.GetControllerName(typeof(StandardErrorController)), result.ControllerName);
        }

        [TestMethod]
        public async Task Index_ReturnsStandardError_WhenNoRunDetails()
        {
            var responseContent = "{\r\n  \"runId\": 1,\r\n  \"runClassificationId\": 7,\r\n  \"runName\": \"Test Calculator1702\",\r\n  \"runClassificationStatus\": \"UNCLASSIFIED\",\r\n  \"financialYear\": \"2025-26\"\r\n}";
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Content = new StringContent(responseContent)
                    });

            mockHttpMessageHandler
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.Is<HttpRequestMessage>(k => k.RequestUri != null && k.RequestUri.ToString().Contains("Financial")),
                   ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                   {
                       StatusCode = HttpStatusCode.OK,
                       Content = new StringContent("{\r\n\"financialYear\": \"2025-26\",\r\n  \"classifications\": [\r\n    {\r\n      \"id\": 4,\r\n      \"status\": \"TEST RUN\"\r\n    },\r\n    {\r\n      \"id\": 8,\r\n      \"status\": \"INITIAL RUN\"\r\n    }\r\n  ]\r\n}"),
                   });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockTokenAcquisition
                .Setup(x => x.GetAccessTokenForUserAsync(It.IsAny<IEnumerable<string>>(), null, null, null, null))
                .ReturnsAsync("somevalue");

            _mockHttpClientFactory
               .Setup(_ => _.CreateClient(It.IsAny<string>()))
                   .Returns(httpClient);

            _controller = new SetRunClassificationController(
                       _configuration,
                       new Mock<IApiService>().Object,
                       _mockLogger.Object,
                       _mockTokenAcquisition.Object,
                       _mockTelemetryClient,
                       new Mock<ICalculatorRunDetailsService>().Object);

            var mockSession = new MockHttpSession();
            mockSession.SetString("accessToken", "something");
            mockSession.SetString(SessionConstants.FinancialYear, "2024-25");
            var context = new DefaultHttpContext()
            {
                Session = mockSession
            };

            // Setting the mocked HttpContext for the controller
            _controller.ControllerContext = new ControllerContext { HttpContext = context };

            // Arrange
            int runId = 1;

            // Act
            var result = await _controller.Index(runId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(CommonUtil.GetControllerName(typeof(StandardErrorController)), result.ControllerName);
        }

        private void SetMessageHandlerResponses(bool isUnclassified, HttpStatusCode httpStatusCode)
        {
            var responseContent = isUnclassified ? "{\r\n  \"runId\": 1,\r\n  \"runClassificationId\": 3,\r\n  \"runClassificationStatus\": \"UNCLASSIFIED\",\r\n  \"financialYear\": \"2025-26\"\r\n}"
                : "{\r\n  \"runId\": 1,\r\n  \"runClassificationId\": 7,\r\n  \"runName\": \"Test Calculator1702\",\r\n  \"runClassificationStatus\": \"UNCLASSIFIED\",\r\n  \"financialYear\": \"2025-26\"\r\n}";
            this.MockMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = httpStatusCode,
                        Content = new StringContent(responseContent)
                    });

            this.MockMessageHandler
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.Is<HttpRequestMessage>(k => k.RequestUri != null && k.RequestUri.ToString().Contains("Financial")),
                   ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                   {
                       StatusCode = httpStatusCode,
                       Content = new StringContent("{\r\n\"financialYear\": \"2025-26\",\r\n  \"classifications\": [\r\n    {\r\n      \"id\": 4,\r\n      \"status\": \"TEST RUN\"\r\n    },\r\n    {\r\n      \"id\": 8,\r\n      \"status\": \"INITIAL RUN\"\r\n    }\r\n  ]\r\n}"),
                   });
        }

        private SetRunClassificationController BuildTestClass(
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

            var testClass = new SetRunClassificationController(
                configurationItems,
                mockApiService,
                new Mock<ILogger<SetRunClassificationController>>().Object,
                new Mock<ITokenAcquisition>().Object,
                new TelemetryClient(),
                TestMockUtils.BuildMockCalculatorRunDetailsService(details).Object);
            testClass.ControllerContext.HttpContext = new DefaultHttpContext()
            {
                Session = TestMockUtils.BuildMockSession(fixture).Object,
            };

            return testClass;
        }
    }
}