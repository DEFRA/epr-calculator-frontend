using System.Configuration;
using System.Net;
using System.Reflection;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;

namespace EPR.Calculator.Frontend.UnitTests.Services
{
    [TestClass]
    public class BillingInstructionsApiServiceTests
    {
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private IConfiguration _mockConfiguration = ConfigurationItems.GetConfigurationValues();
        private BillingInstructionsApiService _service;

        public BillingInstructionsApiServiceTests()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _service = new BillingInstructionsApiService(_mockHttpClientFactory.Object, _mockConfiguration);
        }

        [TestMethod]
        public async Task PutAcceptRejectBillingInstructions_Success_ReturnsTrue()
        {
            // Arrange
            var calculationRunId = 1;
            var requestDto = new ProducerBillingInstructionsHttpPutRequestDto
            {
                OrganisationIds = new[] { 1, 2, 3 },
                Status = "Accepted",
                AuthorizationToken = "Bearer token"
            };

            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            var httpClient = new HttpClient(mockHandler.Object);
            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var result = await _service.PutAcceptRejectBillingInstructions(calculationRunId, requestDto);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task PutAcceptRejectBillingInstructions_Failure_ReturnsFalse()
        {
            // Arrange
            var calculationRunId = 1;
            var requestDto = new ProducerBillingInstructionsHttpPutRequestDto
            {
                OrganisationIds = new[] { 1, 2, 3 },
                Status = "Rejected",
                ReasonForRejection = "Invalid data",
                AuthorizationToken = "Bearer token"
            };

            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            var httpClient = new HttpClient(mockHandler.Object);
            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var result = await _service.PutAcceptRejectBillingInstructions(calculationRunId, requestDto);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GetConfigSetting_NullOrWhitespace_ThrowsConfigurationErrorsException()
        {
            // Arrange
            var service = new BillingInstructionsApiService(_mockHttpClientFactory.Object, _mockConfiguration);

            // Act
            var method = service.GetType()
                .GetMethod("GetConfigSetting", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

            var ex = Assert.ThrowsException<TargetInvocationException>(() =>
                method.Invoke(service, new object[] { "SomeSection", "SomeKey" }));

            // Assert
            Assert.IsInstanceOfType(ex.InnerException, typeof(ConfigurationErrorsException));
            StringAssert.Contains(ex.InnerException!.Message, "SomeSection:SomeKey is null or empty");
        }
    }
}