using System.Net;
using System.Security.Claims;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Moq;
using Moq.Protected;

namespace EPR.Calculator.Frontend.UnitTests.Controllers
{
    [TestClass]
    public class RemoveClassificationControllerTests
    {
        private Mock<HttpMessageHandler> mockMessageHandler;

        private Mock<IConfiguration> configurationMock;
        private Mock<IApiService> apiServiceMock;
        private Mock<ILogger<RemoveClassificationController>> loggerMock;
        private Mock<ITokenAcquisition> tokenAcquisitionMock;
        private TelemetryClient telemetryClient;
        private Mock<ICalculatorRunDetailsService> runDetailsServiceMock;

        private RemoveClassificationController controller;

        [TestInitialize]
        public void Setup()
            {
                // Setup mocked HttpMessageHandler and responses
                mockMessageHandler = new Mock<HttpMessageHandler>();
                SetupMockResponses(isUnclassified: false, HttpStatusCode.OK);

                // Setup other mocks
                configurationMock = new Mock<IConfiguration>();
                apiServiceMock = new Mock<IApiService>();
                loggerMock = new Mock<ILogger<RemoveClassificationController>>();
                tokenAcquisitionMock = new Mock<ITokenAcquisition>();
                telemetryClient = new TelemetryClient();
                runDetailsServiceMock = new Mock<ICalculatorRunDetailsService>();

                // Setup IApiService GetApiUrl method (return a Uri object)
                apiServiceMock.Setup(s => s.GetApiUrl(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(new Uri("http://test.test"));

            // Create controller instance
                controller = new RemoveClassificationController(
                    configurationMock.Object,
                    apiServiceMock.Object,
                    loggerMock.Object,
                    tokenAcquisitionMock.Object,
                    telemetryClient,
                    runDetailsServiceMock.Object);

                // Setup HttpContext with user and session
                var httpContext = new DefaultHttpContext();
                httpContext.Session = new Mock<ISession>().Object;
                httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.Name, "TestUser")
                }));

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                };
            }

        [TestMethod]
        public async Task Index_WithValidRunId_ReturnsViewWithViewModel()
        {
            int runId = 123;
            var runDetails = new CalculatorRunDetailsViewModel { RunId = runId };

            runDetailsServiceMock
                .Setup(s => s.GetCalculatorRundetailsAsync(It.IsAny<HttpContext>(), runId))
                .ReturnsAsync(runDetails);

            var result = await controller.Index(runId);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult);

            Assert.IsInstanceOfType(viewResult.Model, typeof(SetRunClassificationViewModel));
            var model = (SetRunClassificationViewModel)viewResult.Model;
            Assert.IsNotNull(model);
            Assert.AreEqual(runId, model.CalculatorRunDetails.RunId);
            Assert.IsNull(model.ClassifyRunType);
        }

        [TestMethod]
        public async Task Index_WhenExceptionThrown_RedirectsToError()
        {
            int runId = 123;

            runDetailsServiceMock
                .Setup(s => s.GetCalculatorRundetailsAsync(It.IsAny<HttpContext>(), runId))
                .ThrowsAsync(new Exception("Test Exception"));

            var result = await controller.Index(runId);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("Index", redirectResult.ActionName);
            Assert.AreEqual("StandardError", redirectResult.ControllerName);
        }

        [TestMethod]
        public async Task Submit_ModelStateInvalid_ReturnsViewWithViewModel()
        {
            controller.ModelState.AddModelError("ClassifyRunType", "Required");

            var model = new SetRunClassificationViewModel
            {
                CalculatorRunDetails = new CalculatorRunDetailsViewModel { RunId = 123 },
                CurrentUser = "TestUser"
            };

            runDetailsServiceMock
                .Setup(s => s.GetCalculatorRundetailsAsync(It.IsAny<HttpContext>(), model.CalculatorRunDetails.RunId))
                .ReturnsAsync(new CalculatorRunDetailsViewModel { RunId = model.CalculatorRunDetails.RunId });

            var result = await controller.Submit(model);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.IsInstanceOfType(viewResult.Model, typeof(SetRunClassificationViewModel));
            var vm = (SetRunClassificationViewModel)viewResult.Model;
            Assert.AreEqual(model.CalculatorRunDetails.RunId, vm.CalculatorRunDetails.RunId);
        }

        [TestMethod]
        public async Task Submit_WhenDeleteSelected_RedirectsToDeleteController()
        {
            var runId = 123;

            var model = new SetRunClassificationViewModel
            {
                ClassifyRunType = (int)RunClassification.DELETED,
                CalculatorRunDetails = new CalculatorRunDetailsViewModel { RunId = runId },
                CurrentUser = "TestUser"
            };

            var result = await controller.Submit(model);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("Index", redirectResult.ActionName);
            Assert.AreEqual("CalculationRunDelete", redirectResult.ControllerName);
            Assert.AreEqual(runId, redirectResult.RouteValues["runId"]);
        }

        [TestMethod]
        public async Task Submit_WhenTestRunSelected_AndApiFails_ReturnsErrorRedirect()
        {
            // Arrange
            var model = new SetRunClassificationViewModel
            {
                ClassifyRunType = (int)RunClassification.TEST_RUN,
                CalculatorRunDetails = new CalculatorRunDetailsViewModel { RunId = 123 },
                CurrentUser = "TestUser"
            };

            var failedResponse = new HttpResponseMessage(HttpStatusCode.BadRequest); // Simulates failure
            apiServiceMock
                .Setup(s => s.GetApiUrl(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new Uri("http://test.test"));
            apiServiceMock
                .Setup(s => s.CallApi(
                    It.IsAny<HttpContext>(),
                    HttpMethod.Put,
                    It.IsAny<Uri>(),
                    It.IsAny<string>(),
                    It.IsAny<ClassificationDto>()))
                .ReturnsAsync(failedResponse);

            // Act
            var result = await controller.Submit(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirect = (RedirectToActionResult)result;
            Assert.AreEqual("Index", redirect.ActionName);
            Assert.AreEqual("StandardError", redirect.ControllerName);
        }

        [TestMethod]
        public async Task Submit_WhenUnexpectedClassifyRunType_RedirectsToError()
        {
            var model = new SetRunClassificationViewModel
            {
                ClassifyRunType = 999,
                CalculatorRunDetails = new CalculatorRunDetailsViewModel { RunId = 123 },
                CurrentUser = "TestUser"
            };

            var result = await controller.Submit(model);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("Index", redirectResult.ActionName);
            Assert.AreEqual("StandardError", redirectResult.ControllerName);
        }

        [TestMethod]
        public async Task Submit_WhenTestRunSelected_AndApiReturnsCreated_RedirectsToConfirmation()
        {
            // Arrange
            var runId = 123;

            var model = new SetRunClassificationViewModel
            {
                ClassifyRunType = (int)RunClassification.TEST_RUN,
                CalculatorRunDetails = new CalculatorRunDetailsViewModel { RunId = runId },
                CurrentUser = "TestUser"
            };

            var successfulResponse = new HttpResponseMessage(HttpStatusCode.Created);

            apiServiceMock
                .Setup(s => s.GetApiUrl(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new Uri("http://test.test"));

            apiServiceMock
                .Setup(s => s.CallApi(
                    It.IsAny<HttpContext>(),
                    HttpMethod.Put,
                    It.IsAny<Uri>(),
                    It.IsAny<string>(),
                    It.IsAny<ClassificationDto>()))
                .ReturnsAsync(successfulResponse);

            // Act
            var result = await controller.Submit(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirect = (RedirectToActionResult)result;
            Assert.AreEqual("Index", redirect.ActionName);
            Assert.AreEqual(ControllerNames.ClassifyRunConfirmation, redirect.ControllerName);
            Assert.AreEqual(runId, redirect.RouteValues["runId"]);
        }

        [TestMethod]
        public async Task Submit_WhenExceptionThrown_RedirectsToError()
        {
            var model = new SetRunClassificationViewModel
            {
                ClassifyRunType = (int)RunClassification.TEST_RUN,
                CalculatorRunDetails = new CalculatorRunDetailsViewModel { RunId = 123 },
                CurrentUser = "TestUser"
            };

            apiServiceMock
                .Setup(s => s.GetApiUrl(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("Test Exception"));

            var result = await controller.Submit(model);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("Index", redirectResult.ActionName);
            Assert.AreEqual("StandardError", redirectResult.ControllerName);
        }

        // Helper method to setup mocked HTTP responses
        private void SetupMockResponses(bool isUnclassified, HttpStatusCode httpStatusCode)
        {
            var responseContent = isUnclassified
                ? "{ \"runId\": 1, \"runClassificationId\": 3, \"runClassificationStatus\": \"UNCLASSIFIED\", \"financialYear\": \"2025-26\" }"
                : "{ \"runId\": 1, \"runClassificationId\": 7, \"runName\": \"Test Calculator1702\", \"runClassificationStatus\": \"UNCLASSIFIED\", \"financialYear\": \"2025-26\" }";

            mockMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = httpStatusCode,
                    Content = new StringContent(responseContent)
                });

            mockMessageHandler
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.Is<HttpRequestMessage>(k => k.RequestUri != null && k.RequestUri.ToString().Contains("Financial")),
                   ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage
               {
                   StatusCode = httpStatusCode,
                   Content = new StringContent("{ \"financialYear\": \"2025-26\", \"classifications\": [{\"id\":4,\"status\":\"TEST RUN\"},{\"id\":8,\"status\":\"INITIAL RUN\"}]}")
               });
        }
    }
}