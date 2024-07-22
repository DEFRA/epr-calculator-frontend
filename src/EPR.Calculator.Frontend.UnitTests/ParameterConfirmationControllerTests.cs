using EPR.Calculator.Frontend.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.UnitTests
{
    public class ParameterConfirmationControllerTests
    {
        [TestMethod]
        public void ParameterConfirmationController_View_Test()
        {
            var controller = new ParameterConfirmationController();
            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ViewName);
        }
    }
}
