namespace EPR.Calculator.Frontend.UnitTests.Controllers
{
    using System.Net;
    using System.Net.Http;
    using AutoFixture;
    using EPR.Calculator.Frontend.Constants;
    using EPR.Calculator.Frontend.Controllers;
    using EPR.Calculator.Frontend.Models;
    using EPR.Calculator.Frontend.UnitTests.HelpersTest;
    using EPR.Calculator.Frontend.UnitTests.Mocks;
    using EPR.Calculator.Frontend.ViewModels;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Identity.Web;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Moq.Protected;
    using Newtonsoft.Json;

    [TestClass]
    public class ClassifyingCalculationRunScenario2ControllerTests
    {
        private readonly IConfiguration _configuration = ConfigurationItems.GetConfigurationValues();
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private Mock<ILogger<ClassifyingCalculationRunScenario2Controller>> _mockLogger;
        private Mock<ITokenAcquisition> _mockTokenAcquisition;
        private TelemetryClient _telemetryClient;
        private Mock<HttpContext> _mockHttpContext;
        private ClassifyingCalculationRunScenario2Controller _controller;

        public ClassifyingCalculationRunScenario2ControllerTests()
        {
            this.Fixture = new Fixture();
            this.MockHttpContext = new Mock<HttpContext>();
            this.MockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);

            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<ClassifyingCalculationRunScenario2Controller>>();
            _mockTokenAcquisition = new Mock<ITokenAcquisition>();
            _telemetryClient = new TelemetryClient();
            _mockHttpContext = new Mock<HttpContext>();
        }

        private Fixture Fixture { get; }

        private Mock<HttpContext> MockHttpContext { get; }

        [TestInitialize]
        public void Setup()
        {
            _controller = new ClassifyingCalculationRunScenario2Controller(
               _configuration,
               _mockTokenAcquisition.Object,
               _telemetryClient)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _mockHttpContext.Object
                }
            };
        }

        [TestMethod]
        public async Task IndexAsync_ReturnsView_WhenRunExists()
        {
            // Arrange
            CalculatorRunDto calculatorRunDto = MockData.GetCalculatorRun();
            var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, calculatorRunDto);
            var client = new HttpClient(mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);
            _mockHttpContext.Setup(ctx => ctx.User.Identity.Name).Returns("TestUser");

            // Act
            var result = _controller.Index(calculatorRunDto.RunId);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(ViewNames.ClassifyingCalculationRunScenario2Index, viewResult.ViewName);

            var model = viewResult.Model as ClassifyCalculationRunScenerio2ViewModel;
            Assert.IsNotNull(model);
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