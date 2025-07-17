using System.Net;
using System.Security.Claims;
using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Models;
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

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class PaymentCalculatorControllerTests
    {
        private readonly IConfiguration _configuration = ConfigurationItems.GetConfigurationValues();
        private Mock<ITokenAcquisition> _mockTokenAcquisition;
        private TelemetryClient _telemetryClient;
        private PaymentCalculatorController _controller;
        private Mock<HttpContext> _mockHttpContext;
        private Mock<IHttpClientFactory> _mockClientFactory;

        public PaymentCalculatorControllerTests()
        {
            this.Fixture = new Fixture();
            _mockClientFactory = new Mock<IHttpClientFactory>();
            _mockTokenAcquisition = new Mock<ITokenAcquisition>();
            _mockTokenAcquisition
                .Setup(x => x.GetAccessTokenForUserAsync(It.IsAny<IEnumerable<string>>(), null, null, null, null))
                .ReturnsAsync("somevalue");
            _telemetryClient = new TelemetryClient();
            _mockHttpContext = new Mock<HttpContext>();
            var mockSession = new MockHttpSession();
            _mockHttpContext.Setup(s => s.Session).Returns(mockSession);
            _mockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
            mockSession.SetString("accessToken", "something");
            _controller = new PaymentCalculatorController(
                   _configuration,
                   _mockTokenAcquisition.Object,
                   _telemetryClient,
                   new Mock<IApiService>().Object,
                   new Mock<ICalculatorRunDetailsService>().Object)
            {
                // Setting the mocked HttpContext for the controller
                ControllerContext = new ControllerContext
                {
                    HttpContext = _mockHttpContext.Object
                }
            };
            _mockHttpContext.Setup(context => context.User)
               .Returns(new ClaimsPrincipal(new ClaimsIdentity(
           [
               new Claim(ClaimTypes.Name, "Test User")
           ])));
        }

        private Fixture Fixture { get; init; }

        [TestMethod]
        public async Task Index_ReturnsViewResult_WithAcceptInvoiceInstructionsViewModel()
        {
            // Arrange
            CalculatorRunDto calculatorRunDto = MockData.GetCalculatorRun();
            var runDetails = Fixture.Create<CalculatorRunDetailsViewModel>();
            var controller = BuildTestClass(
                Fixture,
                HttpStatusCode.OK,
                calculatorRunDto,
                runDetails,
                _configuration);

            // Act
            var result = await controller.Index(calculatorRunDto.RunId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(AcceptInvoiceInstructionsViewModel));
            var model = result.Model as AcceptInvoiceInstructionsViewModel;
            Assert.AreEqual(calculatorRunDto.RunId, model.RunId);
            Assert.IsFalse(model.AcceptAll);
            Assert.AreEqual(runDetails.RunName, model.CalculationRunTitle);
            Assert.AreEqual(ControllerNames.ClassifyRunConfirmation, model.BackLink);
        }

        [TestMethod]
        public async Task Submit_InvalidModelState_RedirectsToIndex()
        {
            // Arrange
            var model = new AcceptInvoiceInstructionsViewModel { RunId = 1 };
            _controller.ModelState.AddModelError("AcceptAll", "Required");

            // Act
            var result = await _controller.Submit(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.PaymentCalculatorIndex, result.ViewName);
        }

        [TestMethod]
        public async Task Submit_ApiResponseAccepted_RedirectsToOverview()
        {
            // Arrange
            var model = new AcceptInvoiceInstructionsViewModel { RunId = 123 };
            var controller = BuildTestClass(
                Fixture,
                HttpStatusCode.Accepted,
                model);

            // Act
            var result = await controller.Submit(model) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(ControllerNames.CalculationRunOverview, result.ControllerName);
            Assert.AreEqual(model.RunId, result.RouteValues["RunId"]);
        }

        [TestMethod]
        public async Task Submit_RedirectsToError_WhenTryAcceptBillingInstructionsFails()
        {
            // Arrange
            var model = new AcceptInvoiceInstructionsViewModel { RunId = 123, AcceptAll = true };

            // Mock: GenerateBillingFile succeeds, AcceptBillingInstructions fails
            var handlerAccepted = TestMockUtils.BuildMockMessageHandler(HttpStatusCode.Accepted, model);
            var handlerFailed = TestMockUtils.BuildMockMessageHandler(HttpStatusCode.BadRequest, model);

            var httpClientAccepted = new HttpClient(handlerAccepted.Object);
            var httpClientFailed = new HttpClient(handlerFailed.Object);

            // First call: GenerateBillingFile (POST) -> Accepted
            // Second call: AcceptBillingInstructions (PUT) -> BadRequest
            _mockClientFactory.SetupSequence(x => x.CreateClient(It.IsAny<string>()))
                .Returns(httpClientAccepted) // For TryGenerateBillingFile
                .Returns(httpClientFailed);  // For TryAcceptBillingInstructions

            _controller = new PaymentCalculatorController(
                _configuration,
                _mockTokenAcquisition.Object,
                _telemetryClient,
                new Mock<IApiService>().Object,
                new Mock<ICalculatorRunDetailsService>().Object)
            {
                ControllerContext = new ControllerContext { HttpContext = _mockHttpContext.Object }
            };

            // Act
            var result = await _controller.Submit(model) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
        }

        [TestMethod]
        public async Task Submit_RedirectsToError_WhenTryGenerateBillingFileFails()
        {
            // Arrange
            var model = new AcceptInvoiceInstructionsViewModel { RunId = 123, AcceptAll = true };

            // Mock: GenerateBillingFile fails
            var handlerFailed = TestMockUtils.BuildMockMessageHandler(HttpStatusCode.BadRequest, model);
            var httpClientFailed = new HttpClient(handlerFailed.Object);

            _mockClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(httpClientFailed);

            _controller = new PaymentCalculatorController(
                _configuration,
                _mockTokenAcquisition.Object,
                _telemetryClient,
                new Mock<IApiService>().Object,
                new Mock<ICalculatorRunDetailsService>().Object)
            {
                ControllerContext = new ControllerContext { HttpContext = _mockHttpContext.Object }
            };

            // Act
            var result = await _controller.Submit(model) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
        }

        [TestMethod]
        public async Task Submit_RedirectsToOverview_WhenAllApiCallsSucceed()
        {
            // Arrange
            var model = new AcceptInvoiceInstructionsViewModel { RunId = 123, AcceptAll = true };

            var controller = BuildTestClass(
                Fixture,
                HttpStatusCode.Accepted,
                model);

            // Act
            var result = await controller.Submit(model) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(ControllerNames.CalculationRunOverview, result.ControllerName);
            Assert.AreEqual(model.RunId, result.RouteValues["RunId"]);
        }

        [TestMethod]
        public async Task TryAcceptBillingInstructions_ReturnsTrue_OnSuccess()
        {
            // Arrange
            var runId = 123;

            var controller = BuildTestClass(
                Fixture,
                HttpStatusCode.Accepted);

            // Use reflection to get the private method for invocation.
            var method = typeof(PaymentCalculatorController)
                .GetMethod("TryAcceptBillingInstructions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var task = (Task<bool>)method.Invoke(controller, new object[] { runId });
            var result = await task;

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task Submit_ApiReturnsError_RedirectsToError()
        {
            // Arrange
            var model = new AcceptInvoiceInstructionsViewModel { RunId = 123 };
            var mockHttpMessageHandler = TestMockUtils.BuildMockMessageHandler(HttpStatusCode.UnprocessableContent, model);

            var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);

            _mockClientFactory.SetupSequence(x => x.CreateClient(It.IsAny<string>()))
                            .Returns(mockHttpClient);
            // Act
            var result = await _controller.Submit(model) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
        }

        [TestMethod]
        public async Task Submit_ApiThrowsException_RedirectsToError()
        {
            // Arrange
            var model = new AcceptInvoiceInstructionsViewModel { RunId = 123 };
            var mockHttpMessageHandler = TestMockUtils.BuildMockMessageHandler(shouldThrowException: true);

            var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);

            _mockClientFactory.SetupSequence(x => x.CreateClient(It.IsAny<string>()))
                            .Returns(mockHttpClient);
            // Act
            var result = await _controller.Submit(model) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
        }

        private PaymentCalculatorController BuildTestClass(
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

            var testClass = new PaymentCalculatorController(
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
