using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class PaymentCalculatorControllerTests
    {
        [TestMethod]
        public void Index_ReturnsViewResult_WithCorrectViewName()
        {
            // Arrange
            var controller = new PaymentCalculatorController();

            // Assert
            var viewResult = controller.BillingFileSuccess() as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(ViewNames.BillingConfirmationSuccess, viewResult.ViewName);
        }
    }
}