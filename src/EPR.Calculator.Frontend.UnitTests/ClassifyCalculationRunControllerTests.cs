using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests
{
    internal class ClassifyCalculationRunControllerTests
    {
        private readonly IConfiguration _configuration = ConfigurationItems.GetConfigurationValues();
        private Mock<IHttpClientFactory> _mockClientFactory;
        private Mock<ILogger<ClassifyCalculationRunController>> _mockLogger;
        private TelemetryClient _mockTelemetryClient;

        public ClassifyCalculationRunControllerTests()
        {
            this.Fixture = new Fixture();
            this.MockHttpContext = new Mock<HttpContext>();
            this.MockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
        }

        private Fixture Fixture { get; init; }

        private Mock<HttpContext> MockHttpContext { get; init; }

        [TestInitialize]
        public void Setup()
        {
            _mockClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<ClassifyCalculationRunController>>();
            _mockTelemetryClient = new TelemetryClient();
        }

        [TestMethod]
        public void ClassifyCalculationRun_Success_View_Test()
        {
            // Arrange

            var controller = new ClassifyCalculationRunController(_configuration, _mockClientFactory.Object,
                _mockLogger.Object, new Mock<ITokenAcquisition>().Object, _mockTelemetryClient);
            controller.ControllerContext.HttpContext = this.MockHttpContext.Object;

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            var resultModel = result.Model as ClassifyCalculationViewModel;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunClassification, result.ViewName);
        }
    }
}
