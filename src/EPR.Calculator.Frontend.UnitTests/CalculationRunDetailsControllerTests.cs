using System.Net;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class CalculationRunDetailsControllerTests
    {
        private readonly IConfiguration _configuration = ConfigurationItems.GetConfigurationValues();
        private Mock<IHttpClientFactory> _mockClientFactory;
        private Mock<ILogger<CalculationRunDetailsController>> _mockLogger;

        [TestInitialize]
        public void Setup()
        {
            _mockClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<CalculationRunDetailsController>>();
        }

        [TestMethod]
        public async Task IndexAsync_ReturnsView_WhenApiCallIsSuccessful()
        {
            // Arrange
            var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, MockData.GetCalculationRuns());
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var controller = new CalculationRunDetailsController(_configuration, _mockClientFactory.Object, _mockLogger.Object);
            int runId = 1;
            string calcName = "TestCalc";

            // Act
            var result = await controller.IndexAsync(runId, calcName) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunDetailsIndex, result.ViewName);
            var model = result.Model as CalculatorRunStatusUpdateDto;
            Assert.IsNotNull(model);
            Assert.AreEqual(runId, model.RunId);
            Assert.AreEqual((int)RunClassification.DELETED, model.ClassificationId);
            Assert.AreEqual(calcName, model.CalcName);
        }

        [TestMethod]
        public async Task IndexAsync_RedirectsToError_WhenApiCallFails()
        {
            // Arrange
            var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.BadRequest, MockData.GetCalculationRuns());
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var controller = new CalculationRunDetailsController(_configuration, _mockClientFactory.Object, _mockLogger.Object);
            int runId = 1;
            string calcName = "TestCalc";

            // Act
            var result = await controller.IndexAsync(runId, calcName) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual(CommonUtil.GetControllerName(typeof(StandardErrorController)), result.ControllerName);
        }

        [TestMethod]
        public void DeleteCalcDetails_ReturnsView_WhenApiCallIsSuccessful()
        {
            // Arrange
            var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, MockData.GetCalculationRuns());
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var controller = new CalculationRunDetailsController(_configuration, _mockClientFactory.Object, _mockLogger.Object);
            int runId = 1;

            // Act
            var result = controller.DeleteCalcDetails(runId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.DeleteConfirmation, result.ViewName);
        }

        [TestMethod]
        public async Task IndexAsync_GetCalculationDetailsResponseIsNull_ShouldLogErrorAndRedirect()
        {
            // Arrange
            var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.InternalServerError, MockData.GetCalculationRuns());
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var controller = new CalculationRunDetailsController(_configuration, _mockClientFactory.Object, _mockLogger.Object);
            int runId = 1;
            string calcName = "TestCalc";

            // Act
            var result = await controller.IndexAsync(runId, calcName) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        private static Mock<HttpMessageHandler> CreateMockHttpMessageHandler(HttpStatusCode statusCode, object content)
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
                    StatusCode = statusCode,
                    Content = new StringContent(JsonConvert.SerializeObject(content))
                });

            return mockHttpMessageHandler;
        }
    }
}