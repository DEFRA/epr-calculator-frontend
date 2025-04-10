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
    [TestClass]
    public class CalculationRunDeleteControllerTests
    {
        private readonly IConfiguration _configuration = ConfigurationItems.GetConfigurationValues();
        private Mock<IHttpClientFactory> _mockClientFactory;
        private Mock<ILogger<CalculationRunDeleteController>> _mockLogger;
        private TelemetryClient _mockTelemetryClient;

        public CalculationRunDeleteControllerTests()
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
            _mockLogger = new Mock<ILogger<CalculationRunDeleteController>>();
            _mockTelemetryClient = new TelemetryClient();
        }

        [TestMethod]
        public void CalculationRunDelete_Success_View_Test()
        {
            // Arrange
            int runId = this.Fixture.Create<int>();

            var controller = new CalculationRunDeleteController(_configuration, _mockClientFactory.Object,
                new Mock<ITokenAcquisition>().Object, _mockTelemetryClient);
            controller.ControllerContext.HttpContext = this.MockHttpContext.Object;

            // Act
            var result = controller.Index(runId) as ViewResult;

            // Assert
            var resultModel = result.Model as CalculationRunDeleteViewModel;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunDeleteIndex, result.ViewName);
            Assert.AreEqual(runId, resultModel.CalculatorRunStatusData.RunId);
        }

        [TestMethod]
        public void CalculationRunDelete_Confirmation_Success_View_Test()
        {
            // Arrange
            var controller = new CalculationRunDeleteController(_configuration, _mockClientFactory.Object,
                new Mock<ITokenAcquisition>().Object, _mockTelemetryClient);
            controller.ControllerContext.HttpContext = this.MockHttpContext.Object;

            // Act
            var result = controller.DeleteConfirmationSuccess() as ViewResult;

            // Assert
            var resultModel = result.Model;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunDeleteConfirmationSuccess, result.ViewName);
            Assert.IsTrue(typeof(string) == resultModel.GetType());
        }
    }
}
