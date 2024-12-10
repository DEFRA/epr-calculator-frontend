using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class CalculationRunErrorControllerTests
    {
        private Mock<ITempDataDictionary> _tempDataMock;

        public CalculationRunErrorControllerTests()
        {
            this.Fixture = new Fixture();
            this.MockHttpContext = new Mock<HttpContext>();
            this.MockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
        }

        private Fixture Fixture { get; init; }

        private Mock<HttpContext> MockHttpContext { get; init; }

        [TestMethod]
        public void CalculationRunErrorController_View_Test()
        {
            // Arrange
            _tempDataMock = new Mock<ITempDataDictionary>();
            _tempDataMock.Setup(tempData => tempData["ErrorMessage"]).Returns("Test error message");
            var controller = new CalculationRunErrorController();
            controller.TempData = _tempDataMock.Object;
            controller.ControllerContext.HttpContext = this.MockHttpContext.Object;
            var error = new ErrorDto() { Message = "Error" };

            // Act
            var result = controller.Index(error) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunErrorIndex, result.ViewName);
        }
    }
}
