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
    public class ClassifyCalculationRunControllerTests
    {
        public ClassifyCalculationRunControllerTests()
        {
            this.Fixture = new Fixture();
            this.MockHttpContext = new Mock<HttpContext>();
            this.MockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
        }

        private Fixture Fixture { get; } = new Fixture();

        private Mock<HttpContext> MockHttpContext { get; init; }

        [TestMethod]
        public void Index_ReturnsView_WithClassifyCalculationView()
        {
            var controller = new ClassifyCalculationRunController();
            controller.ControllerContext = new ControllerContext { HttpContext = MockHttpContext.Object };
            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunClassification, result.ViewName);
        }

        [TestMethod]
        public void Index_ReturnsViewResult_WithClassifyCalculationViewModel()
        {
            // Arrange
            var expectedViewName = ViewNames.CalculationRunClassification;
            var expectedViewModel = new ClassifyCalculationViewModel(99, "Calculation run 99", "2024-25", 240008, true, "1 December 2024 at 12:09");

            var controller = new ClassifyCalculationRunController();
            controller.ControllerContext = new ControllerContext { HttpContext = MockHttpContext.Object };

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedViewName, result.ViewName);
            var model = result.Model as ClassifyCalculationViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(expectedViewModel.CalcName, model.CalcName);
            Assert.AreEqual(expectedViewModel.FinancialYear, model.FinancialYear);
            Assert.AreEqual(expectedViewModel.CalculationId, model.CalculationId);
            Assert.AreEqual(expectedViewModel.ClassificationType, model.ClassificationType);
            Assert.AreEqual(expectedViewModel.RunDate, model.RunDate);
        }
    }
}