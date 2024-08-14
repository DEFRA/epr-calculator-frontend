using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class CalculationRunDetailsControllerTests
    {
        [TestMethod]
        public void CalculationRunDetailsController_View_Test()
        {
            var controller = new CalculationRunDetailsController();
            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculatorRunDetailsIndex, result.ViewName);
        }
    }
}
