using EPR.Calculator.Frontend.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class DefaultParametersControllerTests
    {
        [TestMethod]
        public void DefaultParameterController_View_Test()
        {
            var controller = new DefaultParametersController();
            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
        }
    }
}
