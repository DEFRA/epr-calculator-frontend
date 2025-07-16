using System.Security.Claims;
using System.Security.Principal;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Enums;
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
using Moq.Protected;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.UnitTests.Controllers
{
    [TestClass]
    public class AcceptRejectConfirmationControllerTests
    {
        private readonly IConfiguration _configuration = ConfigurationItems.GetConfigurationValues();
        private Mock<ITokenAcquisition> _mockTokenAcquisition;
        private Mock<IBillingInstructionsApiService> _billingInstructionsApiService;
        private TelemetryClient _telemetryClient;

        public AcceptRejectConfirmationControllerTests()
        {
            _mockTokenAcquisition = new Mock<ITokenAcquisition>();
            _billingInstructionsApiService = new Mock<IBillingInstructionsApiService>();
            _telemetryClient = new TelemetryClient();
        }

        [TestMethod]
        public async Task IndexAsync_InvalidCalculationRunId_RedirectsToStandardError()
        {
            // Arrange
            int invalidRunId = 0;
            var controller = CreateController(new Mock<IHttpClientFactory>().Object);

            // Act
            var result = await controller.IndexAsync(invalidRunId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(ActionNames.StandardErrorIndex, redirectResult.ActionName);
        }

        [TestMethod]
        public async Task IndexAsync_NullRunDetails_RedirectsToStandardError()
        {
            // Arrange
            int runId = 123;

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

            var controller = CreateController(mockHttpClientFactory.Object);

            // Act
            var result = await controller.IndexAsync(runId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(ActionNames.StandardErrorIndex, redirectResult.ActionName);
        }

        [TestMethod]
        public async Task IndexAsync_ValidRunDetails_ReturnsViewWithModel()
        {
            // Arrange
            int runId = 99;
            var runDetails = new CalculatorRunDetailsViewModel { RunName = "TestRun" };

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

            var controller = CreateController(mockHttpClientFactory.Object);

            // Act
            var result = await controller.IndexAsync(runId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.IsInstanceOfType(viewResult.Model, typeof(AcceptRejectConfirmationViewModel));
            var model = viewResult.Model as AcceptRejectConfirmationViewModel;
            Assert.AreEqual(runId, model.CalculationRunId);
            Assert.AreEqual("TestRun", model.CalculationRunName);
            Assert.AreEqual(BillingStatus.Accepted, model.Status);
        }

        [TestMethod]
        public async Task Submit_InvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            var controller = CreateController(new Mock<IHttpClientFactory>().Object);
            controller.ModelState.AddModelError("CalculationRunId", "Required");
            var model = new AcceptRejectConfirmationViewModel
            {
                CalculationRunId = 1,
                CalculationRunName = "Test",
                Status = BillingStatus.Accepted,
                ApproveData = true
            };

            // Act
            var result = await controller.Submit(model);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(ViewNames.AcceptRejectConfirmationIndex, viewResult.ViewName);
            Assert.AreEqual(model, viewResult.Model);
        }

        [TestMethod]
        public async Task Submit_ApproveDataFalse_RedirectsToBillingInstructions()
        {
            // Arrange
            var controller = CreateController(new Mock<IHttpClientFactory>().Object);
            var model = new AcceptRejectConfirmationViewModel
            {
                CalculationRunId = 1,
                CalculationRunName = "Test",
                Status = BillingStatus.Accepted,
                ApproveData = false
            };

            // Act
            var result = await controller.Submit(model);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(ActionNames.Index, redirectResult.ActionName);
            Assert.AreEqual(ControllerNames.BillingInstructionsController, redirectResult.ControllerName);
            Assert.AreEqual(model.CalculationRunId, redirectResult.RouteValues["calculationRunId"]);
        }

        [TestMethod]
        public async Task Submit_ApiReturnsFalse_RedirectsToStandardError()
        {
            // Arrange
            _billingInstructionsApiService.Setup(x => x.PutAcceptRejectBillingInstructions(It.IsAny<int>(), It.IsAny<ProducerBillingInstructionsHttpPutRequestDto>()))
                .ReturnsAsync(false);

            var controller = CreateController(new Mock<IHttpClientFactory>().Object);
            var model = new AcceptRejectConfirmationViewModel
            {
                CalculationRunId = 1,
                CalculationRunName = "Test",
                Status = BillingStatus.Accepted,
                ApproveData = true
            };

            // Act
            var result = await controller.Submit(model);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(ActionNames.StandardErrorIndex, redirectResult.ActionName);
        }

        [TestMethod]
        public async Task Submit_ApiThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            _billingInstructionsApiService.Setup(x => x.PutAcceptRejectBillingInstructions(It.IsAny<int>(), It.IsAny<ProducerBillingInstructionsHttpPutRequestDto>()))
                .ThrowsAsync(new Exception("API error"));

            var controller = CreateController(new Mock<IHttpClientFactory>().Object);
            var model = new AcceptRejectConfirmationViewModel
            {
                CalculationRunId = 1,
                CalculationRunName = "Test",
                Status = BillingStatus.Accepted,
                ApproveData = true
            };

            // Act
            var result = await controller.Submit(model);

            // Assert
            var statusResult = result as ObjectResult;
            Assert.IsNotNull(statusResult);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
            Assert.AreEqual("An error occurred while processing your request.", statusResult.Value);
        }

        [TestMethod]
        public async Task Submit_Success_RedirectsToBillingInstructions()
        {
            // Arrange
            _billingInstructionsApiService.Setup(x => x.PutAcceptRejectBillingInstructions(It.IsAny<int>(), It.IsAny<ProducerBillingInstructionsHttpPutRequestDto>()))
                .ReturnsAsync(true);

            var controller = CreateController(new Mock<IHttpClientFactory>().Object);
            var model = new AcceptRejectConfirmationViewModel
            {
                CalculationRunId = 1,
                CalculationRunName = "Test",
                Status = BillingStatus.Accepted,
                ApproveData = true
            };

            // Act
            var result = await controller.Submit(model);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(ActionNames.Index, redirectResult.ActionName);
            Assert.AreEqual(ControllerNames.BillingInstructionsController, redirectResult.ControllerName);
            Assert.AreEqual(model.CalculationRunId, redirectResult.RouteValues["calculationRunId"]);
        }

        private AcceptRejectConfirmationController CreateController(IHttpClientFactory httpClientFactory)
        {
            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);

            var mockHttpSession = new MockHttpSession();
            mockHttpSession.SetString("accessToken", "dummy-token");

            var context = new DefaultHttpContext()
            {
                User = principal,
                Session = mockHttpSession
            };

            var controller = new AcceptRejectConfirmationController(
                _configuration,
                _mockTokenAcquisition.Object,
                _telemetryClient,
                httpClientFactory,
                _billingInstructionsApiService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = context
                }
            };

            return controller;
        }
    }
}