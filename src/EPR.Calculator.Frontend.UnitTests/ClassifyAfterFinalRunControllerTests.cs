using System.Net;
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
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class ClassifyAfterFinalRunControllerTests
    {
        private readonly IConfiguration _configuration = ConfigurationItems.GetConfigurationValues();
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private Mock<ILogger<ClassifyAfterFinalRunController>> _mockLogger;
        private Mock<ITokenAcquisition> _mockTokenAcquisition;
        private TelemetryClient _telemetryClient;
        private ClassifyAfterFinalRunController _controller;
        private Mock<IApiService> _mockApiService;

        private Fixture Fixture { get; set; }

        [TestInitialize]
        public void Setup()
        {
            Fixture = new Fixture();

            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<ClassifyAfterFinalRunController>>();
            _mockTokenAcquisition = new Mock<ITokenAcquisition>();
            _telemetryClient = new TelemetryClient();
            _mockApiService = new Mock<IApiService>();

            var mockSession = new MockHttpSession();
            mockSession.SetString(SessionConstants.FinancialYear, "2024-25");

            var context = new DefaultHttpContext
            {
                Session = mockSession
            };

            _controller = new ClassifyAfterFinalRunController(
                _configuration,
                _mockTokenAcquisition.Object,
                _telemetryClient,
                _mockApiService.Object,
                new Mock<ICalculatorRunDetailsService>().Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = context
                }
            };
        }

        [TestMethod]
        public async Task IndexAsync_ReturnsView_WhenRunExists()
        {
            // Arrange
            var runId = 123;
            var financialYear = "2024-25";
            var mockResponseDto = new FinancialYearClassificationResponseDto
            {
                FinancialYear = "2025-26",
                Classifications = new List<CalculatorRunClassificationDto>
    {
        new CalculatorRunClassificationDto { Id = 9, Status = "INTERIM RE-CALCULATION RUN" },
        new CalculatorRunClassificationDto { Id = 4, Status = "TEST RUN" }
    },
                ClassifiedRuns = new List<ClassifiedCalculatorRunDto>
    {
        new ClassifiedCalculatorRunDto
        {
            RunId = 18,
            CreatedAt = DateTime.Parse("2025-09-07T10:41:11.49982"),
            RunName = "Scenario2_Initial",
            RunClassificationId = 7,
            UpdatedAt = DateTime.Parse("2025-09-08T10:41:11.49982")
        },
        new ClassifiedCalculatorRunDto
        {
            RunId = 20,
            CreatedAt = DateTime.Parse("2025-09-08T10:52:40.143975"),
            RunName = "FinalRe-calcCompletedTest",
            RunClassificationId = 13,
            UpdatedAt = DateTime.Parse("2025-09-08T10:54:57.9122624")
        },
        new ClassifiedCalculatorRunDto
        {
            RunId = 21,
            CreatedAt = DateTime.Parse("2025-09-08T10:52:40.155863"),
            RunName = "RunfinalRunCompletedTest",
            RunClassificationId = 14,
            UpdatedAt = DateTime.Parse("2025-09-08T10:54:57.9122624")
        }
    }
            };

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(mockResponseDto))
            };

            _mockApiService
                .Setup(api => api.CallApi(
                    It.IsAny<HttpContext>(),
                    HttpMethod.Get,
                    It.IsAny<System.Uri>(),
                    It.IsAny<string>(),
                    null))
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _controller.IndexAsync(runId);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(ViewNames.ClassifyAfterFinalRunIndex, viewResult.ViewName);

            var model = viewResult.Model as ClassifyAfterFinalRunViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(runId, model.CalculatorRunStatus.RunId);
        }

        private static Mock<HttpMessageHandler> CreateMockHttpMessageHandler(HttpStatusCode statusCode, object content)
        {
            var mockHandler = new Mock<HttpMessageHandler>();

            mockHandler
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

            return mockHandler;
        }
    }
}