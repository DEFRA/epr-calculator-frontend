using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class CalculationRunDeleteControllerTests
    {
        public CalculationRunDeleteControllerTests()
        {
            this.Fixture = new Fixture();
            this.MockHttpContext = new Mock<HttpContext>();
            this.MockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
        }

        private Fixture Fixture { get; init; }

        private Mock<HttpContext> MockHttpContext { get; init; }

        [TestMethod]
        public void CalculationRunDelete_Success_View_Test()
        {
            // Arrange
            int runId = this.Fixture.Create<int>();

            var controller = new CalculationRunDeleteController();
            controller.ControllerContext.HttpContext = this.MockHttpContext.Object;

            // Act
            var result = controller.Index(runId) as ViewResult;

            // Assert
            var resultModel = result.Model as CalculationRunDeleteViewModel;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunDeleteIndex, result.ViewName);
            Assert.AreEqual(runId, resultModel.Data.RunId);
        }
    }
}
