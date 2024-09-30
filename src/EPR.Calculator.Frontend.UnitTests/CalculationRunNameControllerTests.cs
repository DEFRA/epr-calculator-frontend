using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class CalculationRunNameControllerTests
    {
        [TestMethod]
        public void CalculationRunNameController_View_Test()
        {
            var controller = new CalculationRunNameController();
            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
        }

        [TestMethod]
        public void CalculateRun_ReturnsCorrectView_WithCalculationName()
        {
            // Arrange
            var controller = new CalculationRunNameController();
            var calculationName = "Test Calculation";

            // Act
            var result = controller.CalculateRun(calculationName) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunConfirmationIndex, result.ViewName);
            Assert.AreEqual(calculationName, controller.ViewBag.CalculationName);
        }

        [TestMethod]
        public void CalculateRun_ReturnsCorrectView_WithoutCalculationName()
        {
            // Arrange
            var controller = new CalculationRunNameController();

            // Act
            var result = controller.CalculateRun(null) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunConfirmationIndex, result.ViewName);
            Assert.IsNull(controller.ViewBag.CalculationName);
        }
    }
}