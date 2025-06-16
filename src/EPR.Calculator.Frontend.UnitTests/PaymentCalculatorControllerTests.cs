using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Net;
using System.Security.Claims;
using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using EPR.Calculator.Frontend.Validators;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

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
                   _mockClientFactory.Object)
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

            var mockHttpMessageHandler = TestMockUtils.BuildMockMessageHandler(HttpStatusCode.OK, calculatorRunDto);
            _mockClientFactory = TestMockUtils.BuildMockHttpClientFactory(mockHttpMessageHandler.Object);

            _controller = new PaymentCalculatorController(
                _configuration,
                _mockTokenAcquisition.Object,
                _telemetryClient,
                _mockClientFactory.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            };
            // Act
            var result = await _controller.Index(calculatorRunDto.RunId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(AcceptInvoiceInstructionsViewModel));
            var model = result.Model as AcceptInvoiceInstructionsViewModel;
            Assert.AreEqual(calculatorRunDto.RunId, model.RunId);
            Assert.IsFalse(model.AcceptAll);
            Assert.AreEqual("Test Run", model.CalculationRunTitle);
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
            var mockHttpMessageHandler = TestMockUtils.BuildMockMessageHandler(HttpStatusCode.Accepted, model);
            _mockClientFactory = TestMockUtils.BuildMockHttpClientFactory(mockHttpMessageHandler.Object);

            _controller = new PaymentCalculatorController(
                _configuration,
                _mockTokenAcquisition.Object,
                _telemetryClient,
                _mockClientFactory.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            };

            // Act
            var result = await _controller.Submit(model) as RedirectToActionResult;

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
                _mockClientFactory.Object)
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
                _mockClientFactory.Object)
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

            // Both API calls succeed
            var handlerAccepted = TestMockUtils.BuildMockMessageHandler(HttpStatusCode.Accepted, model);
            var handlerSuccess = TestMockUtils.BuildMockMessageHandler(HttpStatusCode.OK, model);

            var httpClientAccepted = new HttpClient(handlerAccepted.Object);
            var httpClientSuccess = new HttpClient(handlerSuccess.Object);

            // First call: GenerateBillingFile (POST) -> Accepted
            // Second call: AcceptBillingInstructions (PUT) -> OK
            _mockClientFactory.SetupSequence(x => x.CreateClient(It.IsAny<string>()))
                .Returns(httpClientAccepted) // For TryGenerateBillingFile
                .Returns(httpClientSuccess); // For TryAcceptBillingInstructions

            _controller = new PaymentCalculatorController(
                _configuration,
                _mockTokenAcquisition.Object,
                _telemetryClient,
                _mockClientFactory.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = _mockHttpContext.Object }
            };

            // Act
            var result = await _controller.Submit(model) as RedirectToActionResult;

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
            var handlerSuccess = TestMockUtils.BuildMockMessageHandler(HttpStatusCode.OK, null);
            var httpClientSuccess = new HttpClient(handlerSuccess.Object);

            _mockClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(httpClientSuccess);

            var controller = new PaymentCalculatorController(
                _configuration,
                _mockTokenAcquisition.Object,
                _telemetryClient,
                _mockClientFactory.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = _mockHttpContext.Object }
            };

            // Use reflection to invoke the private method
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

        [TestMethod]
        public void Index_ReturnsViewResult_WithCorrectViewName()
        {
            // Mocking HttpContext.User.Identity.Name to simulate a logged-in user
            _mockHttpContext.Setup(ctx => ctx.User.Identity.Name).Returns("TestUser");

            // Assert
            var viewResult = _controller.BillingFileSuccess() as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(ViewNames.BillingConfirmationSuccess, viewResult.ViewName);
        }

        [TestMethod]
        public void BillingFileSuccess_ReturnsViewResult_WithCorrectViewModel()
        {
            // Mocking HttpContext.User.Identity.Name to simulate a logged-in user
            _mockHttpContext.Setup(ctx => ctx.User.Identity.Name).Returns("TestUser");

            // Act
            var result = _controller.BillingFileSuccess() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.BillingConfirmationSuccess, result.ViewName);

            var model = result.Model as BillingFileSuccessViewModel;
            Assert.IsNotNull(model);

            var confirmationModel = model.ConfirmationViewModel;
            Assert.IsNotNull(confirmationModel);
            Assert.AreEqual(ConfirmationMessages.BillingFileSuccessTitle, confirmationModel.Title);
            Assert.AreEqual(ConfirmationMessages.BillingFileSuccessBody, confirmationModel.Body);
            CollectionAssert.AreEqual(ConfirmationMessages.BillingFileSuccessAdditionalParagraphs, confirmationModel.AdditionalParagraphs);
            Assert.AreEqual(ControllerNames.Dashboard, confirmationModel.RedirectController);
        }

        private void MockHttpMessageHandler(out AcceptInvoiceInstructionsViewModel model, out Mock<HttpMessageHandler> mockHttpMessageHandler)
        {
            model = new AcceptInvoiceInstructionsViewModel { RunId = 123 };
            var mockSession = new MockHttpSession();
            mockSession.SetString("accessToken", "something");
            var context = new DefaultHttpContext()
            {
                Session = mockSession
            };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        }
    }
}
