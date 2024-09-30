using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class CalculationRunConfirmationControllerTests
    {
        [TestMethod]
        public void CalculationRunConfirmationController_View_Test()
        {
            var controller = new CalculationRunConfirmationController();
            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunConfirmationIndex, result.ViewName);
        }
    }
}